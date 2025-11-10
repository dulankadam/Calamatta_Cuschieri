using Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class InMemoryAgentRepository
    {
        private readonly List<Agent> _agents;
        public InMemoryAgentRepository(List<Team> teams)
        {
            _agents = teams.SelectMany(t => t.Agents).ToList();
        }

        public IEnumerable<Agent> GetAll() => _agents;
        public Agent? GetById(System.Guid id) => _agents.FirstOrDefault(a => a.Id == id);
    
    }
}
