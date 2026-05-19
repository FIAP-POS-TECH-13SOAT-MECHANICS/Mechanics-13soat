# Criação de novos microsserviços

Siga os passos abaixo para criar um novo serviço no projeto.
Ao concluir, ajuste o README na raiz desse repositório para incluir o novo serviço, se necessário.

## Container Registry e banco de dados

Acesse o repositório [mechanics-infra](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-infra) e altere a variável `service_names` na camada `cr` para criar o repositório para o serviço.

Acesse também o repositório [mechanics-database](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-database) e siga as instruções para criar o banco de dados do serviço.

## Repositório da aplicação

O repositório [mechanics-example](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-example) contém uma versão monolítica do projeto.
Crie o repositório a partir desse template seguindo o padrão `mechanics-SERVICE` e clone-o.
Logo após clonar, crie uma nova feature-branch:

```bash
git checkout -b feature/init
```

> Não faça nenhuma alteração antes de criar uma nova branch!

O primeiro passo é remover tudo que não faz parte do domínio do serviço.
Por exemplo, para o serviço de cadastro de usuários, todos os AppServices, controllers, entidades e testes relacionados a outras partes do projeto devem ser excluídas, como o fluxo de ordens de serviço ou de cadastro de veículos.

Faça os seguintes ajustes no projeto:

- Seção `AppInfo` no arquivo `appsettings.json`: informações do serviço, como nome, descrição e prefixo das rotas
- ConnectionString no arquivo `appsettings.json`: altere o nome do banco para o nome do serviço
- Arquivo `values.yaml` na pasta `k8s`: informações de deploy e URL
- Cabeçalho no `README.md` do repositório: nome e descrição
- Arquivo `docker-compose.yml`: nome e connectionString com o nome do serviço
- Nome da solution na raiz do repositório: mantém consistência entre os projetos
- Crie o `appsettings.Development.json`: siga as [instruções](./configuration.md) para configurar o ambiente de desenvolvimento, como credenciais da AWS

```text
mechanics-service/
├── src/
│   ├── Mechanics.Api
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
├── k8s/
│   └── values.yaml
├── docker-compose.yml
├── README.md
└── Mechanics.Service.sln
```

O template também não tem nenhuma migração do banco de dados.
Remova as entidades e mapeamentos que não estão relacionadas ao serviço e siga as instruções de [migrações](./migrations.md) (caso utilize um banco de dados relacional).
O comando abaixo sobe um container Docker para o Microsoft SQL Server, baixa as dependências do projeto e cria uma migração chamada `Init`.

```powershell
docker compose up mssql -d
dotnet restore
dotnet ef migrations add Init --project src\Mechanics.Infra.Data --startup-project src\Mechanics.Api
```

Acesse as configurações do repositório e crie uma branch protection rule bloqueando commits na branch `main` sem abertura de PR.

Por fim, acesse as opções do GitHub Actions no repositório e crie as variáveis e secrets abaixo:

| Nome                  | Tipo     | Descrição                                  |
|-----------------------|----------|--------------------------------------------|
| AWS_ACCESS_KEY_ID     | Secret   | Credenciais da AWS Academy                 |
| AWS_SECRET_ACCESS_KEY | Secret   | Credenciais da AWS Academy                 |
| AWS_SESSION_TOKEN     | Secret   | Credenciais da AWS Academy                 |
| AWS_REGION            | Variable | Use o valor `us-east-1`                    |
| SERVICE_NAME          | Variable | Nome do serviço sem o prefixo `mechanics-` |

## Execução do projeto

Tente executar o projeto, criando antes um banco de dados e instância do MailPit no Docker.

```powershell
docker compose up mssql mailpit -d
dotnet run --project .\src\Mechanics.Api
```

> Certifique-se de não ter outros containers em execução para evitar conflito de portas.

O Swagger estará disponível na URL abaixo. Ajuste de acordo com o prefixo do serviço (informado na appsettings).

```text
http://localhost:5000/SERVICE_NAME/swagger
```

Para acessar as rotas no Swagger, utilize o script para gerar um token:

```powershell
.\scripts\new-token.ps1
```

Se conseguir chamar as rotas corretamente, execute os testes antes de fazer commit.
Lembre-se revisar o prefixo das rotas nos testes de integração e BDD.

```powershell
dotnet test
```

## Checklist de conclusão

Revise todos os pontos abaixo para considerar o trabalho como concluído:

1. Infraestrutura provisionada
   1. Crie o Repositório ECR via camada `cr` do [mechanics-infra](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-infra)
   2. Crie as filas SQS via [mechanics-infra](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-infra), se aplicável
   3. Crie o Banco de dados via [mechanics-database](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-database)
   4. Inclua o novo serviço na lista do [README](../README.md#microsserviços-do-projeto) deste repositório
2. Projeto executa localmente
   1. Suba os containers de dependências com `docker compose up mssql mailpit localstack -d`
   2. Execute a aplicação com `dotnet run --project .\src\Mechanics.Api`
   3. Execute os testes com `dotnet test` e confirme que nenhum falhou
   4. Execute o projeto via Docker Compose com `docker compose up` e confirme que iniciou corretamente
3. Configurações do repositório estão corretas
   1. Confirme que nenhum commit foi feito diretamente na branch `main`
   2. Crie a regra de proteção de branch bloqueando commits diretos na `main` (ver [documentação do GitHub](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/managing-a-branch-protection-rule))
   3. Crie as variáveis e secrets nas configurações do repositório (ver [documentação do GitHub](https://docs.github.com/en/actions/security-for-github-actions/security-guides/using-secrets-in-github-actions))
4. Pipeline funcionando corretamente
   1. Crie um PR da feature-branch para `develop` e confirme que os testes rodaram
   2. Aprove o PR e confirme que a pipeline completou sem erros
   3. Crie um PR para `main` e repita a verificação
   4. Faça o merge na `main` via PR e confirme que a pipeline completou sem erros
5. Deploy efetuado
   1. Confirme que a imagem no ECR possui a tag correspondente ao último commit
   2. Confirme que o pod está em execução com `kubectl get pods -n SERVICE_NAME`
   3. Gere um token via `mechanics-auth` e teste as rotas do serviço pelo API Gateway
