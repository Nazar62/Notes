namespace NotesApi.Interfaces
{
    public interface IMailRepository
    {
        void SendMail(string body, string reciever, string subject);
    }
}
