# Criação de novos microsserviços

Siga os passos abaixo para criar um novo serviço no projeto.
Ao concluir, ajuste o README na raiz desse repositório para incluir o novo serviço, se necessário.

## Container Registry e banco de dados

Acesse o repositório [mechanics-infra](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-infra) e altere a variável `service_names` na camada `cr` para criar o repositório para o serviço.

Acesse também o repositório [mechanics-database](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-database) e siga as instruções para criar o banco de dados do serviço.

## Repositório da aplicação

O repositório [mechanics-example](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-example) contém uma versão monolítica do projeto.
Crie o repositório a partir desse template seguindo o padrão `mechanics-SERVICE` e clone ele.
Logo após clonar, crie uma nova feature-branch.

>Não faça nenhuma alteração antes de criar uma nova branch!

O primeiro passo é remover tudo que não faz parte do domínio do serviço.
Por exemplo, para o serviço de cadastro de usuários, todos os AppServices, controllers, entidades e testes relacionados a outras partes do projeto devem ser excluídas, como o fluxo de ordens de serviço ou de cadastro de veículos.

Faça os seguintes ajustes no projeto:

- Seção `AppInfo` no arquivo `appsettings.json`: informações do serviço, como nome, descrição e prefixo
- ConnectionString no arquivo `appsettings.json`: altere o nome do banco para o nome do serviço
- Arquivo `values.yaml` na pasta `k8s`: informações de deploy e URL
- Cabeçalho no `README.md` do repositório: nome e descrição
- Arquivo `docker-compose.yml`: nome e connectionString com o nome do serviço
- Nome da solution na pasta `src`: mantém consistência entre os projetos

```plain
mechanics-service/
├── src/
│   ├── Mechanics.Api
│   │   └── appsettings.json
│   └── Mechanics.Service.sln
├── k8s/
│   └── values.yaml
├── docker-compose.yml
└── README.md
```

O template também não tem nenhuma migração do banco de dados.
Remova as entidades e mapeamentos que não estão relacionadas ao serviço e siga as instruções de [migrações](./migrations.md) (caso utilize um banco de dados relacional).

Acesse as configurações do repositório e crie uma regra bloqueando commits na branch `main` sem abertura de PRs.

Por fim, as opções do GitHub Actions no repositório e crie as variáveis e secrets abaixo:

| Nome                  | Tipo     |
|-----------------------|----------|
| AWS_ACCESS_KEY_ID     | Secret   |
| AWS_SECRET_ACCESS_KEY | Secret   |
| AWS_SESSION_TOKEN     | Secret   |
| SERVICE_NAME          | Variable |
