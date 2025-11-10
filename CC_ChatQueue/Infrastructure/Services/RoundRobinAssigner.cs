using Application.Interfaces;
using Domain.Models;
using Domain.Enums;
using Infrastructure.Repositories;
using Application.Services;

namespace Infrastructure.Services
{
    public class RoundRobinAssigner : IAssignmentService
    {
        private readonly IQueueService _queueService;
        private readonly InMemoryAgentRepository _agentRepository;
        private readonly int _maxConcurrency;
        private readonly object _lock = new();

        public RoundRobinAssigner(IQueueService queueService, InMemoryAgentRepository agentRepository, int maxConcurrency)
        {
            _queueService = queueService;
            _agentRepository = agentRepository;
            _maxConcurrency = maxConcurrency;
        }

        public Task AssignNextAsync()
        {
            lock (_lock)
            {
                while (_queueService.TryDequeueAsync(out var chat).Result && chat is not null && !chat.Refused)
                {
                    var availableAgents = _agentRepository.GetAll()
                        .Where(a => !a.IsShiftEnding)
                        .OrderBy(a => a.CurrentLoad)
                        .ToList();

                    var seniorityOrder = new[]
                    {
                        Seniority.Junior,
                        Seniority.MidLevel,
                        Seniority.Senior,
                        Seniority.TeamLead
                    };

                    Domain.Models.Agent? chosenAgent = null;

                    foreach (var seniority in seniorityOrder)
                    {
                        var candidates = availableAgents
                            .Where(a => a.Seniority == seniority)
                            .OrderBy(a => a.CurrentLoad)
                            .ToList();

                        foreach (var agent in candidates)
                        {
                            int capacity = CapacityCalculator.AgentCapacity(agent, _maxConcurrency);
                            if (agent.CurrentLoad < capacity)
                            {
                                chosenAgent = agent;
                                break;
                            }
                        }

                        if (chosenAgent != null)
                            break;
                    }

                    if (chosenAgent == null)
                    {
                        chat.Refused = true;
                        continue;
                    }

                    chat.IsAssigned = true;
                    chat.AssignedAgentId = chosenAgent.Id;
                    chosenAgent.CurrentLoad++;
                }
            }

            return Task.CompletedTask;
        }
    }
}
