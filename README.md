# Fiap.Mechanics

[![.NET](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/Mechanics-13soat/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/FIAP-POS-TECH-13SOAT-MECHANICS/Mechanics-13soat/actions/workflows/dotnet.yml)

Repositório do projeto destinado aos Tech Challenges da Oficina Mecânica da FIAP.

## Definição do ambiente

- SDK: .NET 8.0
- Banco de dados: MSSQL 2025

## Execução via Docker

1. Rode o comando `docker compose up -d --build` na raiz do projeto.
2. Aguarde o processo concluir e acesse `http://localhost:5000/swagger`.
3. Acesse o cliente de e-mail em `http://localhost:8025`.

## Usuários padrão

Os seguintes logins podem ser utilizados para testes:

| Usuário         | Senha       | Perfil        |
|-----------------|-------------|---------------|
| `administrator` | `5eCre+Key` | Administrador |
| `attendant`     | `5eCre+Key` | Atendente     |
| `mechanic`      | `5eCre+Key` | Mecânico      |

## Links úteis

<!-- Mantenha a lista em ordem alfabética -->

- [Configuração do ambiente](/docs/configuration.md)
- [Diretrizes de design do projeto](/docs/design-guidelines.md)
- [Migrações do banco de dados](/docs/migrations.md)
