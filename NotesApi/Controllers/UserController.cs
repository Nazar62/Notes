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
        public IActionResult CreateUser([FromBody]User user)
        {
            if(user == null)
                return BadRequest();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if(!_userRepository.CreateUser(user))
            {
                ModelState.AddModelError("","Something went wrong creating");
                return BadRequest(ModelState);
            }
            return Ok("Created");
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
                ModelState.AddModelError("", "Something went wrong updating");
                return BadRequest(ModelState);
            }
            return Ok("Updated");

        }
        [HttpPost]
        [Route("login")]
        public IActionResult LoginUser([FromBody]UserDto user)
        {
            if (user == null)
                return BadRequest();
            if (!_userRepository.UserExists(user.Name))
                return BadRequest();
            var hash = _userRepository.HashPassword(user.Password);
            var users = _userRepository.GetUser(user.Name);
            if (hash != users.Password)
                return BadRequest("Password incorrect");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(users);

        }
        [HttpGet("{UserId}")]
        public IActionResult GetUser(int UserId)
        {
            if (!_userRepository.UserExists(UserId))
                return NotFound();
            var user = _userRepository.GetUser(UserId);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(user);
        }
    }
}
