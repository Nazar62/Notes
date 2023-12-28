using System.ComponentModel.DataAnnotations;

namespace NotesApi.Models.Request
{
    public class ChangeLoginRequest
    {
        [Required]
        public string OldName { get; set; }
        [Required]
        public string NewName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
