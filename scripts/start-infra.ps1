param ([string]$environment)
. "$PSScriptRoot/get-environment.ps1"
$environment = Get-Environment $environment

# back-end do Terraform
Write-Host -ForegroundColor Yellow "Creating DynamoDB 'fiap-mechanics-tf'..."
aws dynamodb create-table `
  --table-name fiap-mechanics-tf `
  --attribute-definitions AttributeName=LockID,AttributeType=S `
  --key-schema AttributeName=LockID,KeyType=HASH `
  --billing-mode PAY_PER_REQUEST 
Write-Host -ForegroundColor Yellow "Creating bucket 'fiap-mechanics-tf'..."
aws s3 mb s3://fiap-mechanics-tf --region us-east-1

# subir infraestrutura pelo Terraform
Write-Host -ForegroundColor Yellow "Deploying infrastructure for environment '$environment'..."

terraform -chdir="./infra" init -backend-config="key=$environment.tfstate" -reconfigure
terraform -chdir="./infra" apply -var="environment=$environment" -auto-approve

# configurar cluster
Write-Host -ForegroundColor Yellow "Configuring cluster..."

aws eks update-kubeconfig --name fiap-mechanics-$environment-cluster --region us-east-1

helm repo add external-secrets https://charts.external-secrets.io
helm repo add metrics-server https://kubernetes-sigs.github.io/metrics-server
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

helm upgrade --install external-secrets external-secrets/external-secrets --namespace external-secrets --create-namespace

helm upgrade --install metrics-server metrics-server/metrics-server --namespace kube-system  `
  --set args[0]=--kubelet-insecure-tls `
  --set args[1]=--kubelet-preferred-address-types=InternalIP

helm upgrade --install ingress-nginx ingress-nginx/ingress-nginx --namespace ingress-nginx --create-namespace `
  --set controller.service.annotations."service\.beta\.kubernetes\.io/aws-load-balancer-type"="alb" `
  --set controller.service.annotations."service\.beta\.kubernetes\.io/aws-load-balancer-scheme"=internet-facing

kubectl create secret generic aws-credentials `
  --namespace external-secrets `
  --from-literal=access-key-id="$(aws configure get aws_access_key_id)" `
  --from-literal=secret-access-key="$(aws configure get aws_secret_access_key)" `
  --from-literal=session-token="$(aws configure get aws_session_token)"

Write-Host -ForegroundColor Yellow "Waiting for External Secrets Operator to be ready..."
kubectl wait --for=condition=Ready pod --all -n external-secrets --timeout=120s

Write-Host -ForegroundColor Green "Infrastructure for environment '$environment' has been deployed."
