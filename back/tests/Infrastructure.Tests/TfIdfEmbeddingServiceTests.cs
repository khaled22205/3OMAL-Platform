using FluentAssertions;
using Infrastructure.Services;

namespace Infrastructure.Tests;

public class TfIdfEmbeddingServiceTests
{
    private readonly TfIdfEmbeddingService _service = new();

    [Fact]
    public async Task GenerateEmbeddingAsync_Should_return_256_dim_vector()
    {
        var result = await _service.GenerateEmbeddingAsync("hello world");
        result.Should().HaveCount(256);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_Should_return_unit_vector()
    {
        var result = await _service.GenerateEmbeddingAsync("plumbing service repair water pipe leak");
        var norm = Math.Sqrt(result.Sum(v => v * v));
        norm.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_Should_handle_empty_string()
    {
        var result = await _service.GenerateEmbeddingAsync("");
        result.Should().HaveCount(256);
        result.All(v => v == 0).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_Should_handle_whitespace()
    {
        var result = await _service.GenerateEmbeddingAsync("   ");
        result.Should().HaveCount(256);
        result.All(v => v == 0).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_Should_filter_stop_words()
    {
        var result = await _service.GenerateEmbeddingAsync("the and a is in on at");
        result.All(v => v == 0).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_Should_filter_single_char_tokens()
    {
        var result = await _service.GenerateEmbeddingAsync("a b c d e");
        result.All(v => v == 0).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_Should_handle_arabic_text()
    {
        var result = await _service.GenerateEmbeddingAsync("سباكة تصليح مياه");
        result.Should().HaveCount(256);
        var norm = Math.Sqrt(result.Sum(v => v * v));
        norm.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_Should_produce_non_zero_vector_for_repeated_words()
    {
        var result = await _service.GenerateEmbeddingAsync("pipe pipe pipe pipe pipe");
        var nonZeroCount = result.Count(v => v != 0);
        nonZeroCount.Should().Be(1);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_Should_tokenize_on_non_alphanumeric()
    {
        var result = await _service.GenerateEmbeddingAsync("hello-world!test_value");
        var norm = Math.Sqrt(result.Sum(v => v * v));
        norm.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void ComputeSimilarity_Should_return_1_for_identical_vectors()
    {
        var vec = new float[] { 1, 0, 0, 0 };
        var similarity = _service.ComputeSimilarity(vec, vec);
        similarity.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void ComputeSimilarity_Should_return_0_for_orthogonal_vectors()
    {
        var a = new float[] { 1, 0, 0, 0 };
        var b = new float[] { 0, 1, 0, 0 };
        var similarity = _service.ComputeSimilarity(a, b);
        similarity.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void ComputeSimilarity_Should_return_0_for_different_lengths()
    {
        var a = new float[] { 1, 0, 0 };
        var b = new float[] { 1, 0, 0, 0 };
        var similarity = _service.ComputeSimilarity(a, b);
        similarity.Should().Be(0);
    }

    [Fact]
    public void ComputeSimilarity_Should_return_0_when_magnitude_is_0()
    {
        var a = new float[] { 0, 0, 0, 0 };
        var b = new float[] { 1, 0, 0, 0 };
        var similarity = _service.ComputeSimilarity(a, b);
        similarity.Should().Be(0);
    }

    [Fact]
    public void ComputeSimilarity_Should_be_symmetric()
    {
        var a = new float[] { 1, 2, 3, 4 };
        var b = new float[] { 4, 3, 2, 1 };
        var simAB = _service.ComputeSimilarity(a, b);
        var simBA = _service.ComputeSimilarity(b, a);
        simAB.Should().BeApproximately(simBA, 0.001);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_Should_be_case_insensitive()
    {
        var lower = await _service.GenerateEmbeddingAsync("Plumbing Service");
        var upper = await _service.GenerateEmbeddingAsync("PLUMBING SERVICE");
        lower.Should().BeEquivalentTo(upper);
    }
}
