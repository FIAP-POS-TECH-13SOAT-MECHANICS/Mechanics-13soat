param ([string]$environment)
. "$PSScriptRoot/get-environment.ps1"
$environment = Get-Environment $environment

$repositoryUrl = aws ecr describe-repositories --repository-names fiap-mechanics-$environment-cr --query "repositories[0].repositoryUri" --output text
$tag = (new-guid).Guid

aws eks update-kubeconfig --name fiap-mechanics-$environment-cluster --region us-east-1

$password = aws ecr get-login-password --region us-east-1
docker login --username AWS --password $password $repositoryUrl

docker build -t fiap-mechanics .
docker tag fiap-mechanics:latest "$($repositoryUrl):$tag"
docker push "$($repositoryUrl):$tag"

helm upgrade --install `
    --set image.repository=$repositoryUrl `
    --set image.tag="$tag" `
    --set app.env=$environment `
    fiap-mechanics ./k8s
