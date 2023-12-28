using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Data;
using NotesApi.Interfaces;
using NotesApi.Models;

namespace NotesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : Controller
    {
        private readonly INotesRepository _notesRepository;
        private readonly IUserRepository _userRepository;

        public NotesController(INotesRepository notesRepository, IUserRepository userRepository)
        {
            _notesRepository = notesRepository;
            _userRepository = userRepository;
        }

        [HttpGet("GetUserNotes/{UserId}/{Password}")]
        public IActionResult GetNotes(int UserId, string Password)
        {
            if (!_userRepository.UserExists(UserId))
                return NotFound();
            if (_userRepository.GetUser(UserId).Password != _userRepository.HashPassword(Password))
                return BadRequest(Json("Password changed"));

            var notes = _notesRepository.GetNotes(UserId);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(notes);
        }

        [HttpPost]
        [Route("CreateNote")]
        public IActionResult CreateNote([FromBody]Note note)
        {
            var user = _userRepository.GetUser(note.UserId);
            if (note == null)
                return BadRequest();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (user.VerificatedAt == null)
                return BadRequest(Json("Verify account"));

            if(!_notesRepository.CreateNote(note))
            {
                ModelState.AddModelError("error", "Something went wrong creating");
                return StatusCode(500, ModelState);
            }
            return Ok(Json("Created"));
        }

        [HttpPut("updateNote/{NoteId}")]
        public IActionResult UpdateNote(int NoteId, [FromBody]Note note)
        {
            if (NoteId != note.Id)
                return BadRequest();
            if (note == null)
                return BadRequest();
            if (!ModelState.IsValid)
                return BadRequest();
            if (!_notesRepository.UpdateNote(note))
            {
                ModelState.AddModelError("error", "Something went wrong updating");
                return StatusCode(500, ModelState);
            }
            return Ok(Json("\"r\": \"Updated\""));
        }

        [HttpDelete("{NoteId}")]
        public IActionResult DeleteNote(int NoteId,[FromBody]Note note)
        {
            if (NoteId != note.Id)
                return BadRequest();
            if (note == null)
                return BadRequest();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_notesRepository.DeleteNote(note))
            {
                ModelState.AddModelError("error", "Something went wrong deleting");
                return StatusCode(500, ModelState);
            }
            return Ok(Json("deleted"));
        }
    }
}
