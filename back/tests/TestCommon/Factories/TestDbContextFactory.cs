using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TestCommon.Factories;

public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    public static AppDbContext CreateWithData(Action<AppDbContext> seed)
    {
        var context = Create();
        seed(context);
        context.SaveChanges();
        return context;
    }
}
