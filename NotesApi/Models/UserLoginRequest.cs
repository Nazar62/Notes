using System.ComponentModel.DataAnnotations;

namespace NotesApi.Models
{
    public class UserLoginRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
