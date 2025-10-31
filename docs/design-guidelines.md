# Diretrizes de design do projeto

- [Padronização de nomes](#padronização-de-nomes)
- [Casos de uso](#casos-de-uso)
- [Validações](#validações)
- [Normalização de dados](#normalização-de-dados)

## Padronização de nomes

Siga os [padrões oficiais do .Net](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines), como usar PascalCase em classes, propriedades e métodos, ou camelCase no payload de requisições.

Em controllers e requests, utilize XmlDoc pra documentar os métodos, conforme exemplo abaixo. As definições ficarão disponíveis no Swagger.

```csharp
/// <summary>
///     Controller para gerenciar informações de login.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/auth")]
public class AuthController(AuthAppService service) : ControllerBase
{
    /// <summary>
    ///     Gera um token JWT para o usuário fornecido.
    /// </summary>
    /// <returns>Um <see cref="TokenResponse"/> contendo o token JWT.</returns>
    /// <remarks>A senha deve ser <c>5eCre+Key</c>.</remarks>
    /// <response code="200">Usuário autenticado com sucesso.</response>
    /// <response code="401">Usuário ou senha inválidos.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [Consumes(typeof(LoginRequest), "application/json")]
    [Produces("application/json", Type = typeof(TokenResponse))]
    [ProducesResponseType(typeof(TokenResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await service.Login(request, cancellationToken);
        return response is not null ? Ok(response) : Unauthorized();
    }
}

public class LoginRequest
{
    /// <summary>
    ///     Nome de usuário.
    /// </summary>
    /// <example>administrator</example>
    public required string UserName { get; init; }

    /// <summary>
    ///     Senha da conta.
    /// </summary>
    /// <example>5eCre+Key</example>
    public required string Password { get; init; }
}
```

## Casos de uso

São implementados no projeto [Mechanics.Application](src/Mechanics.Application) seguindo o seguinte
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
    - Sempre passe o `CancellationToken` para serviços externos, como queries ou outras APIs.
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
- `UpdateItemResponse`: Retorna o ID de um objeto atualizado.
- `ListResponse`: Encapsula uma lista de objetos.
- `PagedList`: Padroniza consulta de itens utilizando paginação.

## Normalização de dados

São regras aplicadas nas entidades para garantir padronização dos dados.
Por exemplo, utilize para remover espaços em branco ou pontuações desnecessárias em strings. As regras são definidas implementando a interface `INormalizable`.

Também pode ser útil normalizar os dados antes de executar certas validações para evitar inconsistências. Por exemplo, normalize antes de validar clientes para garantir um resultado correto no cálculo de CNPJ.

```csharp
if (!entity.IsNormalized())
  entity.Normalize();

Validator.ValidateAndThrow(entity);
```

Caso não seja necessário para as validações, não chame o método `Normalize()` na camada de serviço. Ao persistir os dados (chamar `SaveChangesAsync()`), a entidade é interceptada na camada de infraestrutura e, caso
`IsNormalized()` seja `false`, o método `Normalize()` é executado.
