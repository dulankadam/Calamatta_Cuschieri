using Domain.Enums;

namespace Domain.Models
{
    public class Agent: ModelBase
    {
        public string Name { get; set; } = "";
        public Seniority Seniority { get; set; }
        public int CurrentLoad { get; set; } = 0;
        public bool IsShiftEnding { get; set; } = false;
    }
}
