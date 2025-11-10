using NUnit.Framework;
using System.Linq;
using Application.Services;
using System.Collections.Generic;
using Domain.Models;
using Domain.Enums;
using FluentAssertions;

namespace ChatQueue.Tests
{
    [TestFixture]
    public class DemoDataTests
    {
        [Test]
        public void DemoData_TotalCapacity_IncludesOverflowAndMatchesExpected()
        {
            var teams = DemoData.CreateSampleTeams(); 
            int total = DemoData.CalculateTotalCapacity(teams, 10);
            // Expected calculation:
            // Team A: TL(5) + 2xMid(6+6) + Jnr(4) = 21
            // Team B: Snr(8) + Mid(6) + 2xJnr(4+4) = 22
            // Team C: 2xMid(6+6) = 12
            // Overflow: 6xJnr(4*6) = 24
            // Total = 21 + 22 + 12 + 24 = 79
            total.Should().Be(79);
        }

        [Test]
        public void DemoData_QueueLimit_IsFloorOf1Point5TimesCapacity()
        {
            var teams = DemoData.CreateSampleTeams();
            int total = DemoData.CalculateTotalCapacity(teams, 10);
            int limit = DemoData.CalculateQueueLimit(total);
            limit.Should().Be((int)System.Math.Floor(79 * 1.5));
            limit.Should().Be(118);
        }
    }
}

