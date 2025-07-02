using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TrainingTask.Server.Models;
using TrainingTask.Server.Data;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;

namespace TrainingTask.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        // For demo: hardcoded user
        private readonly UserRepository _userRepository;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, UserRepository userRepository, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginDTO register)
        {
            if (string.IsNullOrEmpty(register.Username) || string.IsNullOrEmpty(register.Password))
                return BadRequest("Username and password are required");

            var existingUser = await _userRepository.GetUserByUsernameAsync(register.Username);
            if (existingUser != null)
                return Conflict("Username already exists");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(register.Password);
            var user = new User
            {
                Username = register.Username,
                Password = hashedPassword
            };

            await _userRepository.CreateUserAsync(user);

            _logger.LogInformation("User {Username} registered successfully", register.Username);
            return Ok(new { success = true, message = "Registration successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(login.Password);
           
            var user = await _userRepository.GetUserAsync(login.Username, login.Password);
            if (user == null)
                return Unauthorized("Invalid username or password");

            // Fix: Use BCrypt.Net.BCrypt.HashPassword instead of BCrypt.HashPassword
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            Response.Cookies.Append("jwt", jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                // SameSite = SameSiteMode.Strict,
                SameSite = SameSiteMode.None, // <-- Change this line
                Expires = DateTime.UtcNow.AddHours(2)
            });

            _logger.LogInformation("Set-Cookie header: {Header}", Response.Headers["Set-Cookie"].ToString());

            return Ok(new
            {
                success = true,
                message = "Login successful",
                //token = jwt
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new
            {
                success = true,
                message = "Logout successful"
            });
        }
    }
}
