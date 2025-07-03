using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingTask.Server.Models;
using TrainingTask.Server.Data;
using System.Security.Claims; // Add this namespace
using MongoDB.Driver;

namespace TrainingTask.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BotConfigController : ControllerBase
    {
        // For demo: in-memory config
        private static BotConfiguration _config = new BotConfiguration
        {
            LanguageCode = "en",
            JsonCreds = ""
        };

        private readonly MongoDbContext _context;
        private readonly ILogger<BotConfigController> _logger;

        public BotConfigController(MongoDbContext context, ILogger<BotConfigController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetConfig()
        {
            try
            {
                // get all bots with user id
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                _logger.LogInformation("Fetching bot configurations for user: {UserId}", userId);
                var bots = _context.BotConfigurations.Find(b => b.UserId == userId).ToList();
                return Ok(new { success = true, data = bots });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bot configurations");
                return StatusCode(500, new { success = false, message = "An error occurred while fetching bot configurations." });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetConfigById([FromRoute] string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("Invalid configuration ID.");
                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var botConfig = _context.BotConfigurations.Find(b => b.Id == id && b.UserId == userId).FirstOrDefault();

                if (botConfig == null)
                {
                    return NotFound("Configuration not found.");
                }
                return Ok(new { success = true, data = botConfig });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bot configuration by id: {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while fetching the configuration." });
            }
        }

        [Authorize]
        [HttpPost("create")]
        public IActionResult CreateConfig([FromBody] BotConfigDTO config)
        {
            try
            {
                if (config == null)
                {
                    return BadRequest("Invalid configuration data.");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var newConfig = new BotConfiguration
                {
                    UserId = userId,
                    BotName = config.BotName,
                    JsonCreds = config.JsonCreds,
                    LanguageCode = config.LanguageCode
                };
                // Here you would typically save the newConfig to your database
                //add these in a mongo collection
                _context.BotConfigurations.InsertOne(newConfig);

                return Ok(new { success = true, data = newConfig });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bot configuration");
                return StatusCode(500, new { success = false, message = "An error occurred while creating the configuration." });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult UpdateConfig([FromRoute] string id, [FromBody] BotConfigDTO config)
        {
            try
            {
                //update the bot config
                if (config == null || string.IsNullOrEmpty(id))
                {
                    return BadRequest("Invalid configuration data.");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingConfig = _context.BotConfigurations.Find(b => b.Id == id && b.UserId == userId).FirstOrDefault();

                if (existingConfig == null)
                {
                    return NotFound("Configuration not found.");
                }

                existingConfig.BotName = config.BotName;
                existingConfig.JsonCreds = config.JsonCreds;
                existingConfig.LanguageCode = config.LanguageCode;

                _context.BotConfigurations.ReplaceOne(b => b.Id == id, existingConfig);

                return Ok(new { success = true, data = existingConfig });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bot configuration: {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while updating the configuration." });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteConfig([FromRoute] string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("Invalid configuration ID.");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = _context.BotConfigurations.DeleteOne(b => b.Id == id && b.UserId == userId);

                if (result.DeletedCount == 0)
                {
                    return NotFound("Configuration not found.");
                }

                return Ok(new { success = true, message = "Configuration deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bot configuration: {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the configuration." });
            }
        }
    }
}
