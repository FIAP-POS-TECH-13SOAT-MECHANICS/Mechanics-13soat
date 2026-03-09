# Autenticação e autorização

O fluxo de geração e renovação de token foi movido para o [serviço de autenticação](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/mechanics-auth).
Consulte a documentação para realizar o login.

## Perfis de acesso

- **Administrador**: Acesso completo ao sistema. É o único que pode cadastrar novos usuários.
- **Atendente**: Possui as permissões para cadastrar novos clientes e seus veículos.
- **Mecânico**: Possui as permissões para cadastrar e alterar produtos e serviços oferecidos.

No fluxo de ordens de serviço, os perfis Atendente e Mecânico possuem alguns acessos específicos. Acesse [Fluxo de Ordem de Serviço](./work-order-flow.md) para mais detalhes.

## Criação de novo usuário

Somente administradores podem cadastrar e alterar usuários.
O cadastro pode ser feito pelo endpoint `POST /api/auth/users`.
O ID do perfil do usuário pode ser consultado no endpoint `GET /api/auth/roles`.

```shell
curl -X 'POST' \
  'http://localhost:5000/api/auth/users' \
  -H 'accept: application/json' \
  -H 'Authorization: Bearer ...' \
  -H 'Content-Type: application/json' \
  -d '{
  "fullName": "string",
  "cpfNumber": "11144477735",
  "email": "user@example.com",
  "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}'
```

### Usuários de Clientes

Clientes PJ podem ter seus próprios usuários para gerenciar suas ordens de serviço.
Ao criar um cliente empresarial (`POST /api/customers/business`), o sistema cria automaticamente um usuário com o perfil `CUSTOMER_ADMIN` vinculado a esse cliente.

Com esse usuário, o cliente pode realizar o login e gerenciar seus próprios usuários adicionais através do endpoint `api/customers/users`. Estes novos usuários terão o perfil `CUSTOMER_USER`.

Para criar um novo usuário de cliente:
1. Faça login como um funcionário (Admin ou Atendente).
2. Crie um cliente PJ (`POST /api/customers/business`).
3. Faça login com o CPF do responsável definido no cadastro do cliente PJ.
4. Chame o endpoint `POST /api/customers/users` para cadastrar novos usuários.

```shell
curl -X 'POST' \
  'http://localhost:5000/api/customers/users' \
  -H 'accept: application/json' \
  -H 'Authorization: Bearer ...' \
  -H 'Content-Type: application/json' \
  -d '{
  "fullName": "João do Cliente",
  "email": "joao@cliente.com",
  "cpfNumber": "111.444.777-35"
}'
```

O usuário receberá um e-mail para criação da senha, seguindo o mesmo fluxo de usuários internos.

```shell
curl -X 'POST' \
  'http://localhost:5000/api/auth/create-password' \
  -H 'accept: application/json' \
  -H 'Authorization: Bearer ...' \
  -H 'Content-Type: application/json' \
  -d '{
  "cpfNumber": "11144477735",
  "passwordCreationCode": "B2C16D975B6AD83144A8533E804AA43D",
  "password": "4Nz9c5uQ(&sHZCX"
}'
```

## Recuperação de senha

Para recuperar a senha, utilize o endpoint `POST /api/auth/reset-password`.
O usuário receberá um link no e-mail cadastrado e deve enviar para o endpoint `POST /api/auth/create-password`.

```shell
curl -X 'POST' \
  'http://localhost:5000/api/auth/reset-password' \
  -H 'accept: application/json' \
  -H 'Authorization: Bearer eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJmaWFwLW1lY2hhbmljcyIsImV4cCI6MTc2MjA0NjA1NywiaWF0IjoxNzYyMDQ0MjU3LCJuYmYiOjE3NjIwNDQyNTcsInN1YiI6IjBjNWJjZTQ0LWIxMTItNGI1My1iOTRjLWUyY2I2YTkyNDVmNSIsInVzZXJOYW1lIjoic3RyaW5nIiwicm9sZSI6IkFUVEVOREFOVCJ9.a8VU9Hdu4ttEMmaXhODV5NHuOK0subCxQ75tTRP9nIw' \
  -H 'Content-Type: application/json' \
  -d '{
  "cpfNumber": "11144477735"
}'
```

O endpoint sempre retorna `HTTP 204`, independentemente do login informado existir.
