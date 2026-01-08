<#
.SYNOPSIS
    Initialize AWS infrastructure for FIAP Mechanics application
    
.DESCRIPTION
    Idempotent script that safely creates or updates: 
    - Terraform backend (S3 + DynamoDB)
    - AWS infrastructure via Terraform (EKS, RDS, ECR, VPC)
    - Kubernetes add-ons (External Secrets, Metrics Server, Mailpit)
    
    Safe to run multiple times - skips existing resources. 
    Designed for GitHub Actions but works locally too.
    
.PARAMETER environment
    Target environment:  dev, stg, or prod
    If not provided, will prompt interactively
    
.NOTES
    Credentials:  Reads from environment variables (set by aws-actions/configure-aws-credentials)
    Required env vars: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN
    
.EXAMPLE
    .\initialize-infrastructure.ps1 dev
    .\initialize-infrastructure.ps1 prod
#>

param (
    [Parameter(Mandatory=$false)]
    [ValidateSet("dev", "stg", "prod", "")]
    [string]$environment
)

#region Configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"  # Speeds up AWS CLI calls
#endregion

#region Helper Functions
function Write-Step {
    param([string]$Message)
    Write-Host "`n===> $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "  ✅ $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "  ℹ️  $Message" -ForegroundColor Yellow
}

function Write-Error-Message {
    param([string]$Message)
    Write-Host "  ❌ $Message" -ForegroundColor Red
}

function Test-AwsResource {
    param(
        [string]$Command,
        [string]$ResourceName
    )
    
    try {
        Invoke-Expression "$Command 2>&1 | Out-Null"
        return ($LASTEXITCODE -eq 0)
    }
    catch {
        return $false
    }
}

function Wait-ForResource {
    param(
        [scriptblock]$TestScript,
        [string]$ResourceName,
        [int]$TimeoutSeconds = 180,
        [int]$IntervalSeconds = 10
    )
    
    $elapsed = 0
    while ($elapsed -lt $TimeoutSeconds) {
        if (& $TestScript) {
            return $true
        }
        Start-Sleep -Seconds $IntervalSeconds
        $elapsed += $IntervalSeconds
        if ($elapsed % 30 -eq 0) {
            Write-Info "Still waiting for $ResourceName...  ($elapsed/$TimeoutSeconds seconds)"
        }
    }
    return $false
}
#endregion

#region Import Environment Helper
. "$PSScriptRoot/get-environment.ps1"
$environment = Get-Environment $environment
Write-Host "`n🚀 Starting infrastructure setup for environment:  " -NoNewline
Write-Host $environment -ForegroundColor Green
#endregion

#region Validate Prerequisites
Write-Step "Validating prerequisites"

# Check required tools
$requiredTools = @("aws", "terraform", "helm", "kubectl")
foreach ($tool in $requiredTools) {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) {
        Write-Error-Message "Required tool not found: $tool"
        Write-Host "`nPlease install $tool and try again." -ForegroundColor Yellow
        exit 1
    }
}
Write-Success "All required tools installed (aws, terraform, helm, kubectl)"

# Validate AWS credentials from environment variables
if ([string]::IsNullOrWhiteSpace($env:AWS_ACCESS_KEY_ID)) {
    Write-Error-Message "AWS_ACCESS_KEY_ID environment variable not set"
    Write-Host "`nIn GitHub Actions, ensure 'aws-actions/configure-aws-credentials' runs before this script." -ForegroundColor Yellow
    Write-Host "Locally, set environment variables or run 'aws configure'." -ForegroundColor Yellow
    exit 1
}

# Test AWS credentials
try {
    $identity = aws sts get-caller-identity --output json 2>$null | ConvertFrom-Json
    Write-Success "AWS credentials valid (Account: $($identity.Account))"
}
catch {
    Write-Error-Message "AWS credentials are invalid or expired"
    exit 1
}
#endregion

#region Setup Terraform Backend
Write-Step "Setting up Terraform backend"

# DynamoDB table for state locking
$dynamoExists = Test-AwsResource -Command "aws dynamodb describe-table --table-name fiap-mechanics-tf" -ResourceName "DynamoDB"

if ($dynamoExists) {
    Write-Success "DynamoDB table 'fiap-mechanics-tf' already exists"
}
else {
    Write-Info "Creating DynamoDB table 'fiap-mechanics-tf'..."
    aws dynamodb create-table `
        --table-name fiap-mechanics-tf `
        --attribute-definitions AttributeName=LockID,AttributeType=S `
        --key-schema AttributeName=LockID,KeyType=HASH `
        --billing-mode PAY_PER_REQUEST `
        --output json 2>&1 | Out-Null
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Message "Failed to create DynamoDB table"
        exit 1
    }
    
    # Wait for table to be active
    Write-Info "Waiting for DynamoDB table to become active..."
    $tableActive = Wait-ForResource -TimeoutSeconds 60 -IntervalSeconds 5 -ResourceName "DynamoDB table" -TestScript {
        $status = aws dynamodb describe-table --table-name fiap-mechanics-tf --query "Table.TableStatus" --output text 2>$null
        return ($status -eq "ACTIVE")
    }
    
    if ($tableActive) {
        Write-Success "DynamoDB table created and active"
    }
    else {
        Write-Error-Message "DynamoDB table creation timeout"
        exit 1
    }
}

# S3 bucket for state storage
$s3Exists = Test-AwsResource -Command "aws s3 ls s3://fiap-mechanics-tf" -ResourceName "S3 bucket"

if ($s3Exists) {
    Write-Success "S3 bucket 'fiap-mechanics-tf' already exists"
}
else {
    Write-Info "Creating S3 bucket 'fiap-mechanics-tf'..."
    aws s3 mb s3://fiap-mechanics-tf --region us-east-1 2>&1 | Out-Null
    
    if ($LASTEXITCODE -ne 0) {
        # Double-check if bucket exists (race condition)
        $s3Exists = Test-AwsResource -Command "aws s3 ls s3://fiap-mechanics-tf" -ResourceName "S3 bucket"
        if ($s3Exists) {
            Write-Info "Bucket was created by another process"
        }
        else {
            Write-Error-Message "Failed to create S3 bucket"
            exit 1
        }
    }
    else {
        Write-Success "S3 bucket created"
        Write-Info "Waiting for S3 propagation (15 seconds)..."
        Start-Sleep -Seconds 15
    }
}

# Enable versioning (idempotent)
aws s3api put-bucket-versioning --bucket fiap-mechanics-tf --versioning-configuration Status=Enabled 2>&1 | Out-Null
Write-Success "Terraform backend ready"
#endregion

#region Apply Terraform Infrastructure
Write-Step "Applying Terraform infrastructure"

Write-Info "Initializing Terraform..."
terraform -chdir="./infra" init `
    -backend-config="key=$environment.tfstate" `
    -reconfigure `
    -upgrade 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error-Message "Terraform init failed"
    exit 1
}
Write-Success "Terraform initialized"

Write-Info "Applying infrastructure (this may take 10-15 minutes)..."
Write-Host "  Creating:  VPC, EKS Cluster, RDS Database, ECR Repository, Secrets..." -ForegroundColor Gray

terraform -chdir="./infra" apply `
    -var="environment=$environment" `
    -auto-approve `
    -compact-warnings 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error-Message "Terraform apply failed"
    exit 1
}
Write-Success "Infrastructure applied successfully"

# Capture outputs
$ecrRepo = terraform -chdir="./infra" output -raw cr_repository_url 2>$null
$clusterName = "fiap-mechanics-$environment-cluster"
Write-Info "ECR Repository:  $ecrRepo"
Write-Info "EKS Cluster: $clusterName"
#endregion

#region Configure Kubernetes
Write-Step "Configuring Kubernetes cluster"

# Wait for cluster to exist
Write-Info "Verifying EKS cluster exists..."
$clusterReady = Wait-ForResource -TimeoutSeconds 300 -IntervalSeconds 20 -ResourceName "EKS cluster" -TestScript {
    Test-AwsResource -Command "aws eks describe-cluster --name $clusterName" -ResourceName "EKS"
}

if (-not $clusterReady) {
    Write-Error-Message "EKS cluster '$clusterName' not found"
    exit 1
}
Write-Success "EKS cluster verified"

# Configure kubectl
Write-Info "Configuring kubectl..."
aws eks update-kubeconfig --name $clusterName --region us-east-1 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error-Message "Failed to configure kubectl"
    exit 1
}
Write-Success "kubectl configured"

# Wait for nodes
Write-Info "Waiting for cluster nodes..."
$nodesReady = Wait-ForResource -TimeoutSeconds 300 -IntervalSeconds 15 -ResourceName "cluster nodes" -TestScript {
    $nodes = kubectl get nodes --no-headers 2>$null
    return (($nodes | Measure-Object).Count -gt 0)
}

if ($nodesReady) {
    $nodeCount = (kubectl get nodes --no-headers 2>$null | Measure-Object).Count
    Write-Success "Cluster accessible ($nodeCount nodes ready)"
}
else {
    Write-Error-Message "No nodes available after waiting"
    exit 1
}
#endregion

#region Setup Helm Repositories
Write-Step "Configuring Helm repositories"

$helmRepos = @(
    @{Name="external-secrets"; URL="https://charts.external-secrets.io"},
    @{Name="metrics-server"; URL="https://kubernetes-sigs.github.io/metrics-server"},
    @{Name="jouve"; URL="https://jouve.github.io/charts"}
)

foreach ($repo in $helmRepos) {
    $repoList = helm repo list 2>$null
    if ($repoList -match "^$($repo.Name)\s") {
        Write-Info "Helm repo '$($repo.Name)' already added"
    }
    else {
        helm repo add $repo. Name $repo.URL 2>&1 | Out-Null
        Write-Info "Added Helm repo '$($repo.Name)'"
    }
}

helm repo update 2>&1 | Out-Null
Write-Success "Helm repositories configured"
#endregion

#region Install Kubernetes Add-ons
Write-Step "Installing Kubernetes add-ons"

# External Secrets Operator
Write-Info "Installing External Secrets Operator..."
helm upgrade --install external-secrets external-secrets/external-secrets `
    --namespace external-secrets `
    --create-namespace `
    --wait `
    --timeout 5m 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error-Message "Failed to install External Secrets Operator"
    exit 1
}
Write-Success "External Secrets Operator installed"

# Metrics Server
Write-Info "Installing Metrics Server..."
helm upgrade --install metrics-server metrics-server/metrics-server `
    --namespace kube-system `
    --set args[0]=--kubelet-insecure-tls `
    --set args[1]=--kubelet-preferred-address-types=InternalIP `
    --wait `
    --timeout 5m 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error-Message "Failed to install Metrics Server"
    exit 1
}
Write-Success "Metrics Server installed"

# Mailpit (requires secret from Terraform)
Write-Info "Retrieving email credentials from Secrets Manager..."
$secretId = "fiap-mechanics-$environment-email"

$secretReady = Wait-ForResource -TimeoutSeconds 60 -IntervalSeconds 10 -ResourceName "email secret" -TestScript {
    Test-AwsResource -Command "aws secretsmanager describe-secret --secret-id $secretId" -ResourceName "Secret"
}

if (-not $secretReady) {
    Write-Error-Message "Secret '$secretId' not found"
    exit 1
}

$smtpAuthJson = aws secretsmanager get-secret-value --secret-id $secretId --query SecretString --output text 2>$null
$smtpAuth = $smtpAuthJson | ConvertFrom-Json

Write-Info "Installing Mailpit..."
helm upgrade --install mailpit jouve/mailpit `
    --set mailpit.smtp.authFile. enabled="true" `
    --set mailpit.smtp.authFile.htpasswd="$($smtpAuth.userName):$($smtpAuth.password)" `
    --wait `
    --timeout 5m 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error-Message "Failed to install Mailpit"
    exit 1
}
Write-Success "Mailpit installed"
#endregion

#region Configure AWS Credentials Secret
Write-Step "Configuring AWS credentials for External Secrets"

# Read from environment variables (set by aws-actions/configure-aws-credentials)
$awsAccessKeyId = $env:AWS_ACCESS_KEY_ID
$awsSecretAccessKey = $env:AWS_SECRET_ACCESS_KEY
$awsSessionToken = $env:AWS_SESSION_TOKEN

if ([string]::IsNullOrWhiteSpace($awsAccessKeyId) -or [string]::IsNullOrWhiteSpace($awsSecretAccessKey)) {
    Write-Error-Message "AWS credentials not available in environment variables"
    exit 1
}

Write-Info "Creating aws-credentials secret..."

# Create/update secret using kubectl apply (idempotent)
$secretYaml = @"
apiVersion: v1
kind: Secret
metadata:
  name: aws-credentials
  namespace: external-secrets
type: Opaque
stringData:
  access-key-id: "$awsAccessKeyId"
  secret-access-key: "$awsSecretAccessKey"
  session-token: "$awsSessionToken"
"@

$secretYaml | kubectl apply -f - 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Error-Message "Failed to create AWS credentials secret"
    exit 1
}
Write-Success "AWS credentials configured"
#endregion

#region Validate Installation
Write-Step "Validating installation"

Write-Info "Waiting for External Secrets Operator pods..."
$podsReady = Wait-ForResource -TimeoutSeconds 120 -IntervalSeconds 10 -ResourceName "External Secrets pods" -TestScript {
    $pods = kubectl get pods -n external-secrets --no-headers 2>$null
    if ($LASTEXITCODE -ne 0) { return $false }
    
    $running = ($pods | Where-Object { $_ -match "Running" } | Measure-Object).Count
    $total = ($pods | Measure-Object).Count
    
    return ($running -eq $total -and $total -gt 0)
}

if ($podsReady) {
    Write-Success "External Secrets Operator is ready"
}
else {
    Write-Info "External Secrets pods may still be starting (non-blocking)"
    kubectl get pods -n external-secrets 2>&1 | Out-Null
}
#endregion

#region Summary
Write-Host "`n" -NoNewline
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Infrastructure Setup Complete!  " -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Environment:       " -NoNewline -ForegroundColor Gray
Write-Host $environment -ForegroundColor White
Write-Host "  Cluster:          " -NoNewline -ForegroundColor Gray
Write-Host $clusterName -ForegroundColor White
Write-Host "  Region:           " -NoNewline -ForegroundColor Gray
Write-Host "us-east-1" -ForegroundColor White
Write-Host "  ECR Repository:   " -NoNewline -ForegroundColor Gray
Write-Host $ecrRepo -ForegroundColor White
Write-Host ""
Write-Host "  Installed Components:" -ForegroundColor Gray
Write-Host "    ✅ Terraform Backend (S3 + DynamoDB)" -ForegroundColor Green
Write-Host "    ✅ EKS Cluster with nodes" -ForegroundColor Green
Write-Host "    ✅ RDS Database" -ForegroundColor Green
Write-Host "    ✅ ECR Repository" -ForegroundColor Green
Write-Host "    ✅ External Secrets Operator" -ForegroundColor Green
Write-Host "    ✅ Metrics Server" -ForegroundColor Green
Write-Host "    ✅ Mailpit (SMTP)" -ForegroundColor Green
Write-Host ""
Write-Host "  Next Steps:" -ForegroundColor Gray
Write-Host "    1. Build and push Docker image to ECR" -ForegroundColor White
Write-Host "    2. Deploy application using Helm chart" -ForegroundColor White
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
#endregion

exit 0
