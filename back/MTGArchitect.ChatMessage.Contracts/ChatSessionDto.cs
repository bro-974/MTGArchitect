using System;

namespace MTGArchitect.ChatMessage.Contracts
{
    public class ChatSessionDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
