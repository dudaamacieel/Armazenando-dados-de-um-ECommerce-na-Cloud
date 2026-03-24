using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace fnSBRentProcess
{
    public class ProcessoLocacao
    {
        private readonly ILogger<ProcessoLocacao> _logger;
        private readonly IConfiguration _configuration;

        public ProcessoLocacao(ILogger<ProcessoLocacao> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Function(nameof(ProcessoLocacao))]
        public async Task Run(
            [ServiceBusTrigger("fila-locacao-auto", Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);

            var body = message.Body.ToString();
            _logger.LogInformation("Message Body: {body}", body);

            try
            {
                var rentModel = JsonSerializer.Deserialize<RentModel>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (rentModel is null || rentModel.veiculo is null)
                {
                    _logger.LogError("Mensagem inválida ou veículo não informado");
                    await messageActions.DeadLetterMessageAsync(message, null, "Mensagem inválida");
                    return;
                }

                var connectionString =
                    _configuration.GetConnectionString("SqlConnectionString")
                    ?? _configuration["SqlConnectionString"]
                    ?? Environment.GetEnvironmentVariable("SqlConnectionString")
                    ?? Environment.GetEnvironmentVariable("SQLCONNSTR_SqlConnectionString");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("SqlConnectionString não configurada.");
                }

                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand(
                    @"INSERT INTO Locacao (Nome, Email, Modelo, Ano, TempoAluguel, Data)
                      VALUES (@Nome, @Email, @Modelo, @Ano, @TempoAluguel, @Data)",
                    connection);

                command.Parameters.AddWithValue("@Nome", rentModel.nome ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", rentModel.email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Modelo", rentModel.veiculo.modelo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Ano", rentModel.veiculo.ano);
                command.Parameters.AddWithValue("@TempoAluguel", rentModel.veiculo.tempoAluguel ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Data", rentModel.data);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                _logger.LogInformation("Locação salva com sucesso. Linhas afetadas: {rowsAffected}", rowsAffected);

                var serviceBusConnection = _configuration["ServiceBusConnection"];
                var serviceBusQueue = _configuration["serviceBusQueue"];

                if (string.IsNullOrWhiteSpace(serviceBusConnection))
                {
                    throw new InvalidOperationException("ServiceBusConnection não configurada.");
                }

                if (string.IsNullOrWhiteSpace(serviceBusQueue))
                {
                    throw new InvalidOperationException("serviceBusQueue não configurada.");
                }

                await SendMessageToPay(serviceBusConnection, serviceBusQueue, rentModel);

                await messageActions.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem: {messageId}", message.MessageId);

                await messageActions.DeadLetterMessageAsync(
                    message,
                    null,
                    $"Erro ao processar a mensagem: {ex.Message}");
            }
        }

        private async Task SendMessageToPay(string serviceBusConnection, string serviceBusQueue, RentModel rentModel)
        {
            await using var serviceBusClient = new ServiceBusClient(serviceBusConnection);
            var serviceBusSender = serviceBusClient.CreateSender(serviceBusQueue);

            var paymentPayload = new
            {
                id = Guid.NewGuid().ToString(),
                IdPaymanet = Guid.NewGuid(),
                nome = rentModel.nome,
                email = rentModel.email,
                modelo = rentModel.veiculo?.modelo,
                ano = rentModel.veiculo?.ano ?? 0,
                tempoAluguel = rentModel.veiculo?.tempoAluguel,
                data = rentModel.data
            };

            var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(paymentPayload))
            {
                ContentType = "application/json"
            };

            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}