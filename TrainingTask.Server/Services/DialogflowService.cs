using Google.Cloud.Dialogflow.V2;
using System.Text.Json;
using TrainingTask.Server.Models;

namespace TrainingTask.Server.Services
{
    public class DialogflowService : IDialogflowService
    {
        private readonly ILogger<DialogflowService> _logger;

        public DialogflowService(ILogger<DialogflowService> logger)
        {
            _logger = logger;
        }

        public async Task<(string fulfillmentText, string intentName)> DetectIntentAsync(ChatRequest request, string credentialsJson, string languageCode)
        {
            var projectId = "";
            if (!string.IsNullOrEmpty(credentialsJson))
            {
                using var doc = JsonDocument.Parse(credentialsJson);
                if (doc.RootElement.TryGetProperty("project_id", out var pid))
                    projectId = pid.GetString();
            }
            if (string.IsNullOrEmpty(credentialsJson) || string.IsNullOrEmpty(projectId))
            {
                throw new InvalidOperationException("Bot configuration is not set.");
            }
            var builder = new SessionsClientBuilder { JsonCredentials = credentialsJson };
            var sessionsClient = await builder.BuildAsync();
            var sessionName = SessionName.FromProjectSession(projectId, request.SessionId);
            var queryInput = new QueryInput
            {
                Text = new TextInput { Text = request.Message, LanguageCode = languageCode }
            };
            var detectIntentRequest = new DetectIntentRequest
            {
                SessionAsSessionName = sessionName,
                QueryInput = queryInput
            };
            var response = await sessionsClient.DetectIntentAsync(detectIntentRequest);

            _logger.LogInformation("Dialogflow response: {Response}", response);
            var fulfillmentText = response.QueryResult.FulfillmentText;
            var intentName = response.QueryResult.Intent.DisplayName;
            return (fulfillmentText, intentName);
        }
    }
}
