using NotesApi.Models;

namespace NotesApi.Interfaces
{
    public interface INotesRepository
    {
        ICollection<Note> GetNotes(int userId);
        Note GetNote(int id);
        bool NoteExists(int id);
        bool CreateNote(Note note);
        bool UpdateNote(Note note);
        bool DeleteNote(Note note);
        bool Save();
    }
}
