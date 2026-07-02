namespace Application.Features.AiAssistant;

public class KnowledgeContext
{
    public string? Language { get; set; }
    public int? UserId { get; set; }
    public List<string> Roles { get; set; } = [];
}

public interface IKnowledgeService
{
    Task<List<SearchResult>> RetrieveAsync(string query, KnowledgeContext context, int topK = 5);
    Task InitializeAsync();
}
