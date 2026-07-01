using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TestCommon.Factories;

namespace TestCommon.Fixtures;

public class IntegrationTestFixture : IDisposable
{
    public AppDbContext CreateContext()
    {
        return TestDbContextFactory.Create();
    }

    public AppDbContext CreateContextWithData(Action<AppDbContext> seed)
    {
        return TestDbContextFactory.CreateWithData(seed);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
