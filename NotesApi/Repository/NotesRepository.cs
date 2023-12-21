using NotesApi.Data;
using NotesApi.Interfaces;
using NotesApi.Models;

namespace NotesApi.Repository
{
    public class NotesRepository : INotesRepository
    {
        private readonly DataContext _context;

        public NotesRepository(DataContext context)
        {
            _context = context;
        }
        public ICollection<Note> GetNotes(int userId)
        {
            return _context.Notes.Where(n => n.UserId == userId).ToList();
        }
        public Note GetNote(int id)
        {
            return _context.Notes.Where(n => n.Id == id).FirstOrDefault();
        }
        public bool Save()
        {
            try
            {
                var saved = _context.SaveChanges();
                return saved > 0 ? true : false;
            }
            catch
            {
                return false;
            }
        }

        public bool NoteExists(int id)
        {
                return _context.Notes.Any(n => n.Id == id);
        }

        public bool CreateNote(Note note)
        {
            _context.Notes.Add(note);
            return Save();
        }
        public bool UpdateNote(Note note)
        {
            _context.Notes.Update(note);
            return Save();
        }
        public bool DeleteNote(Note note)
        {
            _context.Notes.Remove(note);
            return Save();
        }
    }
}
