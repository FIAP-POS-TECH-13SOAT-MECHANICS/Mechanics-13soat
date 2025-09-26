# Fiap.Mechanics

Repositório do projeto destinado aos Tech Challenges da Oficina Mecânica da FIAP.

## Definição do ambiente

- SDK: .NET 8.0
- Banco de dados: MSSQL 2025

## Execução via Docker

1. Rode o comando `docker compose up -d --build` na raiz do projeto.
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

## Use-cases

Os casos de uso são implementados no projeto [Mechanics.Application](src/Mechanics.Application) seguindo o seguinte
padrão:

```text
Mechanics.Application
└── [Domínio]
    ├── Requests
    |   └── Get[Entidade]Request.cs
    ├── Responses
    |   └── Get[Entidade]Response.cs
    ├── Services
    |   └── [Entidade]AppService.cs
    └── [Domínio]MapperProfile.cs
```

- **Request**
    - É a requisição recebida pela controller.
    - Pode conter validações.
- **Response**
    - É a resposta retornada à controller.
- **Service**
    - Executa a operação seguindo as regras de negócio.
    - Implementa a interface `IAppService` (configura automaticamente a injeção de dependência).
    - Devem ser agrupados por entidade de domínio.
- **MapperProfile**
    - Configura o mapeamento entre as entidades e os DTOs de request e response.
    - Estende a classe `AutoMapper.Profile`.
    - Request e response devem estar em DTOs separados (não utilize `ReverseMap()`).
    - Mantenha o arquivo em ordem alfabética e separe os DTOs de cada entidade com uma linha em branco.

### DTOs comuns

O namespace `Application.Utils` contém algumas classes que podem ser utilizadas em diferentes contextos:

- `CreateItemResponse`: Retorna o ID de um objeto recém-criado.
- `ListResponse`: Encapsula uma lista de objetos.
- `PagedList`: Padroniza consulta de itens utilizando paginação.
