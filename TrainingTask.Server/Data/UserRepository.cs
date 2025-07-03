using MongoDB.Driver;
using TrainingTask.Server.Models;
using System.Threading.Tasks;
using BCrypt.Net;

namespace TrainingTask.Server.Data
{
    public class UserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly MongoDbContext _context;

        public UserRepository(MongoDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetUserAsync(string username, string password)
        {
            try
            {
                var user = await _context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
                if (user == null)
                    return null;

                if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                    return null;

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserAsync for username: {Username}", username);
                return null;
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserByUsernameAsync for username: {Username}", username);
                return null;
            }
        }

        public async Task CreateUserAsync(User user)
        {
            try
            {
                await _context.Users.InsertOneAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateUserAsync for user: {Username}", user?.Username);
                throw;
            }
        }
    }
}
