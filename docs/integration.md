# Integração entre microsserviços

A comunicação síncrona entre microsserviços é feita via **HTTP autenticado**, utilizando o projeto `Mechanics.Infra.CrossServiceClient`. Esse projeto encapsula o fluxo de autenticação M2M (machine-to-machine): antes de cada requisição, o cliente obtém um token JWT da Lambda de autenticação e o injeta automaticamente no cabeçalho `Authorization`.

Em ambiente local, o LocalStack é utilizado para emular a AWS. Nesse caso, o token é gerado localmente com uma chave simétrica genérica. Como a validação de tokens é desativada em ambiente de desenvolvimento (DEV), o fluxo funciona sem dependências externas.

## Estrutura do projeto

A integração é distribuída em três camadas:

- **`Mechanics.Infra.CrossServiceClient`:** implementação do fluxo de autenticação. Obtém o token JWT invocando a Lambda [auth-token](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-auth) via SDK da AWS (ou gera um token local quando o LocalStack está ativo) e o injeta nas requisições via `DelegatingHandler`.
- **`Application`:** implementação do cliente HTTP de cada serviço externo. Cada cliente recebe um `HttpClient` já configurado com autenticação e usa as rotas completas do serviço de destino (incluindo o prefixo do serviço).
- **`Mechanics.Infra.CrossCutting.IoC`:** registro dos clientes no contêiner de injeção de dependências, associando a interface ao serviço concreto e informando a URL base.

## Criando um cliente HTTP

### 1. Defina a interface e o serviço na camada Application

Crie a interface e a implementação dentro da pasta correspondente ao contexto do serviço externo:

```csharp
// Application/Identity/Services/IUserService.cs
public interface IUserService
{
    Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);
}
```

```csharp
// Application/Identity/Services/UserService.cs
public class UserService(HttpClient client) : IUserService
{
    public async Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync($"identity/users/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserResponse>(cancellationToken: cancellationToken);
    }
}
```

> A rota deve ser sempre completa, incluindo o prefixo do serviço de destino (ex: `identity/users/{id}`, não `users/{id}`). A URL base configurada aponta para o NLB ou para o endereço interno do cluster, sem o prefixo de rota.

Defina também os contratos de resposta na mesma camada:

```csharp
// Application/Identity/Responses/UserResponse.cs
public record UserResponse
{
    public Guid Id { get; init; }
    public required string FullName { get; init; }
}
```

### 2. Registre o cliente no IoC

No projeto `Mechanics.Infra.CrossCutting.IoC`, adicione o cliente ao método `AddCrossServiceClients`:

```csharp
// Extensions/CrossServiceClientsExtensions.cs
public static IServiceCollection AddCrossServiceClients(this IServiceCollection services, IConfiguration configuration)
{
    var options = configuration.GetSection(nameof(CrossServiceClients)).Get<CrossServiceClients>()!;

    services.AddHttpClients(configuration)
        .AddCrossServiceClient<IUserService, UserService>(options.IdentityBaseUrl);
        // adicione novos clientes aqui

    return services;
}
```

O método `AddCrossServiceClient<TInterface, TService>` registra o `HttpClient` com a URL base informada e adiciona automaticamente o `ServiceTokenHandler`, responsável por injetar o token nas requisições.

### 3. Autorize a role Service no endpoint

Endpoints que precisam ser acessados por outros serviços devem declarar explicitamente a role `Service` no atributo `[Authorize]`:

```csharp
[Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Service}")]
public async Task<IActionResult> GetUser(Guid id, CancellationToken ct) { ... }
```

A constante `RoleNames.Service` é definida no projeto `Mechanics.Infra.Security`.

## Configuração (AppSettings e Chart)

### `appsettings.json`

```json
{
  "AwsCredentials": {
    "Region": "us-east-1",
    "UseLocalstack": true,
    "LocalstackUrl": "http://localhost:4566",
    "AccessKey": "",
    "SecretAccessKey": "",
    "SessionToken": ""
  },
  "CrossServiceClients": {
    "AuthTokenFunctionName": "fiap-mechanics-dev-auth-token",
    "BillingBaseUrl": "",
    "ExecutionBaseUrl": "",
    "IdentityBaseUrl": "",
    "WorkOrdersBaseUrl": ""
  }
}
```

Em ambiente local, defina `UseLocalstack: true` e deixe as URLs base em branco ou apontando para `localhost` conforme necessário. O `AuthTokenFunctionName` é ignorado quando o LocalStack está ativo.

Para apontar para serviços rodando no ambiente AWS, obtenha a URL do NLB e configure a URL base correspondente no `appsettings.Development.json`. Consulte o guia de [configuração do ambiente](./configuration.md) para mais detalhes.

### Helm `values.yaml` e ConfigMap

As URLs internas seguem o padrão DNS do Kubernetes: `http://{serviço}.{namespace}:{porta}`. Todos os serviços rodam na porta `5000`.

O ConfigMap do chart já está configurado com os valores para todos os microsserviços do projeto:

```yaml
# templates/configmap.yaml
CrossServiceClients__AuthTokenFunctionName: "{{ $.Values.app.project }}-{{ $.Values.app.env }}-auth-token"
CrossServiceClients__BillingBaseUrl: "http://billing.billing:5000"
CrossServiceClients__ExecutionBaseUrl: "http://execution.execution:5000"
CrossServiceClients__IdentityBaseUrl: "http://identity.identity:5000"
CrossServiceClients__WorkOrdersBaseUrl: "http://work-orders.work-orders:5000"
```

Ao adicionar um novo microsserviço ao projeto, inclua a entrada correspondente no ConfigMap do chart de cada serviço que precisar consumi-lo.
