# Mechanics-13soat

Repositório do projeto destinado aos Tech Challenges da Oficina Mecânica da FIAP.

## Definição do ambiente

- SDK: .NET 8.0
- Banco de dados: MSSQL 2025

## Execução via Docker

1. Rode o comando `docker compose up -d` na raiz do projeto.
2. Aguarde o processo concluir e acesse `http://localhost:5000/swagger`.

### Configurações locais

Caso queria alterar as configurações locais, crie uma cópia do
arquivo [appsettings.json](src/Mechanics.Api/appsettings.json) chamada `appsettings.Development.json` na pasta
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
  // acessar o banco de fora do container Docker
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;User Id=sa;Password=2%r6dZ6Xk@g3;TrustServerCertificate=True;"
  }
}
```

## Migrações

Após alterar as classes de domínio, crie uma nova migração no banco de dados com o comando abaixo.
Utilize sempre nomes em PascalCase, sem acentos ou espaços.
As migrações serão aplicadas automaticamente ao iniciar a aplicação.

`dotnet ef migrations add <NomeDaMigration> --project src\Mechanics.Infra.Data --startup-project src\Mechanics.Api`
