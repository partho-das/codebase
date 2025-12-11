using System.Collections.Concurrent;
using AIBackend.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AIBackend.Stores
{
    public class SessionStore<T> : ISessionStore<T>
    {
        private readonly ConcurrentDictionary<string, T> _store = new ConcurrentDictionary<string, T>();

        public bool CreateSession(string id, T request)
        {
            return _store.TryAdd(id, request);
        }

        public T? GetSession(string id)
        {
            _store.TryGetValue(id, out var session);
            return session;
        }
        public bool DeleteSession(string id)
        {
            return _store.TryRemove(id, out _);
        }

    }
}
