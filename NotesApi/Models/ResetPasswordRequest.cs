using System.ComponentModel.DataAnnotations;

namespace NotesApi.Models
{
    public class ResetPasswordRequest
    {
        [Required]
        public string token { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
