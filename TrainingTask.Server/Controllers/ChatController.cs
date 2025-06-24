using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingTask.Server.Models;
using Google.Cloud.Dialogflow.V2;
using System.Text.Json;

namespace TrainingTask.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        // Use static config for demo; replace with per-user config in production
        private static BotConfiguration _config = new BotConfiguration
        {
            LanguageCode = "en",
            CredentialsJson = ""
        };

        public ChatController(ILogger<ChatController> logger)
        {
            _logger = logger;
        }

        public static BotConfiguration GetStaticConfig() => _config;

        [HttpPost]
        public async Task<IActionResult> ReceiveMessage([FromBody] ChatRequest request)
        {
            _logger.LogInformation(request.Message);

            var credentialsJson = _config.CredentialsJson;
            var languageCode = _config.LanguageCode;
            var projectId = "";
            if (!string.IsNullOrEmpty(credentialsJson))
            {
                using var doc = JsonDocument.Parse(credentialsJson);
                if (doc.RootElement.TryGetProperty("project_id", out var pid))
                    projectId = pid.GetString();
            }

            if (string.IsNullOrEmpty(credentialsJson) || string.IsNullOrEmpty(projectId))
            {
                return BadRequest("Bot configuration is not set.");
            }

            var builder = new SessionsClientBuilder
            {
                JsonCredentials = credentialsJson
            };
            var sessionsClient = await builder.BuildAsync();

            var sessionName = SessionName.FromProjectSession(projectId, request.SessionId);
            var queryInput = new QueryInput
            {
                Text = new TextInput
                {
                    Text = request.Message,
                    LanguageCode = languageCode
                }
            };

            var detectIntentRequest = new DetectIntentRequest
            {
                SessionAsSessionName = sessionName,
                QueryInput = queryInput
            };

            var response = await sessionsClient.DetectIntentAsync(detectIntentRequest);
            _logger.LogInformation("Dialogflow Response: {Response}", response);

            var fulfillmentText = response.QueryResult.FulfillmentText;
            var intentName = response.QueryResult.Intent.DisplayName;

            return Ok(new { fulfillmentText, intentName });
        }
    }
}
