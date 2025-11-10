using Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.HostedServices
{
    public class AssignmentHostedService : BackgroundService
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ILogger<AssignmentHostedService> _logger;

        public AssignmentHostedService(IAssignmentService assignmentService, ILogger<AssignmentHostedService> logger)
        {
            _assignmentService = assignmentService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _assignmentService.AssignNextAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while assigning chats");
                }

                await Task.Delay(500, stoppingToken);
            }
        }
    }
}
