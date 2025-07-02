using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TrainingTask.Server.Models;

namespace TrainingTask.Server.Services
{
    public class ChatWebSocketHandler
    {
        private readonly IDialogflowService _dialogflowService;
        private readonly ILogger<ChatWebSocketHandler> _logger;
        private readonly IConfiguration _configuration;

        public ChatWebSocketHandler(IDialogflowService dialogflowService, ILogger<ChatWebSocketHandler> logger, IConfiguration configuration)
        {
            _dialogflowService = dialogflowService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task HandleAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            // Extract JWT from Authorization header (Bearer) or cookie
            string token = null;
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer "))
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }
            }
            if (string.IsNullOrEmpty(token) && context.Request.Cookies.ContainsKey("jwt"))
            {
                token = context.Request.Cookies["jwt"];
            }

            // Validate JWT
            var config = Controllers.ChatController.GetStaticConfig();
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Missing JWT token");
                    return;
                }
                tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid JWT token for WebSocket connection");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid or expired JWT token");
                return;
            }

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var buffer = new byte[1024 * 10];
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
                // Console.WriteLine($"Received JSON Credentials: {chatRequest?.JsonCreds}");
                Console.WriteLine($"Session ID: {chatRequest?.SessionId}");
                var credentialsJson = !string.IsNullOrEmpty(chatRequest?.JsonCreds) ? chatRequest.JsonCreds : config.JsonCreds;
                var languageCode = config.LanguageCode;
                string fulfillmentText = "", intentName = "", resultBranch = "";
                if (!string.IsNullOrEmpty(chatRequest?.Message) && !string.IsNullOrEmpty(chatRequest?.SessionId))
                {
                    try
                    {
                        (fulfillmentText, intentName, resultBranch) = await _dialogflowService.DetectIntentAsync(chatRequest, credentialsJson, languageCode);
                    }
                    catch (Exception ex)
                    {
                        fulfillmentText = $"Error: {ex.Message}";
                        intentName = "Error";
                        resultBranch = "-";
                    }
                }
                var reply = JsonSerializer.Serialize(new { fulfillmentText, intentName, resultBranch });

                _logger.LogInformation("Sending reply: {Reply}", reply);
                var replyBytes = Encoding.UTF8.GetBytes(reply);
                await webSocket.SendAsync(new ArraySegment<byte>(replyBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
