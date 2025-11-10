using Domain.Models;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IQueueService
    {
       Task<ChatSession> EnqueueAsync(ChatSession chat);
        Task<bool> TryDequeueAsync(out ChatSession? chat);
        int Count { get; }
        int MaxLength { get; }
        void MarkPolled(Guid chatId);
        ChatSession? Get(Guid id);
    }
}
