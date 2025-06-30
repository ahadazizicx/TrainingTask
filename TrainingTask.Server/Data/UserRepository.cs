using MongoDB.Driver;
using TrainingTask.Server.Models;
using System.Threading.Tasks;
using BCrypt.Net;

namespace TrainingTask.Server.Data
{
    public class UserRepository
    {
        private readonly MongoDbContext _context;
        public UserRepository(MongoDbContext context)
        {
            _context = context;
        }

        // public async Task<User> GetUserAsync(string username, string password)
        // {
        //     return await _context.Users.Find(u => u.Username == username && u.Password == password).FirstOrDefaultAsync();
        // }

        public async Task<User?> GetUserAsync(string username, string password)
        {
            var user = await _context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
            if (user == null)
                return null;

            // Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                return null;

            return user;
        }
    }
}
