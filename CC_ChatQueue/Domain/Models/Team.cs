namespace Domain.Models
{
    public class Team:  ModelBase
    {
        public string Name { get; set; } = string.Empty;
        public List<Agent> Agents { get; set; } = new();

    }
}
