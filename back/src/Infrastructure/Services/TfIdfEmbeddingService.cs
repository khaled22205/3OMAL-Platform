using Application.Features.AiAssistant;

namespace Infrastructure.Services;

public class TfIdfEmbeddingService : IEmbeddingService
{
    public Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var tokens = Tokenize(text);
        var vector = new float[256];
        var tf = new Dictionary<string, int>();

        foreach (var token in tokens)
        {
            tf.TryGetValue(token, out var count);
            tf[token] = count + 1;
        }

        var maxFreq = tf.Values.Any() ? tf.Values.Max() : 1;

        int i = 0;
        foreach (var (token, count) in tf.OrderByDescending(x => x.Value))
        {
            if (i >= vector.Length) break;
            vector[i] = (float)count / maxFreq;
            i++;
        }

        var norm = (float)Math.Sqrt(vector.Sum(v => v * v));
        if (norm > 0)
        {
            for (int j = 0; j < vector.Length; j++)
                vector[j] /= norm;
        }

        return Task.FromResult(vector);
    }

    public double ComputeSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length) return 0;

        double dot = 0, normA = 0, normB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dot += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }

        var magnitude = Math.Sqrt(normA) * Math.Sqrt(normB);
        return magnitude == 0 ? 0 : dot / magnitude;
    }

    private static List<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];

        var tokens = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (var c in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c) || c is >= '\u0600' and <= '\u06FF' or >= '\u0750' and <= '\u077F')
            {
                current.Append(c);
            }
            else
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
            }
        }

        if (current.Length > 0)
            tokens.Add(current.ToString());

        var stopWords = new HashSet<string> { "the", "a", "an", "is", "are", "was", "were", "be", "been",
            "in", "on", "at", "to", "for", "of", "with", "and", "or", "not", "it", "its", "this", "that",
            "what", "how", "do", "does", "did", "can", "will", "would", "could", "should", "may", "might",
            "i", "me", "my", "we", "our", "you", "your", "he", "she", "they", "them", "their", "have",
            "has", "had", "about", "than", "then", "also", "just", "very", "all", "each", "some", "any",
            "no", "so", "if", "but", "because", "as", "up", "out", "off", "over", "after", "before",
            "between", "through", "during", "from", "by" };

        return tokens.Where(t => t.Length > 1 && !stopWords.Contains(t)).Distinct().ToList();
    }
}
