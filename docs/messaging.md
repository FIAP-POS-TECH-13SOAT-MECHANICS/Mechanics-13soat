# Messageria

A comunicação assíncrona entre microsserviços é feita via **Amazon SQS**, utilizando o modelo pub/sub desacoplado: um serviço publica um evento em uma fila e outro serviço consome essa fila de forma independente, sem conhecimento direto entre os lados.

A integração é fornecida pelo projeto `Mechanics.Infra.Messaging`, que encapsula o SDK da AWS e expõe interfaces simples para publicação e consumo de eventos.

## Criando filas (Infra)

As filas são criadas e gerenciadas via Terraform, na camada `messaging` do repositório [mechanics-infra](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-infra). Cada fila é declarada na variável `queue_names` em `vars.tf`:

```hcl
variable "queue_names" {
  type    = list(string)
  default = [
    "customer-created",
    "customer-updated",
  ]
}
```

O nome da fila deve seguir o padrão `{entidade}-{evento}` (ex: `customer-created`, `order-completed`). O Terraform criará automaticamente a fila principal e sua Dead Letter Queue (DLQ), seguindo a nomenclatura:

- Fila principal: `fiap-mechanics-{environment}-{name}`
- DLQ: `fiap-mechanics-{environment}-{name}-dlq`

## Publicando eventos

Injete `IEventPublisher` no serviço que precisar publicar e chame `PublishAsync<T>`, onde `T` é a classe do evento:

```csharp
public class CustomerService(IEventPublisher publisher)
{
    public async Task CreateAsync(Customer customer, CancellationToken ct)
    {
        // lógica de negócio...

        await publisher.PublishAsync(new CustomerCreatedEvent
        {
            CustomerId = customer.Id,
            Name = customer.Name,
        }, ct);
    }
}
```

A classe do evento deve ser definida na camada de `Application` do serviço, **não** no projeto `Mechanics.Infra.Messaging`, que não conhece tipos específicos de nenhum serviço:

```csharp
// Application/Customers/Events/CustomerCreatedEvent.cs
public record CustomerCreatedEvent
{
    public Guid CustomerId { get; init; }
    public string Name { get; init; } = string.Empty;
}
```

O publisher resolve a fila automaticamente pelo nome do tipo: `CustomerCreatedEvent` => chave `CustomerCreated` => nome da fila configurado nas options.

> **Producers não precisam ser declarados no IoC** além do registro do `AddMessaging` no `Program.cs`.

## Consumindo eventos

Implemente a interface `IEventConsumer<T>` na camada de `Application` do serviço consumidor:

```csharp
// Application/Consumers/CustomerCreatedConsumer.cs
public class CustomerCreatedConsumer(ILogger<CustomerCreatedConsumer> logger)
    : IEventConsumer<CustomerCreatedEvent>
{
    public async Task ConsumeAsync(CustomerCreatedEvent message, CancellationToken ct)
    {
        logger.LogInformation("Processing customer {CustomerId}", message.CustomerId);
        // lógica de negócio...
    }
}
```

O `Mechanics.Infra.Messaging` registra automaticamente um `BackgroundService` por consumer, responsável pelo loop de polling no SQS. O consumer concreto só precisa implementar a lógica de negócio - gerenciamento de fila, deserialização, retry e exclusão de mensagem são tratados pela infraestrutura.

O fluxo de retry é gerenciado pelo próprio SQS: em caso de exceção, a mensagem retorna à fila após o *visibility timeout* e é reprocessada até atingir o `maxReceiveCount` configurado no Terraform, quando então é movida automaticamente para a DLQ.

## Configuração (IoC, AppSettings e Chart)

### Injeção de dependências

```csharp
// Program.cs
builder.Services.AddMessaging(builder.Configuration);
```

O registro de consumers é feito na camada `Mechanics.Infra.CrossCutting.IoC` do serviço, seguindo o padrão das demais extensões do projeto:

```csharp
// Extensions/MessagingExtensions.cs
public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
{
    services.Configure<AwsCredentialsOptions>(configuration.GetSection("AwsCredentials"));
    services.Configure<MessagingOptions>(configuration.GetSection(nameof(MessagingOptions)));

    services.AddMessaging(messaging =>
    {
        // declare apenas os consumers do serviço (producers não precisam ser declarados)
        messaging.AddConsumer<CustomerCreatedConsumer, CustomerCreatedEvent>();
    });

    return services;
}
```

### `appsettings.json`

```json
{
  "AwsCredentials": {
    "Region": "us-east-1",
    "AccessKey": "",
    "SecretKey": "",
    "SessionToken": ""
  },
  "MessagingOptions": {
    "QueueNames": {
      "CustomerCreated": "fiap-mechanics-dev-customer-created"
    }
  }
}
```

> **Atenção:** as chaves do `QueueNames` são **case-sensitive** e devem estar em PascalCase, sem o sufixo `Event` (ex: `CustomerCreated`, não `customerCreated` nem `CustomerCreatedEvent`).

### Helm `values.yaml`

```yaml
queueNames:
  CustomerCreated: customer-created
```

O Chart monta o `ConfigMap` iterando sobre `queueNames` e compondo o nome completo da fila com o prefixo de ambiente:

```yaml
# templates/configmap.yaml
data:
  {{- range $key, $value := .Values.queueNames }}
  MessagingOptions__QueueNames__{{ $key }}: "{{ $.Values.app.name }}-{{ $.Values.app.env }}-{{ $value }}"
  {{- end }}
```

O value no `values.yaml` contém apenas o sufixo da fila (sem o prefixo `fiap-mechanics-{env}-`), que é composto automaticamente pelo template.
