# Scripts para deploy

1. Certifique-se de ter instalado as ferramentas necessárias:
   - AWS CLI
   - Helm
2. Configure suas credenciais na AWS:
   - Usando AWS CLI: `aws configure`; ou,
   - Colando no arquivo de configuração (se já existir): `notepad $ENV:USERPROFILE\.aws\credentials`;
3. Utilize o script `deploy-image.ps1` para subir a aplicação.
   - Repita o comando para gerar uma nova release.

O comando abaixo sobe a infraestrutura e faz deploy no ambiente DEV.
Execute na raiz do projeto.

```powershell
.\scripts\deploy-image.ps1 dev
```

Para alternar entre ambientes, use o script `set-environment.ps1`.
Os scripts são idempotentes, isto é, podem ser executados múltiplas vezes.

## Permissão de execução de scripts

No Windows, a execução de scripts do Powershell vem desabilitada por padrão.

Para habilitar, abra um terminal como administrador e utilize o comando [Set-ExecutionPolicy](https://learn.microsoft.com/pt-br/powershell/module/microsoft.powershell.security/set-executionpolicy):

```powershell
Set-ExecutionPolicy Unrestricted
```