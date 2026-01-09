param ([string]$environment)
. "$PSScriptRoot/get-environment.ps1"
$environment = Get-Environment $environment

# back-end do Terraform
Write-Host -ForegroundColor Yellow "Creating DynamoDB 'fiap-mechanics-tf'..."
aws dynamodb create-table `
  --table-name fiap-mechanics-tf `
  --attribute-definitions AttributeName=LockID,AttributeType=S `
  --key-schema AttributeName=LockID,KeyType=HASH `
  --billing-mode PAY_PER_REQUEST | Out-Null

Write-Host -ForegroundColor Yellow "Creating bucket 'fiap-mechanics-tf'..."
aws s3 mb s3://fiap-mechanics-tf --region us-east-1 | Out-Null

# subir infraestrutura pelo Terraform
Write-Host
Write-Host -ForegroundColor Yellow "Updating infrastructure for environment '$environment'..."

terraform -chdir="./infra" init -backend-config="key=$environment.tfstate" -reconfigure
if ($LASTEXITCODE -ne 0) { exit 1 }

terraform -chdir="./infra" apply -var="environment=$environment" -auto-approve
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host
Write-Host -ForegroundColor Yellow "Configuring cluster..."

$clusterName = "fiap-mechanics-$environment-cluster"
aws eks update-kubeconfig --name $clusterName --region us-east-1

kubectl get nodes

helm repo add external-secrets https://charts.external-secrets.io
helm repo add metrics-server https://kubernetes-sigs.github.io/metrics-server
helm repo add jouve https://jouve.github.io/charts
helm repo update

Write-Host
Write-Host -ForegroundColor Yellow "Installing charts..."
helm install external-secrets external-secrets/external-secrets --namespace external-secrets --create-namespace

helm install metrics-server metrics-server/metrics-server --namespace kube-system  `
  --set args[0]=--kubelet-insecure-tls `
  --set args[1]=--kubelet-preferred-address-types=InternalIP

# aguardar secret estar disponível
$secretId = "fiap-mechanics-$environment-email"
$retries = 0
while ($retries -lt 12) {
    aws secretsmanager describe-secret --secret-id $secretId 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) { break }
    $retries++
    Start-Sleep -Seconds 10
}

$smtpAuth = aws secretsmanager get-secret-value --secret-id $secretId --query SecretString --output text | ConvertFrom-Json
helm install mailpit jouve/mailpit `
  --set mailpit.smtp. authFile.enabled="true" `
  --set mailpit.smtp.authFile.htpasswd="$($smtpAuth.userName):$($smtpAuth.password)"

# usar variáveis de ambiente (GitHub Actions)
kubectl create secret generic aws-credentials `
  --namespace external-secrets `
  --from-literal=access-key-id="$env:AWS_ACCESS_KEY_ID" `
  --from-literal=secret-access-key="$env: AWS_SECRET_ACCESS_KEY" `
  --from-literal=session-token="$env:AWS_SESSION_TOKEN" `
  --dry-run=client `
  --output yaml | kubectl apply -f -

Write-Host
Write-Host -ForegroundColor Yellow "Waiting for External Secrets Operator to be ready..."
kubectl wait --for=condition=Ready pod --all -n external-secrets --timeout=120s

Write-Host
Write-Host -ForegroundColor Green "Infrastructure for environment '$environment' has been applied."
