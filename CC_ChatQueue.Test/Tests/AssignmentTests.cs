using NUnit.Framework;
using Infrastructure.Services;
using Infrastructure.Repositories;
using Domain.Models;
using Domain.Enums;
using System.Collections.Generic;
using FluentAssertions;
using System.Threading.Tasks;
using Application.Services;
using System.Linq;

namespace ChatQueue.Tests
{
    [TestFixture]
    public class AssignmentTests
    {
        private (InMemoryQueueService queue, InMemoryAgentRepository repo, RoundRobinAssigner assigner) BuildWithTeams(List<Team> teams, int maxConcurrency = 10)
        {
            int total = DemoData.CalculateTotalCapacity(teams, maxConcurrency);
            int qlen = DemoData.CalculateQueueLimit(total);
            var queue = new InMemoryQueueService(qlen);
            var repo = new InMemoryAgentRepository(teams);
            var assigner = new RoundRobinAssigner(queue, repo, maxConcurrency);
            return (queue, repo, assigner);
        }

        [Test]
        public async Task SingleSeniorAndSingleJunior_FiveChats_Assigns4ToJuniorAnd1ToSenior()
        {
            var teams = new List<Team>
            {
                new Team
                {
                    Name = "TestTeam",
                    Agents = new List<Agent>
                    {
                        new Agent { Name = "Senior", Seniority = Seniority.Senior },
                        new Agent { Name = "Junior", Seniority = Seniority.Junior },
                    }
                }
            };

            var (queue, repo, assigner) = BuildWithTeams(teams);

            // enqueue 5 chats
            var chats = Enumerable.Range(1,5).Select(_ => new ChatSession()).ToList();
            foreach (var c in chats) await queue.EnqueueAsync(c);

            await assigner.AssignNextAsync();

            var agents = repo.GetAll().ToList();
            var senior = agents.Single(a => a.Seniority == Seniority.Senior);
            var junior = agents.Single(a => a.Seniority == Seniority.Junior);

            junior.CurrentLoad.Should().Be(4);
            senior.CurrentLoad.Should().Be(1);
        }

        [Test]
        public async Task TwoJuniorsAndOneMid_SixChats_Distributes3EachToJuniors_NoneToMid()
        {
            var teams = new List<Team>
            {
                new Team
                {
                    Name = "TestTeam2",
                    Agents = new List<Agent>
                    {
                        new Agent { Name = "J1", Seniority = Seniority.Junior },
                        new Agent { Name = "J2", Seniority = Seniority.Junior },
                        new Agent { Name = "M1", Seniority = Seniority.MidLevel },
                    }
                }
            };

            var (queue, repo, assigner) = BuildWithTeams(teams);

            // enqueue 6 chats
            var chats = Enumerable.Range(1,6).Select(_ => new ChatSession()).ToList();
            foreach (var c in chats) await queue.EnqueueAsync(c);

            await assigner.AssignNextAsync();

            var agents = repo.GetAll().ToList();
            var j1 = agents.First(a => a.Name == "J1");
            var j2 = agents.First(a => a.Name == "J2");
            var m1 = agents.First(a => a.Name == "M1");

            (j1.CurrentLoad + j2.CurrentLoad + m1.CurrentLoad).Should().Be(6);
            m1.CurrentLoad.Should().Be(0);
            j1.CurrentLoad.Should().BeGreaterThan(0);
            j2.CurrentLoad.Should().BeGreaterThan(0);
            j1.CurrentLoad.Should().Be(3);
            j2.CurrentLoad.Should().Be(3);
        }

        [Test]
        public async Task Assign_DoesNotAssignToShiftEndingAgents()
        {
            var teams = new List<Team>
            {
                new Team
                {
                    Name = "ShiftTeam",
                    Agents = new List<Agent>
                    {
                        new Agent { Name = "A1", Seniority = Seniority.Junior, IsShiftEnding = true },
                        new Agent { Name = "A2", Seniority = Seniority.Junior, IsShiftEnding = false }
                    }
                }
            };

            var (queue, repo, assigner) = BuildWithTeams(teams);

            for (int i = 0; i < 3; i++) await queue.EnqueueAsync(new ChatSession());

            await assigner.AssignNextAsync();

            var agents = repo.GetAll().ToList();
            var a1 = agents.Single(a => a.Name == "A1");
            var a2 = agents.Single(a => a.Name == "A2");

            a1.CurrentLoad.Should().Be(0, "agent with IsShiftEnding true should not receive new assignment");
            a2.CurrentLoad.Should().Be(3);
        }

        [Test]
        public async Task Assign_WhenNoAgentHasCapacity_MarksChatAsRefused()
        {
            var teams = new List<Team>
            {
                new Team
                {
                    Name = "FullTeam",
                    Agents = new List<Agent>
                    {
                        new Agent { Name = "J1", Seniority = Seniority.Junior, CurrentLoad = 4 }
                    }
                }
            };

            var (queue, repo, assigner) = BuildWithTeams(teams);

            var chat = new ChatSession();
            await queue.EnqueueAsync(chat);

            await assigner.AssignNextAsync();

            var stored = queue.Get(chat.Id);
            stored.Should().NotBeNull();
            stored!.Refused.Should().BeTrue();
        }

        [Test]
        public async Task Assign_MultipleTeams_RespectsJuniorFirstAcrossAllAgents()
        {
            var teams = new List<Team>
            {
                new Team
                {
                    Name = "T1",
                    Agents = new List<Agent> { new Agent { Name="J", Seniority = Seniority.Junior } }
                },
                new Team
                {
                    Name = "T2",
                    Agents = new List<Agent> { new Agent { Name="M", Seniority = Seniority.MidLevel } }
                }
            };

            var (queue, repo, assigner) = BuildWithTeams(teams);

            foreach (var _ in Enumerable.Range(1, 4)) await queue.EnqueueAsync(new ChatSession());

            await assigner.AssignNextAsync();

            var agents = repo.GetAll().ToList();
            var j = agents.Single(a => a.Seniority == Seniority.Junior);
            var m = agents.Single(a => a.Seniority == Seniority.MidLevel);

            j.CurrentLoad.Should().Be(4);
            m.CurrentLoad.Should().Be(0);
        }

        [Test]
        public async Task Assign_RoundRobin_BalancesLoadWithinSeniorityGroup()
        {
            var teams = new List<Team>
            {
                new Team
                {
                    Name = "BalanceTeam",
                    Agents = new List<Agent>
                    {
                        new Agent { Name = "J1", Seniority = Seniority.Junior },
                        new Agent { Name = "J2", Seniority = Seniority.Junior },
                        new Agent { Name = "J3", Seniority = Seniority.Junior },
                    }
                }
            };

            var (queue, repo, assigner) = BuildWithTeams(teams);

            foreach (var _ in Enumerable.Range(1, 7)) await queue.EnqueueAsync(new ChatSession());

            await assigner.AssignNextAsync();

            var agents = repo.GetAll().OrderBy(a => a.Name).ToList();
            var loads = agents.Select(a => a.CurrentLoad).ToList();

            loads.Sum().Should().Be(7);
            loads.Max().Should().BeLessOrEqualTo(4); // each capacity is 4 so none exceed
            loads.Min().Should().BeGreaterThan(0);
        }
    }
}
