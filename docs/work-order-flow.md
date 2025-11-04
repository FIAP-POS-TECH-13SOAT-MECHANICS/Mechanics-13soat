### Resumo do fluxo de Ordem de Serviço.

Atores: Cliente, Atendente, Mecânico, Sistema e Serviço de Notificação.
Objetivo: Receber veículo, diagnosticar, gerar orçamento, obter aprovação do cliente, executar e finalizar serviço e, por fim, devolver veículo. 

### Descrição detalhada
1. Cadastro
- O atendente cadastra o cliente e o veículo no sistema.
- É obrigatório informar um endereço de e‑mail válido para envio das notificações.

2. Abertura da OS
- O atendente registra a OS com o problema relatado.
- O sistema gera automaticamente uma chave/código de acesso e envia um e‑mail ao cliente com o código e instruções para acompanhamento.

3. Análise / Orçamento
- O mecânico realiza o diagnóstico e registra no sistema os produtos e serviços necessários (itens do orçamento).
- Ao finalizar a análise, o sistema envia um e‑mail ao cliente informando que a OS está aguardando aprovação, com resumo do orçamento.

4. Aprovação
- O cliente acessa a OS usando CPF/CNPJ e a chave de acesso enviada por e‑mail.
- O cliente aprova ou rejeita o orçamento:
- Se aprovar, o sistema notifica a oficina e libera a execução.
- Se rejeitar, o cliente pode registrar observações e a OS retorna para nova análise.

5. Execução
- Com o orçamento aprovado, o mecânico atualiza a OS e começa a realizar o serviço.
- Ao concluir, o mecânico registra a finalização no sistema.
- O sistema notifica o cliente que o veículo está pronto para retirada.

6. Devolução / Encerramento
- O cliente retira o veículo; o atendente confere e encerra a OS.
- O sistema envia um e‑mail final ao cliente confirmando a entrega.

### Diagrama
![FluxodaOS](images/d.png)

