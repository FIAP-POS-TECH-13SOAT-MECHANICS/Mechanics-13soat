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

helm repo add external-secrets https://charts.external-secrets.io
helm repo add metrics-server https://kubernetes-sigs.github.io/metrics-server
helm repo add jouve https://jouve.github.io/charts
helm repo add nginx https://kubernetes.github.io/ingress-nginx
helm repo update

Write-Host
Write-Host -ForegroundColor Yellow "Installing charts..."
Write-Host "Already installed charts will be ignored."
# utilizar "install" para ignorar recursos já instalados

helm install external-secrets external-secrets/external-secrets --namespace external-secrets --create-namespace

helm install ingress-nginx nginx/ingress-nginx --namespace ingress-nginx --create-namespace

helm install metrics-server metrics-server/metrics-server --namespace kube-system  `
  --set args[0]=--kubelet-insecure-tls `
  --set args[1]=--kubelet-preferred-address-types=InternalIP

$smtpAuth = aws secretsmanager get-secret-value --secret-id fiap-mechanics-$environment-email --query SecretString --output text | ConvertFrom-Json
helm install mailpit jouve/mailpit `
  --set mailpit.smtp.authFile.enabled="true" `
  --set mailpit.smtp.authFile.htpasswd="$($smtpAuth.userName):$($smtpAuth.password)"

kubectl create secret generic aws-credentials `
  --namespace external-secrets `
  --from-literal=access-key-id="$(aws configure get aws_access_key_id)" `
  --from-literal=secret-access-key="$(aws configure get aws_secret_access_key)" `
  --from-literal=session-token="$(aws configure get aws_session_token)" `
  --dry-run=client `
  --output yaml | kubectl apply -f -

Write-Host
Write-Host -ForegroundColor Yellow "Waiting for External Secrets Operator to be ready..."
kubectl wait --for=condition=Ready pod --all -n external-secrets --timeout=120s

Write-Host
Write-Host -ForegroundColor Green "Infrastructure for environment '$environment' has been applied."
