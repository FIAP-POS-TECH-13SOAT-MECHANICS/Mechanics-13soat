# Scripts para deploy

1. Certifique-se de ter instalado as ferramentas necessárias:
   - AWS CLI
   - Helm
   - Terraform
2. Configure suas credenciais na AWS:
   - Usando AWS CLI: `aws configure`; ou,
   - Colando no arquivo de configuração (se já existir): `notepad $ENV:USERPROFILE\.aws\credentials`;
3. Utilize o script `initialize-infrastructure.ps1` para subir o ambiente na AWS.
4. Utilize o script `deploy-image.ps1` para subir a aplicação.
   - Repita o comando para gerar uma nova release.
5. Utilize o script `remove-environment.ps1` para destruir o ambiente.
   - Para evitar conflitos, o bucket S3 com os states do Terraform não é apagado.

O comando abaixo sobe a infraestrutura e faz deploy no ambiente DEV.
Execute na raiz do projeto.

```powershell
.\scripts\initialize-infrastructure.ps1 dev; .\scripts\deploy-image.ps1 dev
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