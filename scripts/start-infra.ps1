param ([string]$environment)
. "$PSScriptRoot/get-environment.ps1"
$environment = Get-Environment $environment

# bucket do Terraform
Write-Host "Creating bucket 'fiap-mechanics-tf'..."
aws s3 mb s3://fiap-mechanics-tf --region us-east-1

# subir infraestrutura pelo Terraform
Write-Host "Deploying infrastructure for environment '$environment'..."

$ENV:TF_VAR_environment = $environment
terraform -chdir="./infra" init -backend-config="key=$environment.tfstate" -reconfigure
terraform -chdir="./infra" apply -auto-approve

Write-Host "Configuring..."

aws eks update-kubeconfig --name fiap-mechanics-$environment-cluster --region us-east-1

helm repo add external-secrets https://charts.external-secrets.io
helm repo update
helm upgrade --install external-secrets external-secrets/external-secrets --namespace external-secrets --create-namespace

kubectl create secret generic aws-credentials `
  --namespace external-secrets `
  --from-literal=access-key-id="$(aws configure get aws_access_key_id)" `
  --from-literal=secret-access-key="$(aws configure get aws_secret_access_key)" `
  --from-literal=session-token="$(aws configure get aws_session_token)"

Write-Host -ForegroundColor Green "Infrastructure for environment '$environment' has been deployed."
Write-Host "Wait for all pods in 'external-secrets' namespace to be in 'Running' status before deploying the application."
Write-Host "You can check the status with: kubectl get pods -n external-secrets"