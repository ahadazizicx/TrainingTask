using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingTask.Server.Models;

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

        [HttpGet]
        public IActionResult GetConfig()
        {
            return Ok(_config);
        }

        [HttpPut]
        public IActionResult UpdateConfig([FromBody] BotConfiguration config)
        {
            _config.LanguageCode = config.LanguageCode;
            _config.JsonCreds = config.JsonCreds;
            return Ok(_config);
        }
    }
}
