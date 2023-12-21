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

        [HttpGet("GetUserNotes/{UserId}")]
        public IActionResult GetNotes(int UserId)
        {
            if (!_userRepository.UserExists(UserId))
                return NotFound();

            var notes = _notesRepository.GetNotes(UserId);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(notes);
        }

        [HttpPost]
        [Route("CreateNote")]
        public IActionResult CreateNote([FromBody]Note note)
        {
            if (note == null)
                return BadRequest();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if(!_notesRepository.CreateNote(note))
            {
                ModelState.AddModelError("", "Something went wrong creating");
                return StatusCode(500, ModelState);
            }
            return StatusCode(201);
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
                ModelState.AddModelError("", "Something went wrong updating");
                return StatusCode(500, ModelState);
            }
            return Ok("Updated");
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
                ModelState.AddModelError("", "Something went wrong deleting");
                return StatusCode(500, ModelState);
            }
            return Ok("Deleted");
        }
    }
}
