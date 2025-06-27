using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingTask.Server.Models;
using TrainingTask.Server.Services;
using System.Text.Json;

namespace TrainingTask.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IDialogflowService _dialogflowService;
        // Use static config for demo; replace with per-user config in production
        private static BotConfiguration _config = new BotConfiguration
        {
            LanguageCode = "en",
            JsonCreds = ""
        };

        public ChatController(ILogger<ChatController> logger, IDialogflowService dialogflowService)
        {
            _logger = logger;
            _dialogflowService = dialogflowService;
        }

        public static BotConfiguration GetStaticConfig() => _config;

        [HttpPost]
        public async Task<IActionResult> ReceiveMessage([FromBody] ChatRequest request)
        {
            _logger.LogInformation("Received message: {Message}", request.Message);
            _logger.LogInformation("Config JSON Credentials: {JsonCreds}", _config.JsonCreds);

            var credentialsJson = !string.IsNullOrEmpty(request.JsonCreds) ? request.JsonCreds : _config.JsonCreds;
            var languageCode = _config.LanguageCode;
            try
            {
                var (fulfillmentText, intentName) = await _dialogflowService.DetectIntentAsync(request, credentialsJson, languageCode);
                return Ok(new { fulfillmentText, intentName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dialogflow error");
                return BadRequest(ex.Message);
            }
        }
    }
}
