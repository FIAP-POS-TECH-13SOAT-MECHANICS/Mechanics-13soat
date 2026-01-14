param ([string]$environment)
. "$PSScriptRoot/get-environment.ps1"
$environment = Get-Environment $environment

# back-end do Terraform
$bucketName = "fiap-mechanics-tf-$(aws sts get-access-key-info --access-key-id $(aws configure get aws_access_key_id) --query Account --output text)"
Write-Host -ForegroundColor Yellow "Creating bucket '$bucketName'..."
aws s3 mb s3://$bucketName --region us-east-1 | Out-Null

# subir infraestrutura pelo Terraform
Write-Host
Write-Host -ForegroundColor Yellow "Updating infrastructure for environment '$environment'..."

terraform -chdir="./infra" init -backend-config="bucket=$bucketName" -backend-config="key=$environment.tfstate" -reconfigure
if ($LASTEXITCODE -ne 0) { exit 1 }

terraform -chdir="./infra" apply -var="environment=$environment" -auto-approve
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host
Write-Host -ForegroundColor Yellow "Configuring cluster..."

aws eks update-kubeconfig --name fiap-mechanics-$environment-cluster --region us-east-1
kubectl create secret generic aws-credentials `
  --namespace external-secrets `
  --from-literal=access-key-id="$(aws configure get aws_access_key_id)" `
  --from-literal=secret-access-key="$(aws configure get aws_secret_access_key)" `
  --from-literal=session-token="$(aws configure get aws_session_token)" `
  --dry-run=client `
  --output yaml | kubectl apply -f -

Write-Host
Write-Host -ForegroundColor Green "Infrastructure for environment '$environment' has been applied."
