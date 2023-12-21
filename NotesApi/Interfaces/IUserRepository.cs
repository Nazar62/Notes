using NotesApi.Models;

namespace NotesApi.Interfaces
{
    public interface IUserRepository
    {
        bool UserExists(int id);
        bool UserExists(string name);
        User GetUser(string name);
        User GetUser(int id);
        string HashPassword(string pass);
        bool CreateUser(User user);
        bool UpdateUser(User user);
    }
}
