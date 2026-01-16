param ([string]$environment)
. "$PSScriptRoot/get-environment.ps1"
$environment = Get-Environment $environment

Write-Host -ForegroundColor Yellow "Destroying environment: $environment"

Write-Host -ForegroundColor Yellow "Deleting secrets..."
aws secretsmanager list-secrets --query "SecretList[?starts_with(Name, 'fiap-mechanics-$environment')].Name" | ConvertFrom-Json | `
  ForEach-Object { aws secretsmanager delete-secret --secret-id $_ --force-delete-without-recovery } | Out-Null

$repositoryName = "fiap-mechanics-$environment-cr"
Write-Host -ForegroundColor Yellow "Deleting ECR images from $repositoryName..."
$imageList = aws ecr list-images --repository-name $repositoryName --query "imageIds[].imageDigest" --no-paginate | ConvertFrom-Json | `
  ForEach-Object { "imageDigest=$_" }
aws ecr batch-delete-image --repository-name $repositoryName --image-ids $imageList | Out-Null

Write-Host -ForegroundColor Yellow "Running terraform destroy..."
$bucketName = "fiap-mechanics-tf-$(aws sts get-access-key-info --access-key-id $(aws configure get aws_access_key_id) --query Account --output text)"
terraform -chdir="./infra" init -backend-config="bucket=$bucketName" -backend-config="key=$environment.tfstate" -reconfigure
terraform -chdir="./infra" destroy -target="helm_release.metrics_server" -target="helm_release.ingress_nginx" -target="helm_release.mailpit" -target="helm_release.external_secrets"
terraform -chdir="./infra" destroy -var="environment=$environment" -auto-approve

Write-Host -ForegroundColor Green "Infrastructure for environment '$environment' has been destroyed."
Write-Host "The S3 bucket may be deleted manually."
