# Infraestrutura

## Criação via Terraform

As instruções abaixo servem para rodar o projeto em um ambiente da AWS Academy.

### Instalação das ferramentas

Instale o [Terraform](https://developer.hashicorp.com/terraform/install) e [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html).
No Windows, é possível baixar via [WinGet](https://learn.microsoft.com/pt-br/windows/package-manager/winget/).

```cmd
winget install --id Hashicorp.Terraform
winget install --id Amazon.AWSCLI
```

Reinicie o terminal após a instalação para atualizar a variável PATH.

### Configuração do AWS CLI

Com as ferramentas instaladas, acesse a [página de cursos na AWS Academy](https://awsacademy.instructure.com/courses), inicie o laboratório e clique em "AWS Details" para copiar as credenciais de acesso.

Rode o comando abaixo para fazer login:

```cmd
aws configure
```

### Execução dos scripts

Primeiro crie um bucket no S3 para servir de backend do Terraform:

```cmd
aws s3 mb s3://fiap-mechanics-tf --region us-east-1
```

Após a criação do Bucket, acesse a pasta `infra` e aplique os scripts do Terraform:

```cmd
terraform init
terraform apply -auto-approve
```

O processo de criação leva cerca de 10 minutos.
