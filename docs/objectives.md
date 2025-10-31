# Objetivos

O projeto consiste em um sistema de informação para gerenciar ordens de serviço em uma oficina mecânica de médio porte. É possível cadastrar os clientes e veículos e gerenciar o fluxo dos pedidos, incluindo notificação e aprovação dos clientes.

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
