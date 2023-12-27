using NotesApi.Models;

namespace NotesApi.Interfaces
{
    public interface IUserRepository
    {
        bool UserExists(int id);
        bool UserExists(string name);
        User GetUser(string name);
        User GetUser(int id);
        User GetUserByEmail(string email);
        string HashPassword(string pass);
        bool CreateUser(User user);
        bool UpdateUser(User user);
        bool DeleteUser(User user);
    }
}
