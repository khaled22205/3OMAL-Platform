using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;

var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "API"));

var configuration = new ConfigurationBuilder()
    .SetBasePath(baseDir)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
    .Build();

var services = new ServiceCollection();

services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

services.AddIdentity<IdentityUser<int>, IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

services.AddLogging();

var provider = services.BuildServiceProvider();

var seedArgs = ParseArgs(args);

Console.WriteLine($"Seeder — Seed: {seedArgs.Seed}, Workers: {seedArgs.Workers}, Customers: {seedArgs.Customers}, Bookings: {seedArgs.Bookings}");

try
{
    var context = provider.GetRequiredService<AppDbContext>();
    var userManager = provider.GetRequiredService<UserManager<IdentityUser<int>>>();
    var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<int>>>();

    await context.Database.MigrateAsync();

    var generator = new DataGenerator(seedArgs);
    await generator.GenerateAllAsync(context, userManager, roleManager);

    Console.WriteLine("Seeding completed successfully.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Seeding failed: {ex.Message}");
    var ie = ex.InnerException;
    while (ie != null)
    {
        Console.Error.WriteLine($"  -> {ie.GetType().Name}: {ie.Message}");
        ie = ie.InnerException;
    }
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}

static SeedArgs ParseArgs(string[] cmdArgs)
{
    var result = new SeedArgs();
    for (int i = 0; i < cmdArgs.Length; i++)
    {
        switch (cmdArgs[i])
        {
            case "--seed": if (++i < cmdArgs.Length) result.Seed = int.Parse(cmdArgs[i]); break;
            case "--workers": if (++i < cmdArgs.Length) result.Workers = int.Parse(cmdArgs[i]); break;
            case "--customers": if (++i < cmdArgs.Length) result.Customers = int.Parse(cmdArgs[i]); break;
            case "--bookings": if (++i < cmdArgs.Length) result.Bookings = int.Parse(cmdArgs[i]); break;
            case "--reviews": if (++i < cmdArgs.Length) result.Reviews = int.Parse(cmdArgs[i]); break;
            case "--payments": if (++i < cmdArgs.Length) result.Payments = int.Parse(cmdArgs[i]); break;
            case "--invoices": if (++i < cmdArgs.Length) result.Invoices = int.Parse(cmdArgs[i]); break;
            case "--conversations": if (++i < cmdArgs.Length) result.Conversations = int.Parse(cmdArgs[i]); break;
            case "--messages": if (++i < cmdArgs.Length) result.Messages = int.Parse(cmdArgs[i]); break;
            case "--attachments": if (++i < cmdArgs.Length) result.Attachments = int.Parse(cmdArgs[i]); break;
            case "--ci": result.Ci = true; break;
        }
    }
    return result;
}

public class SeedArgs
{
    public int Seed { get; set; } = Random.Shared.Next();
    public int Workers { get; set; } = 50;
    public int Customers { get; set; } = 200;
    public int Bookings { get; set; } = 500;
    public int Reviews { get; set; } = 300;
    public int Payments { get; set; } = 400;
    public int Invoices { get; set; } = 400;
    public int Conversations { get; set; } = 200;
    public int Messages { get; set; } = 10000;
    public int Attachments { get; set; } = 500;
    public bool Ci { get; set; }
}
