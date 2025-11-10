using Domain.Enums;
using Domain.Models;

namespace Application.Services
{
    public static class CapacityCalculator
    {
        public static double SeniorityMultiplier(Seniority seniority) => seniority switch
        {
            Seniority.Junior => 0.4,
            Seniority.MidLevel => 0.6,
            Seniority.Senior => 0.8,
            Seniority.TeamLead => 0.5,
            _ => 0.4
        };

        public static int AgentCapacity(Agent agent, int maxConcurrency)
        {
            double multiplier = SeniorityMultiplier(agent.Seniority);
            return (int)System.Math.Floor(maxConcurrency * multiplier);
        }
    }
}
