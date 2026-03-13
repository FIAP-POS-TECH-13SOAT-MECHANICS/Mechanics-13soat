# Observabilidade

- [Visão geral](#visão-geral)
- [Por que OpenTelemetry?](#por-que-opentelemetry)
- [Arquitetura](#arquitetura)
- [Execução local](#execução-local)
- [Execução no Kubernetes](#execução-no-kubernetes)
- [Configuração](#configuração)
- [Verificação](#verificação)

## Visão geral

O projeto utiliza [OpenTelemetry](https://opentelemetry.io/) (OTel) como padrão de instrumentação para coleta de traces distribuídos, métricas e logs estruturados.
Os dados de telemetria são enviados para o [Datadog](https://www.datadoghq.com/) via protocolo OTLP.

Optamos por utilizar o OpenTelemetry Collector, pois o mesmo é mais flexível.
Mudando apenas a variável de ambiente `OTEL_EXPORTER_OTLP_ENDPOINT`, é possível trocar o backend de observabilidade sem alterar o código da aplicação.

```
Hoje: Datadog             → OTEL_EXPORTER_OTLP_ENDPOINT=http://datadog-agent:4317
Amanhã: Grafana           → OTEL_EXPORTER_OTLP_ENDPOINT=http://grafana-agent:4317
Depois: Jaeger            → OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4317
```

## Por que OpenTelemetry?

- **Vendor-neutral**: a aplicação não possui dependência com nenhum fornecedor de observabilidade
- **Padrão da indústria**: suportado por todos os principais backends (Datadog, Grafana, Jaeger, Dynatrace, etc.)
- **Instrumentação automática**: spans de HTTP, SQL e HttpClient são gerados sem código manual
- **Propagação W3C**: o contexto de trace é propagado automaticamente entre serviços via header `traceparent`
- **Correlação completa**: logs, traces e métricas compartilham os mesmos atributos de identificação (`service.name`, `service.version`, `deployment.environment`)

## Arquitetura

```
┌────────────────────────────────┐
│        Mechanics API           │
│        (.NET 8)                │
│                                │
│  Serilog         → stdout JSON │─── logs ────▶  stdout (coletado pelo K8s)
│  OTel SDK traces → OTLP gRPC  │─── :4317 ──▶  Datadog Agent → Datadog APM
│  OTel SDK metrics → OTLP gRPC │─── :4317 ──▶  Datadog Agent → Datadog Metrics
└────────────────────────────────┘
```

O [Datadog Agent](https://docs.datadoghq.com/agent/) roda como container (local) ou DaemonSet (Kubernetes) e recebe dados via [OTLP Receiver](https://docs.datadoghq.com/opentelemetry/config/otlp_receiver/) na porta 4317 (gRPC).


### Instrumentação automática

| Operação | Instrumentação | O que gera |
|----------|---------------|------------|
| Requisições HTTP de entrada | `AddAspNetCoreInstrumentation` | Spans com `http.method`, `http.route`, `http.response.status_code` |
| Chamadas HTTP de saída | `AddHttpClientInstrumentation` | Spans com propagação de contexto W3C |
| Queries SQL (EF Core) | `AddSqlClientInstrumentation` | Spans com `db.system`, `db.statement` |
| Runtime .NET | `AddRuntimeInstrumentation` | Métricas de GC, thread pool e heap |

### Sampling

| Ambiente | Taxa | Descrição |
|----------|:----:|-----------|
| Development | 100% | Todos os traces são capturados |
| Staging | 100% | Todos os traces são capturados |
| Production | 10% | 1 em cada 10 requisições gera trace |

### Endpoints filtrados

Os seguintes endpoints não geram spans para reduzir ruído:

- `/health`
- `/swagger` 

## Execução local

O Docker Compose está configurado para subir o Datadog Agent junto com a aplicação.
É necessário definir a variável de ambiente `DD_API_KEY` antes de executar.

### Obter a API Key

1. Crie uma conta em [datadoghq.com](https://www.datadoghq.com/)
2. Acesse **Organization Settings → API Keys**
3. Copie a chave

```powershell
$env:DD_API_KEY = "sua_api_key"
docker compose up -d --build
```

### Serviços disponíveis

| Serviço | URL |
|---------|-----|
| Swagger da API | http://localhost:5000/swagger |
| Health check | http://localhost:5000/health |
| Cliente de e-mail | http://localhost:8025 |
| Datadog APM | https://us5.datadoghq.com/apm/traces |

### Sem Datadog (apenas console)

Para rodar sem o Datadog, remova as variáveis `OTEL_EXPORTER_OTLP_ENDPOINT` e o serviço `datadog-agent` do `docker-compose.yml`.
Em ambiente `Development`, os traces são exibidos no console:

```powershell
docker compose logs -f dotnet
```

## Execução no Kubernetes

O Datadog Agent é instalado no cluster EKS via Helm chart durante a pipeline de CI/CD.
A API Key deve ser adicionada como **repository secret** no GitHub.

### Adicionar secret no GitHub

Acesse **Settings → Secrets and variables → Actions → Repository secrets** e adicione:

| Secret | Valor |
|--------|-------|
| `DD_API_KEY` | API Key do Datadog |

A pipeline `deploy.yml` utiliza essa secret para:
1. Criar uma Kubernetes secret (`datadog-api-key`) no cluster
2. Instalar o Datadog Agent como DaemonSet via Helm
3. Configurar a variável `OTEL_EXPORTER_OTLP_ENDPOINT` na API


## Configuração

### Variáveis de ambiente

| Variável | Descrição | Obrigatória | Padrão |
|----------|-----------|:-----------:|--------|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Endpoint do coletor OTLP (gRPC) | Não | Vazio (sem exportação) |
| `SERVICE_VERSION` | Versão do serviço | Não | `0.0.0` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente .NET | Sim | `Production` |
| `DD_API_KEY` | API Key do Datadog (Datadog Agent) | Sim* | — |

\* Necessária apenas para o container do Datadog Agent, não para a aplicação.

### Parâmetros do Helm chart

| Parâmetro | Descrição | Padrão |
|-----------|-----------|--------|
| `otel.otlpEndpoint` | Endpoint OTLP do Datadog Agent | Vazio |

## Verificação

### Checklist

1. Suba o projeto com `docker compose up -d --build`
2. Acesse http://localhost:5000/swagger
3. Faça login via `POST /api/auth/login` (CPF `12345678909`, senha `5eCre+Key`)
4. Chame qualquer endpoint autenticado (ex: `GET /api/work-orders`)
5. Acesse https://us5.datadoghq.com/apm/traces
6. Filtre por `service:mechanics-api`
7. Verifique os spans de HTTP e SQL nos traces

## Referências

- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/dotnet/)
- [Datadog OTLP Receiver](https://docs.datadoghq.com/opentelemetry/config/otlp_receiver/)
- [Datadog Agent Docker](https://docs.datadoghq.com/containers/docker/)
- [Datadog Agent Kubernetes (Helm)](https://docs.datadoghq.com/containers/kubernetes/installation/?tab=helm)