using System;
using System.Text.Json;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using GeneratorTriggerFunc.Models;

namespace GeneratorTriggerFunc
{
    public class GeneratorTrigger
    {
        private const string GENERATE_URL = "https://password-generator-ctf8a4fpandwb9f5.polandcentral-01.azurewebsites.net/api/password/generate";

        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        // Constructor that injects the HttpClient and Logger
        public GeneratorTrigger(ILoggerFactory loggerFactory, HttpClient httpClient)
        {
            _logger = loggerFactory.CreateLogger<GeneratorTrigger>();
            _httpClient = httpClient;
        }

        [Function("GeneratorTrigger")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
            var passwordRequest = GetRequestModel();
            var jsonRequest = JsonSerializer.Serialize(passwordRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(GENERATE_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"POST request successful: {await response.Content.ReadAsStringAsync()}");
                }
                else
                {
                    _logger.LogError($"POST request failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while sending POST request: {ex.Message}");
            }
        }

        private GeneratePasswordRequest GetRequestModel()
        {
            return new GeneratePasswordRequest
            {
                Length = 5,
                IncludeSpecial = true,
                IncludeNumbers = true,
                IncludeUppercase = true,
                IncludeLowercase = true,
                UseSimpleGenerator = true
            };
        }
    }
}
