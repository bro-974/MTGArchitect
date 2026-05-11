using System.Collections.Generic;

namespace MTGArchitect.ChatMessage.Contracts
{
    public class ChatSessionWithMessagesDto
    {
        public ChatSessionDto Session { get; set; } = new ChatSessionDto();
        public IReadOnlyList<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
    }
}
