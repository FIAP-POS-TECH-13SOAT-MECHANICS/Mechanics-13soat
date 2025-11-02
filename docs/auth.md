# Autenticação e autorização

## Perfis de acesso

- **Administrador**: Acesso completo ao sistema. É o único que pode cadastrar novos usuários.
- **Atendente**: Possui as permissões para cadastrar novos clientes e seus veículos.
- **Mecânico**: Possui as permissões para cadastrar e alterar produtos e serviços oferecidos.

OBS.: Todos os perfis possuem acesso pada criar e atualizar as ordens de serviço.

## Login

Para realizar login, utilize o endpoint `POST /api/auth/login`.

```shell
curl --location 'http://localhost:5000/api/auth/login' \
--header 'Content-Type: application/json' \
--data '{
    "userName": "administrator",
    "password": "5eCre+Key"
}'
```

A resposta contém o token de acesso e o de atualização.
O token de acesso possui uma validade de poucos minutos e pode ser renovado utilizando o token de atualização.

```json
{
    "accessToken": "...",
    "refreshToken": "...",
    "expirationDate": "2025-11-02T00:39:54.1120047+00:00"
}
```

Para renovar o token de acesso, utilize o endpoint `POST /api/auth/refresh-token`.

```shell
curl --location 'http://localhost:5000/api/auth/refresh' \
--header 'accept: application/json' \
--header 'Content-Type: application/json' \
--data '{
  "refreshToken": "..."
}'
```

O token de atualização é válido por 12 horas e é cancelado quando o usuário altera a senha.

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
  "userName": "string",
  "email": "user@example.com",
  "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}'
```

O usuário receberá um e-mail no endereço informado com o código para a criação da senha.
Ele deve usar enviar esse código e a senha para o endpoint `POST /api/auth/create-password`.
A senha deve conter letras maiúsculas e minúsculas, números e símbolos.

```shell
curl -X 'POST' \
  'http://localhost:5000/api/auth/create-password' \
  -H 'accept: application/json' \
  -H 'Authorization: Bearer ...' \
  -H 'Content-Type: application/json' \
  -d '{
  "userName": "string",
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
  "userName": "string"
}'
```

O endpoint sempre retorna `HTTP 204`, independentemente do login informado existir.
