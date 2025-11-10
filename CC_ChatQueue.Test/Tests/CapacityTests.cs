using NUnit.Framework;
using Domain.Enums;
using Domain.Models;
using Application.Services;
using FluentAssertions;

namespace ChatQueue.Tests
{
    [TestFixture]
    public class CapacityTests
    {
        [Test]
        public void SeniorityMultiplier_ReturnsExpectedValues()
        {
            CapacityCalculator.SeniorityMultiplier(Seniority.Junior).Should().Be(0.4);
            CapacityCalculator.SeniorityMultiplier(Seniority.MidLevel).Should().Be(0.6);
            CapacityCalculator.SeniorityMultiplier(Seniority.Senior).Should().Be(0.8);
            CapacityCalculator.SeniorityMultiplier(Seniority.TeamLead).Should().Be(0.5);
        }

        [Test]
        public void AgentCapacity_CalculatesCorrectly_ForJunior()
        {
            var agent = new Agent { Seniority = Seniority.Junior };
            CapacityCalculator.AgentCapacity(agent, 10).Should().Be(4);
        }

        [Test]
        public void AgentCapacity_CalculatesCorrectly_ForMidLevel()
        {
            var agent = new Agent { Seniority = Seniority.MidLevel };
            CapacityCalculator.AgentCapacity(agent, 10).Should().Be(6);
        }

        [Test]
        public void AgentCapacity_CalculatesCorrectly_ForSeniorAndTeamLead()
        {
            var senior = new Agent { Seniority = Seniority.Senior };
            var lead = new Agent { Seniority = Seniority.TeamLead };
            CapacityCalculator.AgentCapacity(senior, 10).Should().Be(8);
            CapacityCalculator.AgentCapacity(lead, 10).Should().Be(5);
        }

        [Test]
        public void CombinedTeamCapacity_ExampleTwoMidsAndOneJunior_Is16()
        {
            var team = new[]
            {
                new Agent { Seniority = Seniority.MidLevel },
                new Agent { Seniority = Seniority.MidLevel },
                new Agent { Seniority = Seniority.Junior }
            };

            int total = 0;
            foreach (var a in team) total += CapacityCalculator.AgentCapacity(a, 10);

            total.Should().Be(16);
        }
    }
}
