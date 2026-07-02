using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.AiAssistant;
using Infrastructure.Data;

namespace Infrastructure.Services;

public class KnowledgeService : IKnowledgeService
{
    private readonly AppDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<KnowledgeService> _logger;

    private List<KnowledgeItem> _knowledgeIndex = [];

    public KnowledgeService(AppDbContext context, IEmbeddingService embeddingService, ILogger<KnowledgeService> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        var items = new List<KnowledgeItem>();

        // Categories
        var categories = await _context.Categories
            .Where(c => c.IsActive && !c.IsDeleted)
            .ToListAsync();

        foreach (var cat in categories)
        {
            var text = $"{cat.Name} {cat.Description}";
            items.Add(new KnowledgeItem
            {
                SourceType = "category",
                SourceId = cat.Id,
                Title = cat.Name,
                Excerpt = cat.Description ?? cat.Name,
                Text = text,
                Embedding = await _embeddingService.GenerateEmbeddingAsync(text),
                RequiredRoles = [] // Public
            });
        }

        // Workers (public information)
        var workers = await _context.WorkerProfiles
            .Where(w => !w.IsDeleted && w.IsAvailable)
            .ToListAsync();

        var workerServices = await _context.WorkerServices
            .Where(s => s.IsActive && !s.IsDeleted)
            .Include(s => s.Category)
            .ToListAsync();

        var workerServiceLookup = workerServices
            .GroupBy(s => s.WorkerProfileId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var worker in workers)
        {
            var userName = await _context.Set<Microsoft.AspNetCore.Identity.IdentityUser<int>>()
                .Where(u => u.Id == worker.UserId)
                .Select(u => u.UserName ?? "")
                .FirstOrDefaultAsync();

            var workerSvcs = workerServiceLookup.GetValueOrDefault(worker.Id, []);
            var serviceNames = string.Join(", ", workerSvcs.Select(s => s.Title));
            var text = $"{userName} {worker.Biography} {worker.Skills} {serviceNames} {worker.ServiceAreas}";

            items.Add(new KnowledgeItem
            {
                SourceType = "worker",
                SourceId = worker.Id,
                Title = $"Worker: {userName}",
                Excerpt = $"{userName} - Rating: {worker.AverageRating}, Jobs: {worker.CompletedJobs}, Services: {serviceNames}",
                Text = text,
                Embedding = await _embeddingService.GenerateEmbeddingAsync(text),
                RequiredRoles = [] // Public - customers and guests can see workers
            });
        }

        // Services (public)
        var services = await _context.WorkerServices
            .Where(s => s.IsActive && !s.IsDeleted)
            .Include(s => s.Category)
            .Take(200)
            .ToListAsync();

        foreach (var service in services)
        {
            var text = $"{service.Title} {service.Description} {service.Tags} {service.Price}";
            items.Add(new KnowledgeItem
            {
                SourceType = "service",
                SourceId = service.Id,
                Title = service.Title,
                Excerpt = $"{service.Title} - {service.Price} EGP - {service.Category?.Name}",
                Text = text,
                Embedding = await _embeddingService.GenerateEmbeddingAsync(text),
                RequiredRoles = [] // Public
            });
        }

        _knowledgeIndex = items;
        _logger.LogInformation("Knowledge index initialized with {Count} items (embeddings computed)", items.Count);
    }

    public async Task<List<SearchResult>> RetrieveAsync(string query, KnowledgeContext context, int topK = 5)
    {
        if (_knowledgeIndex.Count == 0)
        {
            await InitializeAsync();
        }

        var queryVector = await _embeddingService.GenerateEmbeddingAsync(query);
        var userRoles = context.Roles ?? [];

        var scored = _knowledgeIndex
            .Where(item =>
                // If item has no role restriction it's public; else user must have one of required roles
                item.RequiredRoles.Count == 0 ||
                item.RequiredRoles.Any(r => userRoles.Contains(r)))
            .Select(item => new
            {
                Item = item,
                Score = _embeddingService.ComputeSimilarity(queryVector, item.Embedding)
            })
            .Where(x => x.Score > 0.05) // lowered threshold for better recall
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => new SearchResult
            {
                SourceType = x.Item.SourceType,
                SourceId = x.Item.SourceId,
                Title = x.Item.Title,
                Excerpt = x.Item.Excerpt,
                RelevanceScore = x.Score
            })
            .ToList();

        return scored;
    }

    private class KnowledgeItem
    {
        public string SourceType { get; set; } = string.Empty;
        public int SourceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = [];
        /// <summary>Empty means public (no restriction). Non-empty means only these roles can see it.</summary>
        public List<string> RequiredRoles { get; set; } = [];
    }
}
