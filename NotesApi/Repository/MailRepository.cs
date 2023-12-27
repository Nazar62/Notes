using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using NotesApi.Interfaces;

namespace NotesApi.Repository
{
    public class MailRepository : IMailRepository
    {
        public void SendMail(string body, string receiver, string subject)
        {
            IConfiguration configuration;
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("624rsr@gmail.com"));
            email.To.Add(MailboxAddress.Parse(receiver));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };
            //mksh splm yvqe pmqg
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate("624rsr@gmail.com", "mksh splm yvqe pmqg");
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
