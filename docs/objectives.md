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

## Fluxo de atendimento

1. Cadastro do cliente
2. Cadastro do veículo
3. Geração da ordem de serviço *
4. Análise pelo mecânico
5. Envio do orçamento *
6. Aprovação pelo cliente
7. Realização do serviço
8. Devolução do veículo *

\* passos que geram notificação ao cliente

Quando um novo cliente chega ao estabelecimento, ele é cadastrado, juntamente com os dados do veículo. É necessário informar um endereço de e-mail para o envio das notificações.

Após o cadastro, um funcionário gera a ordem de serviço informando o problema relatado. É disparado um e-mail com um código de acesso da ordem de serviço que o cliente poderá utilizar para consultar o andamento.

Após análise, um mecânico lança os produtos e serviços no sistema e um novo e-mail é enviado para o cliente informando que a ordem está aguardando aprovação. O cliente pode consultar a ordem utilizando seu documento (CPF ou CNPJ) e a chave de acesso gerada e então aprovar ou rejeitar o serviço.

O serviço então é realizado e o cliente recebe outra notificação informando que o veículo está pronto para ser retirado. Um último e-mail é enviado quando o veículo for retirado e a ordem de serviço encerrada.

## Banco de dados

Optamos por utilizar um banco de dados relacional para garantir a consistência e integridade das informações, já que o sistema envolve diversas entidades relacionadas, como clientes, veículos, ordens de serviço e produtos.

O Microsoft SQL Server foi escolhido por conta da facilidade de integração com aplicações C#/.NET, oferecendo suporte nativo a bibliotecas como Entity Framework. A versão Express é gratuita e suporta uma base de dados de até 10 GB, o que é condizente para o escopo do projeto.

Além disso, o SQL Server apresenta um bom desempenho mesmo em ambientes de menor porte, com recursos como cache de consultas e otimização automática de índices. Isso o torna uma escolha mais completa em comparação com alternativas menores, como o MySQL, especialmente em um contexto de aplicação C#.

## Notificações por e-mail

Para o ambiente de desenvolvimento, o sistema utiliza a ferramenta [MailPit](mailpit.axllent.org). Ela fornece um servidor SMTP para simular o envio de mensagens.

Todos os e-mails enviados podem ser acessados pelo cliente web da ferramenta, que roda na porta [8025](http://localhost:8025/).
