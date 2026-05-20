# SonarQube

## Contexto

Durante a migração para a Fase 4, o time precisou padronizar a análise de qualidade dos microsserviços com SonarQube em pipeline.

No ambiente AWS Academy (Student Lab), tivemos limitações operacionais para manter o SonarQube dentro da mesma esteira de infraestrutura dos demais serviços, como:

- credenciais temporárias e políticas restritas no laboratório;
- instabilidade de operações de state lock/desbloqueio em S3 durante `terraform apply`;
- custo de manter toda a pilha de EKS/NLB apenas para o SonarQube.

## Decisão adotada

O SonarQube foi desacoplado da infraestrutura principal e passou a rodar em instância externa dedicada.

Com isso:

- o `mechanics-infra` mantém o workflow reutilizável de testes (`dotnet-tests.yml`) com suporte a Sonar;
- o provisionamento e a governança do Sonar ficam centralizados no repositório `mechanics-sonar-plataform`;
- o `mechanics-database` e o `mechanics-infra` deixam de provisionar camada SonarQube no fluxo padrão.

## Operação

As instruções operacionais (tokens, bootstrap de projetos, chaves por repositório e troubleshooting) estão em:

- [mechanics-sonar-plataform/docs/operations.md](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-sonar-plataform/blob/main/docs/operations.md)

Fluxos automatizados de governança:

- [platform-governance.yml](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-sonar-plataform/blob/main/.github/workflows/platform-governance.yml)
- [sonarqube-projects.json](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-sonar-plataform/blob/main/scripts/sonarqube-projects.json)
