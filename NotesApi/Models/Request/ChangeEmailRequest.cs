using System.ComponentModel.DataAnnotations;

namespace NotesApi.Models.Request
{
    public class ChangeEmailRequest
    {
        [Required]
        public int id { get; set; }
        [Required, EmailAddress]
        public string NewEmail { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
