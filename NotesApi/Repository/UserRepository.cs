using Microsoft.EntityFrameworkCore.Diagnostics;
using NotesApi.Data;
using NotesApi.Interfaces;
using NotesApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace NotesApi.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public bool UserExists(int id)
        {
                return _context.Users.Any(p => p.Id == id);
        }

        public bool UserExists(string name)
        {
            return _context.Users.Any(p => p.Name == name);
        }
        public User GetUser(string name)
        {
            return _context.Users.Where(p => p.Name == name).FirstOrDefault();
        }
        public User GetUser(int id)
        {
            return _context.Users.Where(p => p.Id == id).FirstOrDefault();
        }
        public string HashPassword(string pass)
        {
            var sha = SHA256.Create();
            var passBytes = Encoding.UTF8.GetBytes(pass);
            
            var hashedPass = sha.ComputeHash(passBytes);
            return Convert.ToBase64String(hashedPass);
        }
        public bool Save()
        {
            try
            {
                var saved = _context.SaveChanges();
                return saved > 0 ? true : false;
            }
            catch
            {
                return false;
            }
        }
        public bool CreateUser(User user)
        {
            string hashedPass = HashPassword(user.Password);
            User userh = new User()
            {
                Id = user.Id,
                Name = user.Name,
                Password = hashedPass,
                Email = user.Email,
                VerificationToken = user.VerificationToken
            };
            _context.Users.Add(userh);
            return Save();
        }
        public bool UpdateUser(User user)
        {
            _context.Users.Update(user);
                return Save();
        }

        public bool DeleteUser(User user)
        {
            _context.Users.Remove(user);
            return Save();
        }
        public User GetUserByEmail(string email)
        {
            return _context.Users.Where(u => u.Email == email).FirstOrDefault();
        }

        public User GetUserByResetToken(string token)
        {
            return _context.Users.Where(u => u.ResetToken == token).FirstOrDefault();
        }

        public User GetUserByVerifyToken(string token)
        {
            return _context.Users.Where(u => u.VerificationToken == token).FirstOrDefault();
        }
    }
}
