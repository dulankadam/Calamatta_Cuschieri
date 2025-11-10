using Application.Interfaces;
using Domain.Models;
using System;
using System.Collections.Concurrent;

namespace Infrastructure.Services
{
    public class InMemoryQueueService : IQueueService
    {
        private readonly ConcurrentQueue<ChatSession> _queue = new();
        private readonly ConcurrentDictionary<Guid, ChatSession> _store = new();

        public int MaxLength { get; }

        public InMemoryQueueService(int maxLength)
        {
            MaxLength = maxLength;
        }

        public int Count => _queue.Count;

        public Task<ChatSession> EnqueueAsync(ChatSession chat)
        {
            if (_queue.Count >= MaxLength)
            {
                chat.Refused = true;
                _store[chat.Id] = chat;
                return Task.FromResult(chat);
            }

            _queue.Enqueue(chat);
            _store[chat.Id] = chat;
            return Task.FromResult(chat);
        }

        public Task<bool> TryDequeueAsync(out ChatSession? chat)
        {
            var result = _queue.TryDequeue(out chat!);
            return Task.FromResult(result);
        }

        public void MarkPolled(Guid chatId)
        {
            if (_store.TryGetValue(chatId, out var chat))
            {
                chat.LastPolledAtUtc = DateTime.UtcNow;
            }
        }

        public ChatSession? Get(Guid id)
        {
            _store.TryGetValue(id, out var chat);
            return chat;
        }
    }
}