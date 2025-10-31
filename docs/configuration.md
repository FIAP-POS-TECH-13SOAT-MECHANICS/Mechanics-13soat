# Configuração do ambiente

Siga os passos abaixo para executar o projeto em uma IDE ou via terminal.

1. [Defina as configurações locais](#configurações-locais)
2. [Inicie os serviços](#serviços)
3. [Rode o projeto](#execução-do-projeto)

## Configurações locais

O arquivo `appsettings.json` está configurado para permitir a execução via Docker e não deve ser alterado. Para rodar o projeto, é necessário criar um arquivo de configuração que aponte para `localhost`.

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
    "JwtOptions": {
        // aumentar o tempo de expiração do token JWT
        "AccessTokenLifetime": 30
    },
    "ConnectionStrings": {
        // acessar o banco de fora do container Docker (necessário para migrações)
        "DefaultConnection": "Server=localhost;User Id=sa;Password=2%r6dZ6Xk@g3;TrustServerCertificate=True;"
    },
    "EmailSenderOptions": {
        // desabilitar envio de email
        "Enabled": false,
        // acessar servidor SMTP de fora do container Docker
        "SmtpServer": "localhost"
    }
}
```

## Serviços

Você pode iniciar os serviços usando o Docker Compose. Certifique-se que o Docker está em execução e rode o seguinte comando na raiz do projeto:

```cmd
docker compose up mssql mailpit -d
```

Pode levar algum tempo até o SQL Server iniciar totalmente.

## Execução do projeto

Para rodar o projeto via terminal, use o comando abaixo:

```cmd
dotnet run --project .\src\Mechanics.Api\Mechanics.Api.csproj
```

Acesse o Swagger do projeto em [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html).
