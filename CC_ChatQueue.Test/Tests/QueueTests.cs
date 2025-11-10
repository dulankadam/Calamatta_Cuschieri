using NUnit.Framework;
using Infrastructure.Services;
using Domain.Models;
using System;
using System.Linq;
using FluentAssertions;
using System.Threading.Tasks;

namespace ChatQueue.Tests
{
    [TestFixture]
    public class QueueTests
    {
        [Test]
        public async Task Enqueue_Dequeue_WorksAndMaintainsOrder()
        {
            var q = new InMemoryQueueService(10);
            var c1 = new ChatSession();
            var c2 = new ChatSession();

            await q.EnqueueAsync(c1);
            await q.EnqueueAsync(c2);

            q.Count.Should().Be(2);

            var ok1 = await q.TryDequeueAsync(out var d1);
            ok1.Should().BeTrue();
            d1!.Id.Should().Be(c1.Id);

            var ok2 = await q.TryDequeueAsync(out var d2);
            ok2.Should().BeTrue();
            d2!.Id.Should().Be(c2.Id);

            q.Count.Should().Be(0);
        }

        [Test]
        public async Task Enqueue_WhenQueueFull_MarksRefused()
        {
            var q = new InMemoryQueueService(1);
            var c1 = new ChatSession();
            var c2 = new ChatSession();

            await q.EnqueueAsync(c1);
            var r = await q.EnqueueAsync(c2);
            r.Refused.Should().BeTrue();
            q.Count.Should().Be(1);
        }

        [Test]
        public void MarkPolled_UpdatesLastPolledAtUtc()
        {
            var q = new InMemoryQueueService(5);
            var chat = new ChatSession();
            q.EnqueueAsync(chat).Wait();

            var before = chat.LastPolledAtUtc;
            System.Threading.Thread.Sleep(10);
            q.MarkPolled(chat.Id);
            chat.LastPolledAtUtc.Should().BeGreaterThan(before);
        }

        [Test]
        public async Task Get_ReturnsStoredChat()
        {
            var q = new InMemoryQueueService(5);
            var chat = new ChatSession();
            await q.EnqueueAsync(chat);

            var loaded = q.Get(chat.Id);
            loaded.Should().NotBeNull();
            loaded!.Id.Should().Be(chat.Id);
        }

        [Test]
        public async Task TryDequeue_FromEmpty_ReturnsFalse()
        {
            var q = new InMemoryQueueService(5);
            var ok = await q.TryDequeueAsync(out var chat);
            ok.Should().BeFalse();
            chat.Should().BeNull();
        }
    }
}
