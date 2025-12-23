# Scripts para deploy

1. Configure suas credenciais na AWS:
   - Usando AWS CLI: `aws configure`; ou,
   - Colando no arquivo de configuração (se já existir): `notepad $ENV:USERPROFILE\.aws\credentials`;
2. Utilize o script `start-infra.ps1` para subir o ambiente na AWS.
3. Utilize o script `deploy-image.ps1` para subir a aplicação.
   - Repita o comando para gerar uma nova release.

Para alternar entre ambientes, use o script `set-environment.ps1`.

## Permissão de execução de scripts

No Windows, a execução de scripts do Powershell vem desabilitada por padrão.

Para habilitar, abra um terminal como administrador e utilize o comando [Set-ExecutionPolicy](https://learn.microsoft.com/pt-br/powershell/module/microsoft.powershell.security/set-executionpolicy):

```powershell
Set-ExecutionPolicy Unrestricted
```