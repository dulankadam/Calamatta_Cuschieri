using Domain.Models;
using Domain.Enums;
using System.Collections.Generic;
using Application.Services;

namespace Application
{
    public static class MockData
    {
        public static List<Team> CreateMockTeams()
        {
            return new List<Team>
        {
            new Team
            {
                Name = "Team A",
                Agents = new List<Agent>
                {
                    new Agent { Name = "TL-A", Seniority = Seniority.TeamLead },
                    new Agent { Name = "Mid1-A", Seniority = Seniority.MidLevel },
                    new Agent { Name = "Mid2-A", Seniority = Seniority.MidLevel },
                    new Agent { Name = "Jnr-A", Seniority = Seniority.Junior },
                }
            },
            new Team
            {
                Name = "Team B",
                Agents = new List<Agent>
                {
                    new Agent { Name = "Snr-B", Seniority = Seniority.Senior },
                    new Agent { Name = "Mid1-B", Seniority = Seniority.MidLevel },
                    new Agent { Name = "Jnr1-B", Seniority = Seniority.Junior },
                    new Agent { Name = "Jnr2-B", Seniority = Seniority.Junior },
                }
            },
            new Team
            {
                Name = "Team C (Night Shift)",
                Agents = new List<Agent>
                {
                    new Agent { Name = "Mid1-C", Seniority = Seniority.MidLevel },
                    new Agent { Name = "Mid2-C", Seniority = Seniority.MidLevel },
                }
            },
            // Overflow team: 6 Juniors
            new Team
            {
                Name = "Overflow Team",
                Agents = Enumerable.Range(1, 6)
                    .Select(i => new Agent { Name = $"Overflow-Jnr-{i}", Seniority = Seniority.Junior })
                    .ToList()
            }
            };
        }

        public static int CalculateTotalCapacity(List<Team> teams, int maxConcurrency)
        {
            int total = 0;
            foreach (var team in teams)
            {
                foreach (var agent in team.Agents)
                {
                    total += CapacityCalculator.AgentCapacity(agent, maxConcurrency);
                }
            }
            return total;
        }

        public static int CalculateQueueMaxLength(int totalCapacity)
        {
            return (int)System.Math.Floor(totalCapacity * 1.5);
        }
    }
}