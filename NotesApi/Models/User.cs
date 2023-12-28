namespace NotesApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? VerificationToken { get; set; }
        public DateTime? VerificatedAt { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public bool? ResetConfirmed { get; set; }
    }
}
