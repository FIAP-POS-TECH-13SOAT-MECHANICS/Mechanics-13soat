# Fiap.Mechanics

Repositório do projeto destinado aos Tech Challenges da Oficina Mecânica da FIAP.

A implementação está dividida em 5 fases. Para os detalhes de cada fase, veja
os [objetivos do projeto](./docs/objectives.md).

- [x] **Fase 1**: Implementação inicial em arquitetura monolítica
- [x] **Fase 2**: Evolução da arquitetura e CI/CD para infraestrutura e deploy
- [x] **Fase 3**: Implementação de login via AWS Lambda e monitoramento
- [x] **Fase 4**: Migração para microsserviços e SAGA pattern
- [ ] Fase 5

O código da aplicação evoluiu até a Fase 3, contemplando a arquitetura monolítica, Kubernetes e pipelines de CI/CD. A partir da Fase 4, o repositório passou a ser utilizado como documentação central do projeto, com os microsserviços mantidos em repositórios independentes.

## Microsserviços do projeto

Consulte [Microsserviços](./docs/microservices.md) para detalhes.
Acesse [Criação de novos microsserviços](./docs/create-microservices.md) para instruções de como criar um novo.

<!-- Mantenha a lista em ordem alfabética -->

- [auth (serverless)](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-auth): Lambda Function para autenticação
- [billing](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-billing): controle de orçamentos e pagamentos
- [execution](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-execution): execução das ordens de serviço
- [identity](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-identity): cadastro e permissões de usuários
- [work-orders](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-work-orders): gestão de ordens de serviço

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
- [Criação de novos microsserviços](./docs/create-microservices.md)
- [Dados de exemplo](./dev-seeds/README.md)
- [Decisões técnicas](./docs/technical-decisions.md)
- [Diagramas](./docs/diagrams.md)
- [Diretrizes de design do projeto](./docs/design-guidelines.md)
- [Filas](./docs/queues.md)
- [Fluxo de Ordem de Serviço](./docs/work-order-flow.md)
- [Fluxos SAGA](./docs/saga.md)
- [Integração entre microsserviços](./docs/integration.md)
- [Kubernetes e Helm chart](./k8s/README.md)
- [Messageria](./docs/messaging.md)
- [Microsserviços](./docs/microservices.md)
- [Migrações do banco de dados](./docs/migrations.md)
- [Objetivos](./docs/objectives.md)
- [Relatórios](./docs/reports/README.md)
- [Scripts para deploy](./scripts/README.md)
- [SonarQube](./docs/sonarqube.md)
