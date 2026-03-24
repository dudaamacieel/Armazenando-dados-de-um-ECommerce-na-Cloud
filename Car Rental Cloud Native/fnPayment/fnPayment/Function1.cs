using Azure.Messaging.ServiceBus;
using fnPayment.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace fnPayment;

public class Paymanet
{
    private readonly ILogger<Paymanet> _logger;
    private readonly IConfiguration _configuration;
    private readonly string[] StatusList = { "Aprovado", "Reprovado", "Em análise" };
    private readonly Random random = new Random();

    public Paymanet(ILogger<Paymanet> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [Function(nameof(Paymanet))]
    [CosmosDBOutput("%CosmosDb%", "%CosmosContainer%", Connection = "CosmosDBConnection", CreateIfNotExists = true)]
    public async Task<object?> Run(
        [ServiceBusTrigger("payment-queue", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        try
        {
            var paymanet = JsonSerializer.Deserialize<PaymanetModel>(
                message.Body.ToString(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (paymanet == null)
            {
                await messageActions.DeadLetterMessageAsync(message, null, "Mensagem mal formatada");
                return null;
            }

            string status = StatusList[random.Next(StatusList.Length)];
            paymanet.status = status;

            if (status == "Aprovado")
            {
                paymanet.dataAprovacao = DateTime.UtcNow;
                await SendToNotificationQueue(paymanet);
            }

            await messageActions.CompleteMessageAsync(message);
            return paymanet;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento. Message ID: {id}", message.MessageId);
            await messageActions.DeadLetterMessageAsync(message, null, "Erro ao processar pagamento.");
            return null;
        }
    }

    private async Task SendToNotificationQueue(PaymanetModel paymanet)
    {
        var connection = _configuration["ServiceBusConnection"];
        var queue = _configuration["NotificationQueue"];

        if (string.IsNullOrWhiteSpace(connection))
        {
            throw new InvalidOperationException("ServiceBusConnection não configurada.");
        }

        if (string.IsNullOrWhiteSpace(queue))
        {
            throw new InvalidOperationException("NotificationQueue não configurada.");
        }

        await using var serviceBusClient = new ServiceBusClient(connection);
        var sender = serviceBusClient.CreateSender(queue);

        var message = new ServiceBusMessage(JsonSerializer.Serialize(paymanet))
        {
            ContentType = "application/json"
        };

        message.ApplicationProperties["type"] = "notification";
        message.ApplicationProperties["message"] = "Pagamento aprovado com sucesso";
        message.ApplicationProperties["timestamp"] = DateTime.UtcNow.ToString("o");

        await sender.SendMessageAsync(message);
    }
}