# Configuração do ambiente

Siga os passos alterar as configurações do projeto.

1. [Defina as configurações locais](#configurações-locais)
2. [Inicie os serviços](#serviços)
3. [Rode o projeto](#execução-do-projeto)

## Ambiente

Em aplicações .Net, é definido através da variável de ambiente `ASPNETCORE_ENVIRONMENT`.
A pipeline de CI/CD seleciona o ambiente automaticamente de acordo com a branch.
O Docker Compose está configurado para rodar em ambiente de desenvolvimento.

O projeto possui os seguintes ambientes:

- `Production` (main): modo de produção. Por segurança, o Swagger é desativado.
- `Staging` (release): ambiente de homologação. O Swagger está ativo, mas as migrações do banco devem ser executadas manualmente.
- `Development` (develop): ambiente de desenvolvimento. Swagger ativo e as migrações são executadas ao iniciar o projeto.

Para executar via Helm chart, utilize o parâmetro `app.env` e o nome do ambiente abreviado (`prod`, `stg` ou `dev`).
As migrações são executadas durante o deploy via Helm (para todos os ambientes).

```bash
helm upgrade --install fiap-mechanics ./k8s --set image.repository=$REPOSITORY_URI --set app.env=stg
```

## Configurações locais

O arquivo `appsettings.json` está configurado para permitir a execução via Docker e não deve ser alterado.
Para alterar a connectionString (por exemplo, usando um banco RDS da AWS Academy), é necessário criar um arquivo de
configuração local. Também pode ser útil para alterar configurações como logs ou o tempo de expiração do token JWT.

Primeiro crie uma cópia do
arquivo [appsettings.json](/src/Mechanics.Api/appsettings.json) chamada `appsettings.Development.json` na pasta
`src/Mechanics.Api`. Esse arquivo não deve ser adicionado ao repositório.

O exemplo abaixo contém algumas configurações comuns:

```json
{
    // logs mais detalhados
    "Logging": {
        "LogLevel": {
            "Default": "Debug",
            "System": "Information",
            "Microsoft": "Information"
        }
    },
    "ConnectionStrings": {
        // acessar outro banco de dados
        "Default": "Server=xxx.rds.amazonaws.com;Database=fiap-mechanics;User Id=sa;Password=2%r6dZ6Xk@g3;TrustServerCertificate=True;"
    },
    "EmailSenderOptions": {
        // desabilitar envio de email
        "Enabled": false,
        // alterar servidor SMTP
        "SmtpServer": "xxx.elb.amazonaws.com",
        "SmtpPort": 25
    },
    "AppInfo": {
        // endereço base do projeto (para links em e-mails)
        "BaseUrl": "http://localhost:5000"
    },
    // credenciais da AWS
    "AwsCredentials": {
        // desativar o serviço local (emulador)
        "UseLocalstack": false,
        // use as credenciais da AWS Academy
        "AccessKey": "ACCESS-KEY",
        "SecretAccessKey": "SECRET",
        "SessionToken": "TOKEN"
  }
}
```

## Serviços

Caso não esteja rodando os serviços externos (banco de dados e servidor SMTP) remotamente, você pode iniciar via Docker
Compose.

Certifique-se que o Docker está em execução e rode o seguinte comando na raiz do projeto.
Pode levar algum tempo até o SQL Server iniciar totalmente.

```cmd
docker compose up mssql mailpit -d
```

## Execução do projeto

Para rodar o projeto via terminal, use o comando abaixo:

```cmd
dotnet run --project .\src\Mechanics.Api\Mechanics.Api.csproj
```

Acesse o Swagger do projeto em [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html).
