namespace Application.Features.AiAssistant;

public class SearchResult
{
    public string SourceType { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public double RelevanceScore { get; set; }
}

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text);
    double ComputeSimilarity(float[] vectorA, float[] vectorB);
}
