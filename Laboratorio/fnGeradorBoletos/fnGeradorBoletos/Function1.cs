using Azure.Messaging.ServiceBus;
using BarcodeStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace fnGeradorBoletos
{
    public class GeradorCodigoBarras
    {
        private readonly ILogger<GeradorCodigoBarras> _logger;
        private readonly string _serviceBusConnectionString;
        private readonly string _queueName = "gerador-codigo-barras";

        public GeradorCodigoBarras(ILogger<GeradorCodigoBarras> logger)
        {
            _logger = logger;
            _serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        }

        [Function("barcode-generate")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                string valor = data?.valor;
                string dataVencimento = data?.dataVencimento;

                string barcodeData;

                // Validação dos dados
                if (string.IsNullOrEmpty(valor) || string.IsNullOrEmpty(dataVencimento))
                {
                    return new BadRequestObjectResult("Os campos valor e dataVencimento são obrigatórios");
                }

                // Validar formato da Data de Vencimento YYYY-MM-DD
                if (!DateTime.TryParseExact(
                        dataVencimento,
                        "yyyy-MM-dd",
                        null,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime dateObj))
                {
                    return new BadRequestObjectResult("Data de vencimento inválida");
                }

                string dateStr = dateObj.ToString("yyyyMMdd");

                // Conversão de valor para centavos e formatação até 8 dígitos
                if (!decimal.TryParse(valor, out decimal valorDecimal))
                {
                    return new BadRequestObjectResult("Valor inválido");
                }

                int valorCentavos = (int)(valorDecimal * 100);
                string valorStr = valorCentavos.ToString("D8");

                string bankCode = "007";
                string baseCode = string.Concat(bankCode, dateStr, valorStr);

                // Preenchimento do barcode para ter 44 caracteres
                barcodeData = baseCode.Length < 44
                    ? baseCode.PadRight(44, '0')
                    : baseCode.Substring(0, 44);

                _logger.LogInformation($"Barcode gerado: {barcodeData}");

                Barcode barcode = new Barcode();
                var skImage = barcode.Encode(BarcodeStandard.Type.Code128, barcodeData);

                using (var encodeData = skImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
                {
                    var imageBytes = encodeData.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);

                    var resultObject = new
                    {
                        barcode = barcodeData,
                        valorOriginal = valorDecimal,
                        dataVencimento = DateTime.Now.AddDays(5),
                        imagemBase64 = base64String
                    };

                    await SendFileFallback(resultObject, _serviceBusConnectionString, _queueName);

                    return new OkObjectResult(resultObject);
                }
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task SendFileFallback(object resultObject, string serviceBusConnectionString, string queueName)
        {
            await using var client = new ServiceBusClient(serviceBusConnectionString);

            ServiceBusSender sender = client.CreateSender(queueName);

            string messageBody = JsonConvert.SerializeObject(resultObject);

            ServiceBusMessage message = new ServiceBusMessage(messageBody);

            await sender.SendMessageAsync(message);

            _logger.LogInformation($"Mensagem enviada para a fila {queueName}");
        }
    }
}