# Observabilidade

- [Visão geral](#visão-geral)
- [Por que OpenTelemetry?](#por-que-opentelemetry)
- [Arquitetura](#arquitetura)
- [Execução local](#execução-local)
- [Execução no Kubernetes](#execução-no-kubernetes)
- [Configuração](#configuração)
- [Verificação](#verificação)

## Visão geral

O projeto utiliza [OpenTelemetry](https://opentelemetry.io/) (OTel) para coleta de traces, métricas e logs estruturados.
Os dados são enviados via OTLP (gRPC) para o [Datadog Agent](https://docs.datadoghq.com/agent/), que encaminha ao Datadog.

A aplicação não possui dependência com o Datadog — utiliza o SDK do OpenTelemetry.
Para trocar o backend, basta alterar a variável `OTEL_EXPORTER_OTLP_ENDPOINT`.


## Por que OpenTelemetry?

- **Vendor-neutral**: a aplicação não possui dependência com nenhum fornecedor de observabilidade
- **Padrão da indústria**: suportado por todos os principais backends (Datadog, Grafana, Jaeger, Dynatrace, etc.)
- **Instrumentação automática**: spans de HTTP, SQL e HttpClient são gerados sem código manual
- **Propagação W3C**: o contexto de trace é propagado automaticamente entre serviços via header `traceparent`
- **Correlação completa**: logs, traces e métricas compartilham os mesmos atributos de identificação (`service.name`, `service.version`, `deployment.environment`)

## Arquitetura

```
┌──────────────────────────────────────┐
│           Mechanics API              │
│           (.NET 8)                   │
│                                      │
│  Serilog (JSON)       → stdout       │─── logs ────▶  stdout (coletado pelo K8s / Docker)
│  OTel SDK traces      → OTLP gRPC   │─── :4317 ──▶  Datadog Agent → Datadog APM
│  OTel SDK metrics     → OTLP gRPC   │─── :4317 ──▶  Datadog Agent → Datadog Metrics
│                                      │
│  DatadogTraceEnricher → dd.trace_id  │─── correlação logs ↔ traces
│  CorrelationIdMiddleware → X-Corr-ID │─── rastreabilidade entre chamadas
└──────────────────────────────────────┘
```

O [Datadog Agent](https://docs.datadoghq.com/agent/) roda como container (local) ou DaemonSet (Kubernetes) e recebe dados via [OTLP Receiver](https://docs.datadoghq.com/opentelemetry/config/otlp_receiver/) na porta 4317 (gRPC).

### Instrumentação automática

| Operação | O que gera |
|----------|------------|
| Requisições HTTP de entrada | Spans com método, rota e status code |
| Chamadas HTTP de saída | Spans com propagação W3C |
| Queries SQL (EF Core) | Spans com `db.statement` (desabilitado em prod) |
| Runtime .NET | Métricas de GC, thread pool e heap |

Os endpoints `/health` e `/swagger` não geram spans.
Em produção, o sampling é de 10%. Nos demais ambientes, 100%.

### Correlação logs ↔ traces

O `DatadogTraceEnricher` injeta `dd.trace_id` e `dd.span_id` nos logs do Serilog.
Isso permite navegar de um log diretamente para o trace no Datadog.
O `CorrelationIdMiddleware` propaga o header `X-Correlation-ID` em todas as requisições.


## Execução local

O Docker Compose está configurado para subir o Datadog Agent junto com a aplicação.
É **obrigatório** definir a variável de ambiente `DD_API_KEY` antes de executar (o compose falhará sem ela).

### Obter a API Key

1. Crie uma conta em [datadoghq.com](https://www.datadoghq.com/)
2. Acesse **Organization Settings → API Keys**
3. Copie a chave

### Subir o projeto

**PowerShell:**
```powershell
$env:DD_API_KEY = "sua_api_key"
docker compose up -d --build
```

**Bash / Linux / macOS:**
```bash
export DD_API_KEY="sua_api_key"
docker compose up -d --build
```

### Sem Datadog (apenas console)

Para rodar sem o Datadog, é necessário fazer três ajustes no `docker-compose.yml`:

1. Remover o serviço `datadog-agent`
2. Remover a variável `OTEL_EXPORTER_OTLP_ENDPOINT` do serviço `dotnet`
3. Remover o `depends_on: datadog-agent` do serviço `dotnet`

Em ambiente `Development`, os traces e métricas são exibidos no console:

```bash
docker compose logs -f dotnet
```

## Execução no Kubernetes

O Datadog Agent é instalado no cluster EKS como **DaemonSet** via Helm chart gerenciado pelo Terraform, no repositório [`mechanics-infra`](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-infra).

A configuração está em:

| Arquivo | Repositório | Descrição |
|---------|-------------|-----------|
| `k8s/addons.tf` | `mechanics-infra` | Helm release do Datadog Agent |
| `k8s/vars.tf` | `mechanics-infra` | Variável `dd_api_key` |
| `.github/workflows/infra.yml` | `mechanics-infra` | Passa `DD_API_KEY` para o módulo Terraform |

### Adicionar secret no GitHub

Acesse o repositório **[mechanics-infra](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-infra)** → **Settings → Secrets and variables → Actions → Repository secrets** e adicione:

| Secret | Valor |
|--------|-------|
| `DD_API_KEY` | API Key do Datadog |

### Como funciona

1. A pipeline de infra passa `DD_API_KEY` como variável do Terraform (`TF_VAR_dd_api_key`)
2. O Terraform instala o Datadog Agent como DaemonSet via Helm (somente se a key não estiver vazia)
3. O Agent escuta na porta 4317 (gRPC) em cada node do cluster (`useHostPort: true`)
4. O `deployment.yaml` da aplicação resolve o endpoint automaticamente via `HOST_IP`:
   ```yaml
   - name: HOST_IP
     valueFrom:
       fieldRef:
         fieldPath: status.hostIP
   - name: OTEL_EXPORTER_OTLP_ENDPOINT
     value: "http://$(HOST_IP):4317"
   ```

## Configuração

### Variáveis de ambiente

| Variável | Descrição | Obrigatória | Padrão |
|----------|-----------|:-----------:|--------|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Endpoint do receptor OTLP (gRPC) | Não | NaN |
| `SERVICE_VERSION` | Versão do serviço | Não | `0.0.0` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente .NET (`Development`, `Staging`, `Production`) | Sim | `Production` |
| `DD_API_KEY` | API Key do Datadog | Sim* | — |

\* Necessária apenas para o container do Datadog Agent, não para a aplicação .NET.


## Verificação

### Checklist

1. Defina `DD_API_KEY` no ambiente
2. Suba o projeto com `docker compose up -d --build`
3. Acesse http://localhost:5000/swagger
4. Faça login via `POST /api/auth/login` (CPF `12345678909`, senha `5eCre+Key`)
5. Chame qualquer endpoint autenticado (ex: `GET /api/work-orders`)
6. Aguarde ~2 minutos para os dados serem processados
7. Acesse https://us5.datadoghq.com/apm/traces
8. Filtre por `service:mechanics-api`
9. Verifique os spans de HTTP e SQL nos traces
10. Acesse **Logs** e procure por `dd.trace_id` para validar a correlação

### Troubleshooting

| Problema | Causa | Solução |
|----------|-------|---------|
| `docker compose up` falha | `DD_API_KEY` não definida | Defina a variável antes de executar |
| Traces não aparecem no Datadog | Agent não está healthy | `docker compose logs datadog-agent` |
| Logs sem `dd.trace_id` | Request fora de um span ativo | Verifique se o endpoint não está filtrado |
| Apenas console output, sem export | `OTEL_EXPORTER_OTLP_ENDPOINT` vazio | Verifique o `docker-compose.yml` |

## Referências

- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/dotnet/)
- [Datadog OTLP Receiver](https://docs.datadoghq.com/opentelemetry/config/otlp_receiver/)
- [Datadog Agent Docker](https://docs.datadoghq.com/containers/docker/)
- [Datadog Agent Kubernetes (Helm)](https://docs.datadoghq.com/containers/kubernetes/installation/?tab=helm)
- [Serilog Enrichers](https://github.com/serilog/serilog/wiki/Enrichment)
- [Datadog Log-Trace Correlation](https://docs.datadoghq.com/tracing/other_telemetry/connect_logs_and_traces/dotnet/)