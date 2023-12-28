using System.ComponentModel.DataAnnotations;

namespace NotesApi.Models.Request
{
    public class ResetPasswordRequest
    {
        [Required]
        public string token { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
