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

        public async Task<IntentDTO> DetectIntentAsync(ChatRequest request, string credentialsJson, string languageCode)
        {
            try
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
                var fulfillmentText = "";
                var intentName = response.QueryResult.Intent.DisplayName;
                if (intentName == "StandardBotMultipleMessages")
                {
                    var messages = response.QueryResult.FulfillmentMessages;
                    var texts = new List<string>();
                    foreach (var message in messages)
                    {
                        if (message.Text?.Text_ != null && message.Text.Text_.Count > 0)
                        {
                            texts.Add(message.Text.Text_[0]);
                        }
                    }
                    fulfillmentText = string.Join(", ", texts);
                }
                else
                {
                    fulfillmentText = response.QueryResult.FulfillmentText;
                }

                if (string.IsNullOrEmpty(fulfillmentText))
                {
                    _logger.LogInformation("Fulfillment text is empty, checking payload for fulfillment messages.");
                    fulfillmentText = response.QueryResult.FulfillmentMessages[0]?.Payload?.ToString();
                    _logger.LogInformation("Custom Payload: {FulfillmentText}", fulfillmentText);
                }
                var resultBranch = GetResultBranch(intentName);

                return new IntentDTO
                {
                    fulfillmentText = fulfillmentText,
                    intentName = intentName,
                    resultBranch = resultBranch
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse Dialogflow credentials JSON.");
                throw new InvalidOperationException("Invalid Dialogflow credentials format.", ex);
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.LogError(ex, "Dialogflow API error: {Message}", ex.Message);
                throw new InvalidOperationException("Dialogflow API error.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DetectIntentAsync: {Message}", ex.Message);
                throw;
            }
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
                // "StandardBotEscalation" => ResultBranchType.ReturnControlToScript.ToString(),
                "StandardBotEndConversation" => ResultBranchType.ReturnControlToScript.ToString(),
                _ when intentName.Contains("escalat", StringComparison.OrdinalIgnoreCase) => ResultBranchType.ReturnControlToScript.ToString(),
                _ => "No Result Branch",
            };
        }
    }
}
