using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TrainingTask.Server.Models;

namespace TrainingTask.Server.Services
{
    public class ChatWebSocketHandler
    {
        private readonly IDialogflowService _dialogflowService;

        public ChatWebSocketHandler(IDialogflowService dialogflowService)
        {
            _dialogflowService = dialogflowService;
        }

        public async Task HandleAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var buffer = new byte[1024 * 10];
            var config = Controllers.ChatController.GetStaticConfig();
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var chatRequest = JsonSerializer.Deserialize<ChatRequest>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Console.WriteLine($"Received message: {chatRequest?.Message}");
                Console.WriteLine($"Config JSON Credentials: {config.JsonCreds}");
                Console.WriteLine($"Session ID: {chatRequest?.SessionId}");
                var credentialsJson = !string.IsNullOrEmpty(chatRequest?.JsonCreds) ? chatRequest.JsonCreds : config.JsonCreds;
                var languageCode = config.LanguageCode;
                string fulfillmentText = "", intentName = "";
                if (!string.IsNullOrEmpty(chatRequest?.Message) && !string.IsNullOrEmpty(chatRequest?.SessionId))
                {
                    try
                    {
                        (fulfillmentText, intentName) = await _dialogflowService.DetectIntentAsync(chatRequest, credentialsJson, languageCode);
                    }
                    catch (Exception ex)
                    {
                        fulfillmentText = $"Error: {ex.Message}";
                        intentName = "Error";
                    }
                }
                var reply = JsonSerializer.Serialize(new { fulfillmentText, intentName });
                var replyBytes = Encoding.UTF8.GetBytes(reply);
                await webSocket.SendAsync(new ArraySegment<byte>(replyBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
