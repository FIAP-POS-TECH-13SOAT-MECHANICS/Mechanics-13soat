# Infraestrutura

## Criação via Terraform

As instruções abaixo servem para rodar o projeto em um ambiente da AWS Academy.

### Instalação das ferramentas

Instale o [Terraform](https://developer.hashicorp.com/terraform/install)
e [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html).
No Windows, é possível baixar via [WinGet](https://learn.microsoft.com/pt-br/windows/package-manager/winget/).

```cmd
winget install --id Hashicorp.Terraform
winget install --id Amazon.AWSCLI
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

Caso queira permitir o acesso público (de fora do EKS) para o banco de dados de servidor SMTP, defina a variável
`public` como `true`.
No Powershell, use o seguinte comando:

```powershell
$ENV:TF_VAR_public = 'true'
```

Após a criação do Bucket, acesse a pasta `infra` e aplique os scripts do Terraform.
O processo leva cerca de 10 minutos.

```cmd
terraform init
terraform apply -auto-approve
```

São exibidas algumas informações úteis sobre o ambiente.
Caso precise desses dados novamente, utilize o comando `terraform output`.

Para rodar o projeto com os serviços criados pelo Terraform, crie
um [arquivo de configuração local](./../docs/configuration.md) e use as informações exibidas no terminal. Observe que
várias dessas informações só são exibidas se o projeto estiver definido como público (`TF_VAR_public = 'true'`).

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
