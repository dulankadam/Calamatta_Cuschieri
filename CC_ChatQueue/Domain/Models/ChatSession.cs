namespace Domain.Models
{
    public class ChatSession: ModelBase
    {
         public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastPolledAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsAssigned { get; set; } = false;
        public Guid? AssignedAgentId { get; set; }
        public bool Refused { get; set; } = false;
    }
}
