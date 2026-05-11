namespace MTGArchitect.AI.Contract;

public class ChatHistoryTurn
{
    public ChatHistoryTurn(string userPrompt, string answer)
    {
        UserPrompt = userPrompt;
        Answer = answer;
    }

    public string UserPrompt { get; }
    public string Answer { get; }
}
