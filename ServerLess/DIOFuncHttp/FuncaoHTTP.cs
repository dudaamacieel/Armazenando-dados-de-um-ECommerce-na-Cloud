using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DIOFuncHttp
{
    public static class FuncaoHTTP
    {
            // <summary>
            // Processes HTTP request for the FuncaoHTTP Azure Function.
            // </summary>
            // <param name="req">The HTTP request containing either a query parameter or request body with the name to be processed.</param>
            // <param name="log">The logger used for logging information about function execution.</param>
            // <returns>
            // An <see cref="IActionResult"/> containing a response message.
            // If a name is provided, returns a personalized greeting.
            // If no name is provided, returns a generic success message.
            // </returns>


        [FunctionName("FuncaoHTTP")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
