# Objetivos

O projeto consiste em um sistema de informação para gerenciar ordens de serviço em uma oficina mecânica de médio porte. É possível cadastrar os clientes e veículos e gerenciar o fluxo dos pedidos, incluindo notificação e aprovação dos clientes.

## Fases do projeto

São 5 fases, cada uma com objetivos diferentes.

### Primeira fase

Implementação inicial do projeto, utilizando arquitetura monolítica. O projeto deve conter Dockerfile e docker-compose com todos os recursos necessários para execução local, como banco de dados e servidor SMTP.

O sistema deve disponibilizar CRUDs para clientes, veículos, peças e serviços oferecidos, com validação de CPF/CNPJ e placa dos veículos. Os usuários devem ser capazes de cadastrar ordens de serviço (OS) e acompanhar seu andamento, incluindo controle de status, envio de notificações e aprovação de orçamentos.

A autenticação deve ser realizada por meio de JWT. O projeto deve conter testes automatizados com, no mínimo, 80% de cobertura nos fluxos principais, além de um relatório de vulnerabilidades gerado a partir de ferramentas de análise, como SonarQube, OWASP e ZAP.

A documentação deve conter instruções para execução do projeto, bem como a justificativa para a escolha do banco de dados. O projeto deve aplicar conceitos de Domain-Driven Design (DDD), com link para os diagramas e artefatos no Miro.

### Segunda fase

O projeto deve ser revisado para se adequar às boas práticas de Clean Code e à evolução da arquitetura da aplicação, além de implementar automação de testes e deploy. Também devem ser realizados ajustes nos fluxos existentes de ordens de serviço, como melhorias na listagem de OS, com ocultação padrão das ordens entregues e revisão da ordenação.

A infraestrutura da aplicação deve ser preparada para ambientes além da execução local, incluindo a criação automatizada do ambiente via Terraform e o deploy em ambiente Kubernetes. A pipeline de CI/CD deve ser revisada e expandida para contemplar o provisionamento da infraestrutura, execução dos testes, build da aplicação e deploy automatizado.

A documentação deve ser revisada para incluir instruções para execução, provisionamento e deploy da aplicação em ambiente em nuvem, além de detalhes sobre o ambiente provisionado e os recursos criados.

### Terceira fase

Deve ser implementado monitoramento e observabilidade da aplicação e da infraestrutura, com coleta, armazenamento e visualização de métricas, logs e traces. A solução deve utilizar ferramentas como Datadog ou New Relic, incluindo monitoramento de latência, consumo de recursos, healthchecks, logs estruturados e alertas para falhas.

A aplicação também deve ser reorganizada em múltiplos repositórios (infraestrutura, banco de dados, função serverless e aplicação principal), todos com CI/CD e deploy automatizado. A documentação deve ser atualizada para refletir a arquitetura, decisões técnicas e indicadores monitorados.

Também devem ser elaborados diagramas de arquitetura, incluindo visão de componentes com infraestrutura em nuvem e fluxos de sequência para autenticação e gestão de ordens de serviço.

#### Endpoints exigidos

Os endpoints abaixo são requisitos para a segunda fase:

- **Abertura de Ordem de Serviço (OS)**
  - `POST /api/work-orders`
  - Recebe o ID de um veículo e cria uma nova ordem de serviço.
  - É possível adicionar o problema reportado pelo cliente, peças e serviços.
  - O veículo deve ser previamente cadastrado.
    - Utilize `POST /api/vehicles` para cadastrar.
    - O cliente é identificado através do cadastro do veículo. Use `POST /api/customers` para cadastrar.
  - É enviado um e-mail para o endereço do cliente.
  - Precisa estar autenticado (use `POST /api/auth/login`).
- **Consulta de status da OS**
  - `GET /api/work-orders/track`
  - Permite consultar a situação da ordem de serviço
  - É necessário informar o documento do cliente (CPF ou CNPJ) e a chave de acesso (número da Ordem de Serviço).
  - A chave de acesso é enviada por e-mail para o cliente.
  - Não precisa estar autenticado para fazer a consulta.
- **Aprovação de orçamento**
  - Links para aprovação e rejeição do orçamento são enviados por e-mail.
  - São enviados para o cliente por e-mail através da chamada `POST /api/work-orders/{workOrderId}/request-approval`
  - Ao clicar em um dos links do e-mail, o status da OS é alterado:
    - Se aprovado, muda para "em andamento";
    - Se rejeitado, volta para "em análise";
  - Os endpoints de aprovação e rejeição também permitem anexar um comentário (parâmetro `description`), mas essa funcionalidade não está disponível pelo e-mail.
  - **Listagem de orçamento**
    - `GET /api/work-orders`
    - Ordenado pelo status de forma decrescente (Em Execução, Aguardando Aprovação, Diagnóstico, Recebida) e depois pela data (mais antigas primeiro)
    - Por padrão, não exibe os itens concluídos (entregues)
      - Utilize `includeCompleted=true` para mostrar esses itens
    - A lista é paginada (padrão de 10 itens por página)
  - **Atualização de status da OS**
    - Ao alterar o status, um e-mail é enviado ao cliente.
    - O mecânico responsável também recebe uma mensagem quando o orçamento é aprovado.

Se estiver utilizando Docker Compose, o cliente de e-mail roda na porta [8025](http://localhost:8025).

### Quarta fase

A aplicação deve ser refatorada para uma arquitetura de microsserviços com gestão transacional
distribuída e automação completa de build, testes e deploy.

Cada microsserviço deve ter repositório, infraestrutura e banco de dados próprios. É obrigatório
o uso de pelo menos um banco relacional e pelo menos um banco não-relacional entre os serviços.
**Nenhum serviço pode acessar diretamente o banco de outro serviço.**

A comunicação entre microsserviços deve ser definida com APIs RESTful síncronas quando necessário
e mensageria assíncrona para eventos e integrações desacopladas.

Deve ser implementado o Saga Pattern para coordenar o fluxo transacional das ordens de serviço,
com rollback e compensação em caso de falha em qualquer etapa. A escolha entre orquestração e
coreografia deve ser documentada e justificada.

Cada microsserviço deve ter pipeline independente de CI/CD, cobertura mínima de 80% de testes,
pelo menos um fluxo completo testado com BDD, e validação de qualidade via SonarQube ou similar.
Os repositórios devem ter a branch `main` protegida com PR obrigatório e checagens automáticas.

A observabilidade deve reutilizar as ferramentas implementadas na Fase 3.
