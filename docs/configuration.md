# Configuração do ambiente

Siga os passos alterar as configurações do projeto.

1. [Defina as configurações locais](#configurações-locais)
2. [Inicie os serviços](#serviços)
3. [Rode o projeto](#execução-do-projeto)

## Configurações locais

O arquivo `appsettings.json` está configurado para permitir a execução via Docker e não deve ser alterado.
Para alterar a connectionString (por exemplo, um banco RDS da AWS Academy), é necessário criar um arquivo de
configuração local. Também pode ser útil para alterar outras configurações como o tempo de expiração do token JWT.

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
        // acessar outro banco de dados
        "Default": "Server=xxx.rds.amazonaws.com;Database=fiap-mechanics;User Id=sa;Password=2%r6dZ6Xk@g3;TrustServerCertificate=True;"
    },
    "EmailSenderOptions": {
        // desabilitar envio de email
        "Enabled": false,
        // alterar servidor SMTP
        "SmtpServer": "xxx.elb.amazonaws.com"
    }
}
```

## Serviços

Caso não esteja rodando os serviços externos (banco de dados e servidor SMTP) remotamente, você pode iniciar via Docker Compose.

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
