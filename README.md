# Fiap.Mechanics

[![.NET](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/Mechanics-13soat/actions/workflows/ci-cd.yml/badge.svg?branch=main)](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/Mechanics-13soat/actions/workflows/ci-cd.yml)

Repositório do projeto destinado aos Tech Challenges da Oficina Mecânica da FIAP.

A implementação está dividida em 5 fases. Para os detalhes de cada fase, veja
os [objetivos do projeto](./docs/objectives.md).

- [x] **Fase 1**: Implementação inicial em arquitetura monolítica
- [x] **Fase 2**: Evolução da arquitetura e CI/CD para infraestrutura e deploy
- [x] **Fase 3**: Implementação de login via AWS Lambda e monitoramento
- [ ] Fase 4
- [ ] Fase 5

## Definição do ambiente

- SDK: .NET 8.0
- Banco de dados: MSSQL 2025
- Serviço de E-mail: MailPit
- Chave pública para JWT: AWS Secrets Manager

## Execução do projeto

Em cada nova fase do projeto, é recomendável apagar os volumes do Docker para evitar conflitos com a estrutura do banco
de dados criado em fases anteriores. Para fazer isso, execute o seguinte comando na raiz do projeto:

```bash
docker compose down -v
```

Baixe a chave pública do AWS Secrets Manager (ajuste o nome de acordo o ambiente):

```powershell
aws secretsmanager get-secret-value --secret-id "fiap-mechanics-dev-jwt/public-key" --query SecretString --output text > "src/Mechanics.Api/keys/jwt-public.pem"
```

Defina a variável de ambiente com a ApiKey do DataDog:

```powershell
# somente sesão atual do terminal
$env:DD_API_KEY = 'xxx'
# persistir nas variáveis de ambiente no Windows
[System.Environment]::SetEnvironmentVariable('DD_API_KEY', 'xxx', 'User')
```

Inicie o projeto via Docker Compose:

```bash
docker compose up -d --build
```

Após o processo concluir, o projeto estará disponível nas seguintes URLs:

- Swagger do projeto: <http://localhost:5000/swagger>
- Cliente de e-mail: <http://localhost:8025>

> **Opcional**
> Utilize o script [dev-seeds](./dev-seeds/README.md) para popular o banco com dados de exemplo.

## Opções do projeto

As configurações são definidas em arquivos `appsettings`.
Para execução local, crie [um arquivo `appsettings.Development`](./docs/configuration.md).
Algumas dessas configurações também poder ser definidas no [Helm chart](./k8s/README.md).

| Branch    | Ambiente    | Swagger | EF Migrations          |
|-----------|-------------|---------|------------------------|
| `main`    | Production  | Não     | Somente via Helm chart |
| `release` | Staging     | Sim     | Somente via Helm chart |
| `develop` | Development | Sim     | Executadas ao iniciar  |

## Usuários padrão

Utilize o [serviço de autenticação](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-auth) para gerar um
token.
O token possui validade de poucos minutos, mas pode ser renovado.
Veja as instruções em [Autenticação e autorização](./docs/auth.md).

Os seguintes logins podem ser utilizados para testes:

| CPF           | Senha       | Perfil        | Permissões                    |
|---------------|-------------|---------------|-------------------------------|
| `12345678909` | `5eCre+Key` | Administrador | Acesso completo ao sistema    |
| `98765432100` | `5eCre+Key` | Atendente     | Cadastrar clientes e veículos |
| `11144477735` | `5eCre+Key` | Mecânico      | Gerenciar produtos e serviços |

Qualquer usuário autenticado pode criar e atualizar ordens de serviço.

## Análises de Qualidade e Segurança

Os scripts para gerar análises de qualidade de código, vulnerabilidades e conformidade de segurança do projeto são
mantidas em um repositório dedicado.
Acesse o repositório pelo link abaixo.

[Mechanics Sonar](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/Mechanics-Sonar)

Esse repositório contém instruções para execução local das análises com **SonarQube**, **OWASP Dependency Check** e
**OWASP ZAP**.

## Links úteis

<!-- Mantenha a lista em ordem alfabética -->

- [Configuração do ambiente](./docs/configuration.md)
- [Dados de exemplo](./dev-seeds/README.md)
- [Diagramas](./docs/diagrams.md)
- [Diretrizes de design do projeto](./docs/design-guidelines.md)
- [Fluxo de Ordem de Serviço](./docs/work-order-flow.md)
- [Kubernetes e Helm chart](./k8s/README.md)
- [Migrações do banco de dados](./docs/migrations.md)
- [Objetivos](./docs/objectives.md)
- [Relatórios](./docs/reports/README.md)
- [Scripts para deploy](./scripts/README.md)
