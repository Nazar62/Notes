using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using NotesApi.Data;
using NotesApi.Interfaces;
using NotesApi.Models;
using NotesApi.Repository;
using Org.BouncyCastle.Tls;
using System.Net;
using System.Net.Mail;

namespace NotesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly DataContext _contex;
        private readonly IMailRepository _mailRepository;

        public UserController(IUserRepository userRepository, DataContext contex, IMailRepository mailRepository)
        {
            _userRepository = userRepository;
            _contex = contex;
            _mailRepository = mailRepository;
        }
        [HttpPost]
        [Route("Create")]
        public IActionResult CreateUser([FromBody]UserRegisterRequest user)
        {
            if(user == null)
                return BadRequest();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if(_contex.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("value", "Email alredy used");
                return BadRequest(ModelState);
            }
            if (_userRepository.UserExists(user.Name))
            {
                ModelState.AddModelError("value", "Login alredy used");
                return BadRequest(ModelState);
            }

            var users = new User()
            {
                Id = 0,
                Name = user.Name,
                Password = _userRepository.HashPassword(user.Password),
                Email = user.Email,
                VerificationToken = Guid.NewGuid().ToString()
            };
            if(!_userRepository.CreateUser(users))
            {
                ModelState.AddModelError("value","Something went wrong creating");
                return BadRequest(ModelState);
            }
            var returnedUser = _userRepository.GetUser(users.Name);
            Verify(returnedUser.Name);
            return Ok(returnedUser);
        }
        
        [HttpDelete("delete/{UserId}")]
        public IActionResult DeleteUser(int UserId, [FromBody]UserLoginRequest request)
        {
            var user = _userRepository.GetUser(request.Name);
            if(UserId != user.Id)
                return BadRequest(Json("Incorrect id"));
            if (!_userRepository.UserExists(UserId))
                return BadRequest("User not exists");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (_userRepository.HashPassword(request.Password) != user.Password)
                return BadRequest(Json("Incorrect password"));
            if (!_userRepository.DeleteUser(user))
            {
                ModelState.AddModelError("value", "Something went wrong updating");
                return BadRequest(ModelState);
            }
            return Ok();
        }

        [HttpPost]
        [Route("login")]
        public IActionResult LoginUser([FromBody]UserLoginRequest request)
        {
            if (request == null)
                return BadRequest();
            if (!_userRepository.UserExists(request.Name))
            {
                ModelState.AddModelError("value", "User not exists");
                return BadRequest(ModelState);
            }
            var user = _userRepository.GetUser(request.Name);
            var hash = _userRepository.HashPassword(request.Password);
            if (user.VerificatedAt == null)
                return BadRequest(Json("Not verified"));
            if (hash != user.Password)
            {
                ModelState.AddModelError("value", "Password incorrect");
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(user);
        }

        [HttpGet("verify/{token}")]
        //[Route("verify/{token}")]
        public ContentResult VerifyHtml(string token)
        {
            var user = _contex.Users.FirstOrDefault(u => u.VerificationToken == token);
            var res = "Verified you can close this page";
            user.VerificatedAt = DateTime.Now;
            if (!_userRepository.UpdateUser(user))
            {
                res = "Something went wrong updating";
            }
            var html = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Verify</title>\r\n    <link href=\"https://fonts.googleapis.com/css?family=Montserrat:100,200,300,regular,500,600,700,800,900,100italic,200italic,300italic,italic,500italic,600italic,700italic,800italic,900italic\" rel=\"stylesheet\" />\r\n</head>\r\n<body style=\"display: flex; align-items: center; justify-content: center; background-color: #424242;\">\r\n    <p style=\"color: white; font-family: Montserrat;\">{res}</p>\r\n</body>\r\n</html>";
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html
            };
        }
        [HttpGet]
        public void Verify(string name)//https://localhost:7055/api/User/verify/ef6e012e-706b-411b-a8be-30506c7b82bc)
        {
            var user = _userRepository.GetUser(name);
            var baseUrl = $"https://{Request.Host.Value}/api/User/verify/{user.VerificationToken}";
            _mailRepository.SendMail($"<h1>Verify Account Notes</h1> <div>{baseUrl}</div>", user.Email, "Verify Account");
        }

        [HttpPost]
        [Route("forgot-password")]
        public IActionResult ForgotPassword([FromBody]string login)
        {
            var user = _userRepository.GetUser(login);
            if (!_userRepository.UserExists(login))
            {
                ModelState.AddModelError("value", "User not exists");
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            user.PasswordResetToken = Guid.NewGuid().ToString();
            var baseUrl = $"https://{Request.Host.Value}/api/User/reset-password-verify/{user.PasswordResetToken}";
            _mailRepository.SendMail($"<h1>Reset password Notes</h1> <div>{baseUrl}</div>", user.Email, "Confirm reset password");
            if (!_userRepository.UpdateUser(user))
            {
                ModelState.AddModelError("value", "Something went wrong updating");
                return BadRequest(ModelState);
            }
            return Ok(Json(user.PasswordResetToken));
        }
        [HttpPost]
        [Route("reset-password")]
        public IActionResult ResetPassword([FromBody]ResetPasswordRequest request)
        {
            var user = _contex.Users.Where(u => u.PasswordResetToken == request.token).FirstOrDefault();
            if (user == null || user.ResetTokenExpires < DateTime.Now)
                return BadRequest(Json("Invalid token"));
            if (!_userRepository.UserExists(user.Name))
            {
                ModelState.AddModelError("value", "User not exists");
                return BadRequest(ModelState);
            }
            if (user.ResetTokenExpires == null)
                return BadRequest(Json("Confirm reset password! Check emeil!"));
            if (user.ResetTokenExpires < DateTime.Now)
                return BadRequest(Json("Token expired"));
            if (user.ResetPasswordConfirmed == false)
                return BadRequest(Json("Reset not confirmed check email"));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hash = _userRepository.HashPassword(request.NewPassword);
            user.Password = hash;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;
            user.ResetPasswordConfirmed = false;

            if (!_userRepository.UpdateUser(user))
            {
                ModelState.AddModelError("value", "Something went wrong updating");
                return BadRequest(ModelState);
            }
            return Ok(Json("Password successfully reset"));
        }

        [HttpGet("reset-password-verify/{token}")]
        public ContentResult ResetPasswordVerify(string token)
        {
            var user = _contex.Users.FirstOrDefault(u => u.PasswordResetToken == token);
            var res = "Reset password confirmed you can close this page";
            user.ResetPasswordConfirmed = true;
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
            if (!_userRepository.UpdateUser(user))
            {
                res = "Something went wrong updating";
            }
            var html = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Verify</title>\r\n    <link href=\"https://fonts.googleapis.com/css?family=Montserrat:100,200,300,regular,500,600,700,800,900,100italic,200italic,300italic,italic,500italic,600italic,700italic,800italic,900italic\" rel=\"stylesheet\" />\r\n</head>\r\n<body style=\"display: flex; align-items: center; justify-content: center; background-color: #424242;\">\r\n    <p style=\"color: white; font-family: Montserrat;\">{res}</p>\r\n</body>\r\n</html>";
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html
            };
        }



        //[HttpPost]
        //[Route("email")]
        //public IActionResult SendEmail(string body,string receiver, string subject)
        //{
        //    var email = new MimeMessage();
        //    email.From.Add(MailboxAddress.Parse("624rsr@gmail.com"));
        //    email.To.Add(MailboxAddress.Parse(receiver));
        //    email.Subject = subject;
        //    email.Body = new TextPart(TextFormat.Html) { Text = body };
        //    //mksh splm yvqe pmqg
        //    using var smtp = new MailKit.Net.Smtp.SmtpClient();
        //    smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        //    smtp.Authenticate("624rsr@gmail.com", "mksh splm yvqe pmqg");
        //    smtp.Send(email);
        //    smtp.Disconnect(true);

        //    return Ok();
        //}
    }
}
