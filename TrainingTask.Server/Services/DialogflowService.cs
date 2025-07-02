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

        public async Task<(string fulfillmentText, string intentName, string resultBranch)> DetectIntentAsync(ChatRequest request, string credentialsJson, string languageCode)
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
                throw new InvalidOperationException("Bot configuration is not set or wrong.");
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

            if (string.IsNullOrEmpty(fulfillmentText))
            {
                _logger.LogInformation("Fulfillment text is empty, checking payload for fulfillment messages.");
                fulfillmentText = response.QueryResult.FulfillmentMessages[0]?.Payload?.ToString();
                _logger.LogInformation("Custom Payload: {FulfillmentText}", fulfillmentText);
            }
            var resultBranch = GetResultBranch(intentName);

            return (fulfillmentText, intentName, resultBranch);
        }

        private string GetResultBranch(string intentName)
        {
            return intentName switch
            {
                "Default Welcome Intent" => ResultBranchType.PromptAndCollectNextResponse.ToString(),
                "Default Fallback Intent" => ResultBranchType.PromptAndCollectNextResponse.ToString(),
                "StandardBotExchange" => ResultBranchType.PromptAndCollectNextResponse.ToString(),
                "StandardBotMultipleMessages" => ResultBranchType.PromptAndCollectNextResponse.ToString(),
                "StandardBotDfoMessage" => ResultBranchType.PromptAndCollectNextResponse.ToString(),
                "StandardBotUserInputTimeout" => ResultBranchType.PromptAndCollectNextResponse.ToString(),
                "StandardBotScriptPayload" => ResultBranchType.PromptAndCollectNextResponse.ToString(),
                "StandardBotExchangeCustomInput" => ResultBranchType.PromptAndCollectNextResponse.ToString(),
                "StandardBotEscalation" => ResultBranchType.ReturnControlToScript.ToString(),
                "StandardBotEndConversation" => ResultBranchType.ReturnControlToScript.ToString(),
                _ => "No Result Branch",
            };
        }
    }
}
