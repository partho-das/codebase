using AIBackend.Interfaces;
using AIBackend.Models;

namespace AIBackend.Stores
{
    public class AiSessionStore : SessionStore<AiRequest>, IAiSessionStore
    {
    }
}
