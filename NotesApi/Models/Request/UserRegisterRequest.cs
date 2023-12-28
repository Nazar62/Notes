using System.ComponentModel.DataAnnotations;

namespace NotesApi.Models.Request
{
    public class UserRegisterRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
