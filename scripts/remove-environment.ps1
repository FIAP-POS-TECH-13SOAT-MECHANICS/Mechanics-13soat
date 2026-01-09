param ([string]$environment)
. "$PSScriptRoot/get-environment.ps1"
$environment = Get-Environment $environment

Write-Host -ForegroundColor Yellow "Destroying environment: $environment"

Write-Host -ForegroundColor Yellow "Removing ingress controller..."
helm uninstall ingress-nginx 2>$null

Write-Host -ForegroundColor Yellow "Deleting secrets..."
aws secretsmanager list-secrets --query "SecretList[?starts_with(Name, 'fiap-mechanics-$environment')].Name" | ConvertFrom-Json | `
  ForEach-Object { aws secretsmanager delete-secret --secret-id $_ --force-delete-without-recovery }

$repositoryName = "fiap-mechanics-$environment-cr"
Write-Host -ForegroundColor Yellow "Deleting ECR images from $repositoryName..."
$imageList = aws ecr list-images --repository-name $repositoryName --query "imageIds[].imageDigest" --no-paginate | ConvertFrom-Json | `
  ForEach-Object { "imageDigest=$_" }
aws ecr batch-delete-image --repository-name $repositoryName --image-ids $imageList

Write-Host -ForegroundColor Yellow "Running terraform destroy..."
terraform -chdir="./infra" destroy -var="environment=$environment" -auto-approve

Write-Host -ForegroundColor Green "Infrastructure for environment '$environment' has been destroyed."
