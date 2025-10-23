# Fiap.Mechanics

[![.NET](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/Mechanics-13soat/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/Mechanics-13soat/actions/workflows/dotnet.yml)

Repositório do projeto destinado aos Tech Challenges da Oficina Mecânica da FIAP.

## Definição do ambiente

- SDK: .NET 8.0
- Banco de dados: MSSQL 2025

## Execução via Docker

1. Rode o comando `docker compose up -d --build` na raiz do projeto.
2. Aguarde o processo concluir e acesse `http://localhost:5000/swagger`.
3. Acesse o cliente de e-mail em `http://localhost:8025`.

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
    // acessar o banco de fora do container Docker (necessário para criar migrações)
    "ConnectionStrings": {
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

## Migrações

Após alterar as classes de domínio, crie uma nova migração no banco de dados com o comando abaixo.
Utilize sempre nomes em PascalCase, sem acentos ou espaços.

### Preparação do ambiente

1. Instale o Dotnet EF Tools com o comando `dotnet tool install --global dotnet-ef`.
2. Configure a connectionString para `localhost` usando [appsettings.Development.json](#configurações-locais).
3. Suba o container do banco de dados com o comando `docker compose up mssql -d`.

### Criação da migração

Execute o comando abaixo na raiz do repositório.
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
    ├── Validators
    |   └── [NomeRequest]Validator.cs
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
- **Validator**
    - Valida as requisições.
    - Estende a classe `IAbstractValidator<TRequest>`.
    - As validações de requests são aplicadas automaticamente via filtro de requisições.
- **MapperProfile**
    - Configura o mapeamento entre as entidades e os DTOs de request e response.
    - Estende a classe `AutoMapper.Profile`.
    - Request e response devem estar em DTOs separados (não utilize `ReverseMap()`).
    - Mantenha o arquivo em ordem alfabética e separe os DTOs de cada entidade com uma linha em branco.

## Validações

As validações podem ser aplicadas na camada de aplicação (filtragem de requisições) ou nas entidades de domínio.
O ideal é fazer validações mais simples na request (por ex, campos não preenchidos ou IDs inválidos) e aplicar regras
mais
complexas nas entidades (por ex, validação de CPF ou CNPJ).

### Validação de requisições

Crie uma classe que estenda `AbstractValidator<TRequest>` na pasta `Validators`.
O nome da classe deve seguir o padrão `[NomeDaRequest]Validator`.
Ao chamar o endpoint que recebe a request, a validação é aplicada automaticamente pelo filtro `RequestValidationFilter`.

### Validação de entidades

Na entidade de domínio, implemente a interface `IValidatableObject`.
Execute a validação chamando `Validator.ValidateAndThrow(entity);`.
A exceção gerada será capturada pelo middleware `DomainValidationMiddleware`, que formata o erro e retorna HTTP 400.

### DTOs comuns

O namespace `Application.Utils` contém algumas classes que podem ser utilizadas em diferentes contextos:

- `CreateItemResponse`: Retorna o ID de um objeto recém-criado.
- `ListResponse`: Encapsula uma lista de objetos.
- `PagedList`: Padroniza consulta de itens utilizando paginação.

## Normalização de dados

São regras aplicadas nas entidades para garantir padronização dos dados.
Por exemplo, utilize para remover espaços em branco ou pontuações desnecessárias em strings.
Também pode ser útil normalizar os dados antes de executar certas validações para evitar inconsistências.

Defina as regras implementando a interface `INormalizable`.
Ao persistir os dados (chamar `SaveChangesAsync()`), a entidade é interceptada na camada de infraestrutura e, caso
`IsNormalized()` seja `false`, o método `Normalize()` é executado.

## Usuários padrão

Os seguintes logins podem ser utilizados para testes:

| Usuário         | Senha       | Perfil        |
|-----------------|-------------|---------------|
| `administrator` | `5eCre+Key` | Administrador |
| `attendant`     | `5eCre+Key` | Atendente     |
| `mechanic`      | `5eCre+Key` | Mecânico      |
