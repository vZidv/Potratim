using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Potratim.Tests
{
    public class TestSession : ISession
    {
        private readonly ConcurrentDictionary<string, byte[]> _data = new();
        
        public bool IsAvailable => true;

        public string Id => Guid.NewGuid().ToString();

        public IEnumerable<string> Keys => _data.Keys;

        public void Clear() => _data.Clear();
        
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _data.TryRemove(key, out _);

        public void Set(string key, byte[] value) => _data[key] = value;

        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) => _data.TryGetValue(key, out value);
    }
}