using System;

namespace MTGArchitect.ChatMessage.Contracts
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public string UserPrompt { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
