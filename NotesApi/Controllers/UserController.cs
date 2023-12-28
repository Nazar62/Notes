using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Text;
using NotesApi.Data;
using NotesApi.Interfaces;
using NotesApi.Models;
using NotesApi.Models.Request;
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
                return BadRequest(Json("User not exists"));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (_userRepository.HashPassword(request.Password) != user.Password)
                return BadRequest(Json("Incorrect password"));
            var notes = _contex.Notes.Where(n => n.UserId == UserId).ToList();
            _contex.Notes.RemoveRange(notes);
            if (!_userRepository.DeleteUser(user))
            {
                ModelState.AddModelError("value", "Something went wrong updating");
                return BadRequest(ModelState);
            }
            return Ok(Json("Deleted"));
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
            var user = _userRepository.GetUserByVerifyToken(token);
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
        private void Verify(string name)//https://localhost:7055/api/User/verify/ef6e012e-706b-411b-a8be-30506c7b82bc)
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
            user.ResetToken = Guid.NewGuid().ToString();
            var baseUrl = $"https://{Request.Host.Value}/api/User/reset-password-verify/{user.ResetToken}";
            _mailRepository.SendMail($"<h1>Reset password Notes</h1> <div>{baseUrl}</div>", user.Email, "Confirm reset password");
            if (!_userRepository.UpdateUser(user))
            {
                ModelState.AddModelError("value", "Something went wrong updating");
                return BadRequest(ModelState);
            }
            return Ok(Json(user.ResetToken));
        }
        [HttpPost]
        [Route("reset-password")]
        public IActionResult ResetPassword([FromBody]ResetPasswordRequest request)
        {
            var user = _userRepository.GetUserByResetToken(request.token);
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
            if (user.ResetConfirmed == false)
                return BadRequest(Json("Reset not confirmed check email"));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hash = _userRepository.HashPassword(request.NewPassword);
            user.Password = hash;
            user.ResetToken = null;
            user.ResetTokenExpires = null;
            user.ResetConfirmed = false;

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
            var user = _userRepository.GetUserByResetToken(token);
            var res = "Reset password confirmed you can close this page";
            user.ResetConfirmed = true;
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

        [HttpPost]
        [Route("change-userName/")]
        public IActionResult ChangeLogin([FromBody]ChangeLoginRequest request)
        {
            if (request == null)
                return BadRequest();
            if (!_userRepository.UserExists(request.OldName))
            {
                ModelState.AddModelError("value", "User not exists");
                return BadRequest(ModelState);
            }
            if (_userRepository.UserExists(request.NewName))
            {
                ModelState.AddModelError("value", "Login alredy used");
                return BadRequest(ModelState);
            }
            var user = _userRepository.GetUser(request.OldName);
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
            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
            if (!_userRepository.UpdateUser(user))
                return StatusCode(500, Json("Something went wrong updating"));

            var baseUrl = $"https://{Request.Host.Value}/api/User/change-username-verify/{request.NewName}/{user.ResetToken}";
            _mailRepository.SendMail($"<h1>Reset login Notes</h1> <div>{baseUrl}</div>", user.Email, "Confirm reset login");

            return Ok(Json("Check email"));
        }

        [HttpGet("change-username-verify/{newName}/{token}")]
        public ContentResult ResetUserNameVerify(string newName, string token)
        {
            var user = _userRepository.GetUserByResetToken(token);
            if (user == null)
            {
                var htmls = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Verify</title>\r\n    <link href=\"https://fonts.googleapis.com/css?family=Montserrat:100,200,300,regular,500,600,700,800,900,100italic,200italic,300italic,italic,500italic,600italic,700italic,800italic,900italic\" rel=\"stylesheet\" />\r\n</head>\r\n<body style=\"display: flex; align-items: center; justify-content: center; background-color: #424242;\">\r\n    <p style=\"color: white; font-family: Montserrat;\">Token alredy used or not created</p>\r\n</body>\r\n</html>";
                return ReturnCR(htmls);
            }
            var res = $"Your login changed to {newName}";

            if (user.ResetTokenExpires < DateTime.Now)
            {
                user.ResetToken = null;
                user.ResetTokenExpires = null;
                _userRepository.UpdateUser(user);
                var htmls = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Verify</title>\r\n    <link href=\"https://fonts.googleapis.com/css?family=Montserrat:100,200,300,regular,500,600,700,800,900,100italic,200italic,300italic,italic,500italic,600italic,700italic,800italic,900italic\" rel=\"stylesheet\" />\r\n</head>\r\n<body style=\"display: flex; align-items: center; justify-content: center; background-color: #424242;\">\r\n    <p style=\"color: white; font-family: Montserrat;\">Token expired</p>\r\n</body>\r\n</html>";
                return ReturnCR(htmls);
            }

            user.Name = newName;
            user.ResetToken = null;
            user.ResetTokenExpires = null;
            if (!_userRepository.UpdateUser(user))
            {
                res = "Something went wrong updating";
            }
            var html = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Verify</title>\r\n    <link href=\"https://fonts.googleapis.com/css?family=Montserrat:100,200,300,regular,500,600,700,800,900,100italic,200italic,300italic,italic,500italic,600italic,700italic,800italic,900italic\" rel=\"stylesheet\" />\r\n</head>\r\n<body style=\"display: flex; align-items: center; justify-content: center; background-color: #424242;\">\r\n    <p style=\"color: white; font-family: Montserrat;\">{res}</p>\r\n</body>\r\n</html>";
            return ReturnCR(html);
        }

        [HttpPost]
        [Route("change-email/")]
        public IActionResult ChangeEmail([FromBody]ChangeEmailRequest request)
        {
            var user = _userRepository.GetUser(request.id);
            if(user == null) 
                return BadRequest(Json("Token not used"));
            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
            var res = "";
            if (_contex.Users.Any(u => u.Email == request.NewEmail))
                return BadRequest(Json("Email is alredy used"));

            if(!_userRepository.UpdateUser(user))
            {
                return StatusCode(500, Json("Something went wrong updating"));
            }
            var baseUrl = $"https://{Request.Host.Value}/api/User/verify-old/{user.VerificationToken}/{request.NewEmail}";
            _mailRepository.SendMail($"<h1>Change Email Notes</h1> <div>{baseUrl}</div>", user.Email, "Confirm change Email");
            return Ok(Json("Check email"));
        }

        [HttpGet("verify-old/{token}/{newEmail}")]
        public ContentResult VerifyChangeEmailFromOldEmail(string token, string newEmail)
        {
            var user = _userRepository.GetUserByVerifyToken(token);
            var res = "Verified! Check new email!";
            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
            user.ResetConfirmed = true;
            if (!_userRepository.UpdateUser(user))
            {
                res = "Something went wrong updating";
            }
            var html = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Verify Email Changing</title>\r\n    <link href=\"https://fonts.googleapis.com/css?family=Montserrat:100,200,300,regular,500,600,700,800,900,100italic,200italic,300italic,italic,500italic,600italic,700italic,800italic,900italic\" rel=\"stylesheet\" />\r\n</head>\r\n<body style=\"display: flex; align-items: center; justify-content: center; background-color: #424242;\">\r\n    <p style=\"color: white; font-family: Montserrat;\">{res}</p>\r\n</body>\r\n</html>";
            var baseUrl = $"https://{Request.Host.Value}/api/User/verify-new/{user.ResetToken}/{newEmail}";
            _mailRepository.SendMail($"<h1>Change Email Notes</h1> <div>{baseUrl}</div>", newEmail, "Confirm change email");
            return ReturnCR(html);
        }

        [HttpGet("verify-new/{token}/{email}")]
        public ContentResult VerifyChangeEmailFromNewEmail(string token, string email)
        {
            var res = "Verified! Email changed!";
            var user = _userRepository.GetUserByResetToken(token);
            if (user == null)
            {
                res = "Url expired";
                var htmls = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Verify Email Changing</title>\r\n    <link href=\"https://fonts.googleapis.com/css?family=Montserrat:100,200,300,regular,500,600,700,800,900,100italic,200italic,300italic,italic,500italic,600italic,700italic,800italic,900italic\" rel=\"stylesheet\" />\r\n</head>\r\n<body style=\"display: flex; align-items: center; justify-content: center; background-color: #424242;\">\r\n    <p style=\"color: white; font-family: Montserrat;\">{res}</p>\r\n</body>\r\n</html>";
                return ReturnCR(htmls);
            }

            try
            {
                user.ResetToken = null;
                user.ResetTokenExpires = null;
                user.ResetConfirmed = false;
                user.Email = email;
            } catch
            {

            }
            if (!_userRepository.UpdateUser(user))
            {
                res = "Something went wrong updating";
            }
            var html = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Verify Email Changing</title>\r\n    <link href=\"https://fonts.googleapis.com/css?family=Montserrat:100,200,300,regular,500,600,700,800,900,100italic,200italic,300italic,italic,500italic,600italic,700italic,800italic,900italic\" rel=\"stylesheet\" />\r\n</head>\r\n<body style=\"display: flex; align-items: center; justify-content: center; background-color: #424242;\">\r\n    <p style=\"color: white; font-family: Montserrat;\">{res}</p>\r\n</body>\r\n</html>";
            return ReturnCR(html);
        }

        private ContentResult ReturnCR(string html)
        {
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = html
            };
        }
        
        //    //mksh splm yvqe pmqg
    }
}
