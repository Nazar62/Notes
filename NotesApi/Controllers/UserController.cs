using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Interfaces;
using NotesApi.Models;
using NotesApi.Repository;

namespace NotesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        [HttpPost]
        [Route("Create")]
        public IActionResult CreateUser([FromBody]UserDto user)
        {
            if(user == null)
                return BadRequest();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (_userRepository.UserExists(user.Name))
            {
                ModelState.AddModelError("error", "User exists");
                return BadRequest(ModelState);
            }

            var users = new User()
            {
                Id = 0,
                Name = user.Name,
                Password = _userRepository.HashPassword(user.Password)
            };
            if(!_userRepository.CreateUser(users))
            {
                ModelState.AddModelError("error","Something went wrong creating");
                return BadRequest(ModelState);
            }
            var returnedUser = _userRepository.GetUser(users.Name);
            return Ok(returnedUser);
        }
        [HttpPut("{UserId}")]
        public IActionResult UpdateUser(int UserId, [FromBody]User user)
        {
            if (user.Id != UserId)
                return BadRequest();
            if (!_userRepository.UserExists(UserId))
                return BadRequest();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_userRepository.UpdateUser(user))
            {
                ModelState.AddModelError("error", "Something went wrong updating");
                return BadRequest(ModelState);
            }
            return Ok();

        }
        [HttpPost]
        [Route("login")]
        public IActionResult LoginUser([FromBody]UserDto user)
        {
            if (user == null)
                return BadRequest();
            if (!_userRepository.UserExists(user.Name))
            {
                ModelState.AddModelError("error", "User not exists");
                return BadRequest(ModelState);
            }
            var hash = _userRepository.HashPassword(user.Password);
            var users = _userRepository.GetUser(user.Name);
            if (hash != users.Password)
            {
                ModelState.AddModelError("error", "Password incorrect");
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(users);

        }
    }
}
