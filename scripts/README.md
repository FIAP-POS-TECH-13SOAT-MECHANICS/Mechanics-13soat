# Scripts para deploy

1. Configure suas credenciais na AWS:
   - Usando AWS CLI: `aws configure`; ou,
   - Colando no arquivo de configuração (se já existir): `notepad $ENV:USERPROFILE\.aws\credentials`;
2. Utilize o script `start-infra.ps1` para subir o ambiente na AWS.
3. Utilize o script `deploy-image.ps1` para subir a aplicação.
   - Repita o comando para gerar uma nova release.

O comando abaixo sobe a infraestrutura e faz deploy no ambiente DEV.
Execute na raiz do projeto.

```powershell
.\scripts\start-infra.ps1 dev; .\scripts\deploy-image.ps1 dev
```

Para alternar entre ambientes, use o script `set-environment.ps1`.

Os scripts são idempotentes, isto é, podem ser executados múltiplas vezes.
Isto torna alguns comandos ligeiramente diferentes daqueles mostrados em [Informações sobre a Infraestrutura](../infra/README.md).

## Permissão de execução de scripts

No Windows, a execução de scripts do Powershell vem desabilitada por padrão.

Para habilitar, abra um terminal como administrador e utilize o comando [Set-ExecutionPolicy](https://learn.microsoft.com/pt-br/powershell/module/microsoft.powershell.security/set-executionpolicy):

```powershell
Set-ExecutionPolicy Unrestricted
```