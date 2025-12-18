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

Após a execução do script Terraform, copie o valor do campo `cr_repository_url`.
Retorne à raiz do projeto para compilar a imagem Docker e fazer upload para o ECR:

```powershell
$repositoryUrl = URL_DO_REPOSITORIO_ENTRE_ASPAS
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin $repositoryUrl
docker build -t fiap-mechanics .
docker tag fiap-mechanics:latest "$($repositoryUrl):latest"
docker push "$($repositoryUrl):latest"
```

### Geração do ambiente via Helm

Na raiz do projeto, execute o comando abaixo para instalar o Chart:

```powershell
helm upgrade --install fiap-mechanics ./k8s
```
