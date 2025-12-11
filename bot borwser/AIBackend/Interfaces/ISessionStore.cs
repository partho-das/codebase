using AIBackend.Models;

namespace AIBackend.Interfaces
{
    public interface ISessionStore<T>
    {
        bool CreateSession(string id, T request);
        T? GetSession(string id);
        bool DeleteSession(string id);
    }


    public interface IAiSessionStore : ISessionStore<AiRequest>
    {
    }
}