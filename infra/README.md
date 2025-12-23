# Infraestrutura

<!-- TODO: descrição da infra, como recursos criados e fluxo de acesso -->

## Criação via Terraform

As instruções abaixo servem para rodar o projeto em um ambiente da AWS Academy.

### Instalação das ferramentas

Instale
o [Terraform](https://developer.hashicorp.com/terraform/install), [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
e [Helm](https://helm.sh/docs/intro/install/).
No Windows, é possível baixar via [WinGet](https://learn.microsoft.com/pt-br/windows/package-manager/winget/).

```cmd
winget install --id Hashicorp.Terraform
winget install --id Amazon.AWSCLI
winget install --id Helm.Helm
```

Reinicie o terminal após a instalação para atualizar a variável PATH.

### Configuração do AWS CLI

Com as ferramentas instaladas, acesse a [página de cursos na AWS Academy](https://awsacademy.instructure.com/courses),
inicie o laboratório e clique em "AWS Details" para copiar as credenciais de acesso.

Rode o comando abaixo para fazer login:

```cmd
aws configure
```

### Execução dos scripts

Primeiro crie um bucket no S3 para servir de backend do Terraform:

```cmd
aws s3 mb s3://fiap-mechanics-tf --region us-east-1
```

Por padrão, será gerado um ambiente de desenvolvimento (dev).
O sistema permite o acesso público para o banco de dados e servidor SMTP em ambientes que não sejam de produção (prod).

No Powershell, use os seguintes comandos para alterar o ambiente ou a permissão de acesso:

```powershell
$ENV:TF_VAR_environment = 'prod'    # dev, stg ou prod
$ENV:TF_VAR_public_access = 'true'  # padrão é "true" quando environment != "prod"
```

Após a criação do Bucket, acesse a pasta `infra`.
Passe a chave do backend de acordo com o ambiente desejado (dev, stg ou prod) e aplique os scripts.

```powershell
terraform init -backend-config="key=dev.tfstate"
terraform apply -auto-approve
```

> Observação: evite usar `-auto-approve` em ambientes reais.

O processo leva de 10 a 15 minutos.
Serão exibidas algumas informações úteis sobre o ambiente.
Caso precise desses dados novamente, utilize o comando `terraform output`.

Para rodar o projeto com os serviços criados pelo Terraform, crie
um [arquivo de configuração local](./../docs/configuration.md) e use as informações exibidas no terminal. Observe que
várias dessas informações só são exibidas se o projeto estiver definido como público (`public_access = 'true'`).

Se quiser gerenciar outros ambientes, altere a variável `environment` e reconfigure o terraform para usar o state
correto.

```powershell
$ENV:TF_VAR_environment = 'stg'
terraform init -backend-config="key=$ENV:TF_VAR_environment.tfstate" -reconfigure
```

### Upload de imagens para o ECR

Após a execução do script Terraform, copie o valor do campo `cr_repository_url` ou use o AWS CLI (exemplo abaixo).
Retorne à raiz do projeto para compilar a imagem Docker e fazer upload para o ECR:

```powershell
$repositoryUrl = aws ecr describe-repositories --repository-names fiap-mechanics-dev-cr --query "repositories[0].repositoryUri" --output text
$password = aws ecr get-login-password --region us-east-1
docker login --username AWS --password $password $repositoryUrl
docker build -t fiap-mechanics .
docker tag fiap-mechanics:latest "$($repositoryUrl):latest"
docker push "$($repositoryUrl):latest"
```

>Não foi utilizado login via `--password-stdin` para garantir compatibilidade com Windows PowerShell (legado)

### Geração do ambiente via Helm

Utilize o AWS CLI para baixar as configurações do cluster EKS no kubectl.
Ajuste o nome do cluster de acordo com o ambiente.

```powershell
aws eks update-kubeconfig --name fiap-mechanics-dev-cluster --region us-east-1
```

Instale o [External Secrets Operator (ESO)](https://external-secrets.io):

```powershell
helm repo add external-secrets https://charts.external-secrets.io; helm repo update
helm upgrade --install external-secrets external-secrets/external-secrets --namespace external-secrets --create-namespace
```

Adicione outra secret com as credenciais da AWS para o ESO:

```powershell
kubectl create secret generic aws-credentials `
  --namespace external-secrets `
  --from-literal=access-key-id="$(aws configure get aws_access_key_id)" `
  --from-literal=secret-access-key="$(aws configure get aws_secret_access_key)" `
  --from-literal=session-token="$(aws configure get aws_session_token)"
```

Aguarde até o pod `external-secrets-webhook` ser criado e estar pronto.
Utilize o comando `kubectl get pods -n external-secrets --watch` para monitorar o progresso.

Na raiz do projeto, execute o comando abaixo para instalar o Chart:

```powershell
helm upgrade --install --set image.repository=$repositoryUrl --set app.env=dev fiap-mechanics ./k8s
```

### Atualização (nova release)

Para atualizar o ambiente, é necessário recompilar a imagem Docker, subir no ECR utilizando outra tag e lançar uma nova release via Helm.

```powershell
docker build -t fiap-mechanics .
docker tag fiap-mechanics:latest "$($repositoryUrl):new-tag"
docker push "$($repositoryUrl):new-tag"

helm upgrade --set image.repository=$repositoryUrl --set image.tag="new-tag" --set app.env=dev fiap-mechanics ./k8s
```

### Acessando a aplicação

Há duas opções: mapear a porta via `kubectl` ou criar uma rota pública usando Nginx Controller. 

#### Mapeamento de porta

Utilize o seguinte comando para mapear a porta.

```powershell
kubectl port-forward service/fiap-mechanics 5000:5000
```

Acesse o Swagger pela URL [localhost:5000/swagger](http://localhost:5000/swagger) ou
teste a conexão pelo [localhost:5000/health](http://localhost:5000/health).
Observe que o Swagger não está disponível se o ambiente for `prod`.

#### Ingress Controller

Com um Ingress Controller, são gerados URLs públicas para o serviço, de forma que seja possível acessar diretamente pelo navegador.

```powershell
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx; helm repo update
helm upgrade --install ingress-nginx ingress-nginx/ingress-nginx --namespace ingress-nginx --create-namespace --set controller.service.annotations."service\.beta\.kubernetes\.io/aws-load-balancer-type"="alb" --set controller.service.annotations."service\.beta\.kubernetes\.io/aws-load-balancer-scheme"=internet-facing
```

Utilize o comando abaixo para obter a URL do Swagger:

```powershell
"http://$(kubectl get ingress fiap-mechanics -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')/swagger"
```

Aguarde alguns minutos até o load balancer ser criado e o DNS ser propagado.

#### Comandos úteis

Buscar a URL do repositório:

```powershell
$repositoryUrl = aws ecr describe-repositories --repository-names fiap-mechanics-dev-cr --query "repositories[0].repositoryUri" --output text
```

Remover charts antigos:

```powershell
helm uninstall fiap-mechanics
```

Remover secrets da AWS:

```powershell
aws secretsmanager delete-secret --secret-id fiap-mechanics-dev-database --force-delete-without-recovery
aws secretsmanager delete-secret --secret-id fiap-mechanics-dev-email --force-delete-without-recovery
```

Adicionar manualmente as secrets (útil para execução local):

```powershell
kubectl create secret generic fiap-mechanics-database `
  --from-literal=connectionString="Server=fiap-mechanics-dev-db.000.us-east-1.rds.amazonaws.com,1433;Database=fiap-mechanics;User Id=xxx;Password=xxx;TrustServerCertificate=True;"
kubectl create secret generic fiap-mechanics-email `
  --from-literal=host="fiap-mechanics-dev-email-smtp-lb-000.elb.us-east-1.amazonaws.com" `
  --from-literal=port="1025" `
  --from-literal=userName="xxx@mechanics.com" `
  --from-literal=password="xxx"
```
