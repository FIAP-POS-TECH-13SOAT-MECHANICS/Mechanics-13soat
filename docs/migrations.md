
# Migrações

Após alterar as classes de domínio (namespace `Mechanics.Domain`) ou arquivos de configuração do EF (namespace `Mechanics.Infra.Data.Configurations`), crie uma nova migração no banco de dados.

## Preparação do ambiente

1. Instale o Dotnet EF Tools com o comando `dotnet tool install --global dotnet-ef`.
2. Configure a connectionString para `localhost` usando [appsettings.Development.json](/docs/configuration.md).
3. Suba o container do banco de dados com o comando `docker compose up mssql -d`.

## Criação da migração

Execute o comando abaixo na raiz do repositório.
Utilize sempre nomes em PascalCase, sem acentos ou espaços.

```cmd
dotnet ef migrations add <NomeDaMigracao> --project src\Mechanics.Infra.Data --startup-project src\Mechanics.Api
```

A migração será criada na pasta [Migrations](/src/Mechanics.Infra.Data/Migrations).
Revise se está correta antes de rodar o projeto.
As migrações serão aplicadas automaticamente ao iniciar a aplicação.

Caso encontre algum erro, exclua a migração com o comando abaixo:

```cmd
dotnet ef migrations remove --project src\Mechanics.Infra.Data --startup-project src\Mechanics.Api
```
