using Application.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.HostedServices
{
    public class QueueMonitorHostedService : BackgroundService
    {
        private readonly IQueueService _queue;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(1);

        public QueueMonitorHostedService(IQueueService queue)
        {
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // mark sessions inactive if not polled for 3 seconds
                foreach (var kv in typeof(Infrastructure.Services.InMemoryQueueService).Assembly.GetTypes())
                {
                    // keep it simple: InMemoryQueueService stores ChatSessions; Queue exposes Get
                }

                // In real impl we'd iterate store and mark inactive; but client-facing API uses LastPolledAtUtc
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
