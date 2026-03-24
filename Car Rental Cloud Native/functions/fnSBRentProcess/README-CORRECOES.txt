Correções aplicadas

1. Removido pacote que causava conflito com Application Insights:
   - Microsoft.ApplicationInsights.WorkerService

2. Mantida configuração correta para Azure Functions Isolated (.NET 8).

3. Ajustado Program.cs para iniciar a Function sem configuração extra conflitante.

4. Ajustado Function1.cs para ler a string de conexão do SQL com fallback no Azure:
   - ConnectionStrings:SqlConnectionString
   - SqlConnectionString
   - SQLCONNSTR_SqlConnectionString

5. Removidos arquivos temporários/obj e arquivos duplicados do projeto.

Como publicar

- No Visual Studio, clique com botão direito no projeto fnSBRentProcess > Publish
- Publique o projeto para a Function App existente
- No Azure, em Settings > Configuration da Function App, garanta estes nomes:
  * ServiceBusConnection
  * SqlConnectionString
  * APPLICATIONINSIGHTS_CONNECTION_STRING
- Reinicie a Function App após salvar as configurações

Como confirmar

- No portal do Azure, em Functions, deve aparecer: ProcessoLocacao
- Depois de enviar uma mensagem para a fila, a Function deve consumir a mensagem e gravar no SQL

Observação importante

As credenciais reais foram removidas deste pacote por segurança.
