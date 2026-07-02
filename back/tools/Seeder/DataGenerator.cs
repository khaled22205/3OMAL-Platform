using Bogus;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class DataGenerator
{
    private readonly SeedArgs _args;
    private readonly int _seed;

    private static readonly string[] FirstNames = ["Ahmed", "Mohamed", "Ali", "Hassan", "Hussein", "Omar", "Khaled", "Mahmoud", "Amr", "Tamer", "Youssef", "Ibrahim", "Samy", "Wael", "Hani", "Sherif", "Moustafa", "Karim", "Nabil", "Ramy"];
    private static readonly string[] LastNames = ["Ali", "Hassan", "Mohamed", "Ahmed", "Ibrahim", "Mahmoud", "Said", "Youssef", "Khaled", "Omar", "Fathy", "Gamal", "Nasr", "Rady", "Shaker", "Tawfik", "Zaki", "Waheed", "Fouad", "Emad"];
    private static readonly string[] Cities = ["Cairo", "Alexandria", "Giza", "Sharm El-Sheikh", "Luxor", "Hurghada", "Mansoura", "Tanta", "Port Said", "Suez", "Aswan", "Ismailia", "Damanhur", "Fayoum", "Zagazig"];
    private static readonly string[] StreetNames = ["El Tahrir", "El Nadi", "El Safa", "El Malek", "El Haram", "El Nile", "El Shiekh", "El Faraa", "El Gomhoria", "El Thawra"];
    private static readonly string[] Skills = ["Plumbing", "Electrical", "Carpentry", "Painting", "AC Repair", "Cleaning", "Gardening", "Pest Control", "Moving", "Smart Home", "Automotive", "Welding", "Tiling", "Masonry", "Glass Work"];
    private static readonly string[] BioTemplates = [
        "Experienced {0} with over {1} years in the field. Specializing in residential and commercial {2}.",
        "Professional {0} dedicated to quality work. {1}+ years of experience in {2}.",
        "Certified {0} offering reliable {2} services. {1} years of hands-on experience."
    ];

    public DataGenerator(SeedArgs args)
    {
        _args = args;
        _seed = args.Seed;
    }

    public async Task GenerateAllAsync(
        AppDbContext context,
        UserManager<IdentityUser<int>> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        Randomizer.Seed = new Random(_seed);
        var faker = new Faker();

        Console.Write("Seeding roles... ");
        await SeedRoles(context, roleManager);
        Console.WriteLine("Done");

        Console.Write("Seeding categories... ");
        var categories = await SeedCategories(context);
        Console.WriteLine("Done");

        Console.Write("Seeding users... ");
        var (admins, workers, customers) = await SeedUsers(context, userManager);
        Console.WriteLine($" {admins.Count} admins, {workers.Count} workers, {customers.Count} customers");

        Console.Write("Seeding worker profiles... ");
        var workerProfiles = await SeedWorkerProfiles(context, workers, categories);
        Console.WriteLine($" {workerProfiles.Count} profiles");

        Console.Write("Seeding worker services... ");
        var workerServices = await SeedWorkerServices(context, workerProfiles, categories);
        Console.WriteLine($" {workerServices.Count} services");

        Console.Write("Seeding availability... ");
        await SeedAvailability(context, workerProfiles);
        Console.WriteLine(" Done");

        Console.Write("Seeding portfolio items... ");
        await SeedPortfolio(context, workerProfiles);
        Console.WriteLine(" Done");

        Console.Write("Seeding bookings... ");
        var bookings = await SeedBookings(context, workerProfiles, workerServices, customers);
        Console.WriteLine($" {bookings.Count} bookings");

        Console.Write("Seeding reviews... ");
        await SeedReviews(context, bookings, workerProfiles, customers);
        Console.WriteLine(" Done");

        Console.Write("Seeding payments... ");
        await SeedPayments(context, bookings);
        Console.WriteLine(" Done");

        Console.Write("Seeding invoices... ");
        await SeedInvoices(context, bookings);
        Console.WriteLine(" Done");

        Console.Write("Seeding conversations... ");
        var conversations = await SeedConversations(context, workerProfiles, customers);
        Console.WriteLine($" {conversations.Count} conversations");

        Console.Write("Seeding messages... ");
        await SeedMessages(context, conversations, workerProfiles, customers);
        Console.WriteLine(" Done");

        Console.Write("Seeding favorites... ");
        await SeedFavorites(context, customers, workerProfiles, workerServices);
        Console.WriteLine(" Done");

        Console.Write("Seeding refresh tokens... ");
        await SeedRefreshTokens(context, admins.Concat(workers).Concat(customers).ToList());
        Console.WriteLine(" Done");

        Console.Write("Seeding audit logs... ");
        await SeedAuditLogs(context, admins.Concat(workers).Concat(customers).ToList());
        Console.WriteLine(" Done");
    }

    private async Task SeedRoles(AppDbContext context, RoleManager<IdentityRole<int>> roleManager)
    {
        if (await context.Roles.AnyAsync()) return;

        var roles = new[] { "Admin", "Customer", "Worker" };
        foreach (var role in roles)
            await roleManager.CreateAsync(new IdentityRole<int> { Name = role, NormalizedName = role.ToUpper() });
    }

    private async Task<(List<IdentityUser<int>> Admins, List<IdentityUser<int>> Workers, List<IdentityUser<int>> Customers)> SeedUsers(
        AppDbContext context,
        UserManager<IdentityUser<int>> userManager)
    {
        var admins = new List<IdentityUser<int>>();
        var workers = new List<IdentityUser<int>>();
        var customers = new List<IdentityUser<int>>();

        var existingUsers = await context.Users.ToListAsync();
        if (existingUsers.Count > 0)
        {
            foreach (var u in existingUsers)
            {
                if (await userManager.IsInRoleAsync(u, "Admin")) admins.Add(u);
                else if (await userManager.IsInRoleAsync(u, "Worker")) workers.Add(u);
                else if (await userManager.IsInRoleAsync(u, "Customer")) customers.Add(u);
            }
        }

        if (await userManager.FindByEmailAsync("admin@3omal.com") == null)
        {
            var admin = new IdentityUser<int>
            {
                UserName = "admin",
                Email = "admin@3omal.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");
            admins.Add(admin);
        }

        var workerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Worker");
        var customerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");

        var userFaker = new Faker<IdentityUser<int>>()
            .RuleFor(u => u.UserName, f => f.Internet.UserName().ToLower() + f.Random.Int(100, 9999))
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.EmailConfirmed, true)
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber("010########"))
            .RuleFor(u => u.PhoneNumberConfirmed, true);

        int workersToCreate = _args.Workers - workers.Count;
        for (int i = 0; i < workersToCreate; i++)
        {
            var user = userFaker.Generate();
            var result = await userManager.CreateAsync(user, "Worker@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Worker");
                workers.Add(user);
            }
        }

        int customersToCreate = _args.Customers - customers.Count;
        for (int i = 0; i < customersToCreate; i++)
        {
            var user = userFaker.Generate();
            var result = await userManager.CreateAsync(user, "Customer@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Customer");
                customers.Add(user);
            }
        }

        return (admins, workers, customers);
    }

    private async Task<List<Category>> SeedCategories(AppDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return await context.Categories.ToListAsync();

        var categories = new List<Category>
        {
            new() { Name = "Plumbing", SeoUrl = "plumbing", Description = "Plumbing services", SortOrder = 1, IsActive = true },
            new() { Name = "Electrical", SeoUrl = "electrical", Description = "Electrical services", SortOrder = 2, IsActive = true },
            new() { Name = "Carpentry", SeoUrl = "carpentry", Description = "Carpentry services", SortOrder = 3, IsActive = true },
            new() { Name = "Painting", SeoUrl = "painting", Description = "Painting services", SortOrder = 4, IsActive = true },
            new() { Name = "AC & Refrigeration", SeoUrl = "ac-refrigeration", Description = "AC and refrigeration services", SortOrder = 5, IsActive = true },
            new() { Name = "Cleaning", SeoUrl = "cleaning", Description = "Cleaning services", SortOrder = 6, IsActive = true },
            new() { Name = "Moving Services", SeoUrl = "moving-services", Description = "Moving and relocation services", SortOrder = 7, IsActive = true },
            new() { Name = "Technology & Smart Home", SeoUrl = "technology-smart-home", Description = "Technology and smart home services", SortOrder = 8, IsActive = true },
            new() { Name = "Gardening", SeoUrl = "gardening", Description = "Gardening services", SortOrder = 9, IsActive = true },
            new() { Name = "Pest Control", SeoUrl = "pest-control", Description = "Pest control services", SortOrder = 10, IsActive = true },
            new() { Name = "Automotive", SeoUrl = "automotive", Description = "Mobile car mechanic services", SortOrder = 11, IsActive = true },
            new() { Name = "General Maintenance", SeoUrl = "general-maintenance", Description = "General maintenance services", SortOrder = 12, IsActive = true }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
        return categories;
    }

    private async Task<List<WorkerProfile>> SeedWorkerProfiles(
        AppDbContext context,
        List<IdentityUser<int>> workerUsers,
        List<Category> categories)
    {
        var existingProfiles = await context.WorkerProfiles.ToListAsync();
        var existingUserIds = existingProfiles.Select(p => p.UserId).ToHashSet();
        var profiles = new List<WorkerProfile>(existingProfiles);
        var newProfiles = new List<WorkerProfile>();

        var faker = new Faker();
        var catNames = categories.Select(c => c.Name).ToList();

        var workersWithoutProfiles = workerUsers.Where(u => !existingUserIds.Contains(u.Id)).ToList();
        foreach (var user in workersWithoutProfiles)
        {
            var firstName = faker.PickRandom(FirstNames);
            var lastName = faker.PickRandom(LastNames);
            var mainSkill = faker.PickRandom(Skills);
            var yearsExp = faker.Random.Int(1, 25);
            var bioTemplate = faker.PickRandom(BioTemplates);

            var profile = new WorkerProfile
            {
                UserId = user.Id,
                Biography = string.Format(bioTemplate, mainSkill, yearsExp, mainSkill.ToLower()),
                YearsOfExperience = yearsExp,
                Skills = string.Join(", ", faker.PickRandom(Skills, faker.Random.Int(2, 5))),
                ServiceAreas = string.Join(", ", faker.PickRandom(Cities, faker.Random.Int(1, 4))),
                HourlyRate = faker.Random.Decimal(25, 150),
                StartingPrice = faker.Random.Decimal(50, 500),
                CompletedJobs = faker.Random.Int(0, 500),
                AverageRating = Math.Round(faker.Random.Double(3.0, 5.0), 1),
                IsAvailable = faker.Random.Bool(0.8f),
                IsVerified = faker.Random.Bool(0.7f),
                MinimumJobValue = faker.Random.Decimal(20, 100),
                Photo = $"https://randomuser.me/api/portraits/{faker.PickRandom("men", "women")}/{faker.Random.Int(1, 99)}.jpg",
            };

            newProfiles.Add(profile);
            profiles.Add(profile);
        }

        if (newProfiles.Count > 0)
        {
            context.WorkerProfiles.AddRange(newProfiles);
            await context.SaveChangesAsync();
        }
        return profiles;
    }

    private async Task<List<WorkerService>> SeedWorkerServices(
        AppDbContext context,
        List<WorkerProfile> profiles,
        List<Category> categories)
    {
        var services = await context.WorkerServices.ToListAsync();
        var existingProfileIds = services.Select(s => s.WorkerProfileId).ToHashSet();
        var newProfiles = profiles.Where(p => !existingProfileIds.Contains(p.Id)).ToList();
        if (newProfiles.Count == 0) return services;

        var faker = new Faker();
        var serviceTitles = new Dictionary<string, string[]>
        {
            ["Plumbing"] = ["Pipe Repair", "Faucet Installation", "Water Heater Service", "Drain Cleaning", "Bathroom Renovation"],
            ["Electrical"] = ["Wiring Installation", "Lighting Setup", "Panel Upgrade", "Security System", "Smart Switch Installation"],
            ["Carpentry"] = ["Custom Furniture", "Cabinet Making", "Floor Installation", "Door Repair", "Deck Building"],
            ["Painting"] = ["Interior Painting", "Exterior Painting", "Wallpaper Removal", "Texture Coating", "Furniture Painting"],
            ["AC & Refrigeration"] = ["AC Installation", "AC Maintenance", "AC Repair", "Duct Cleaning", "Refrigerator Repair"],
            ["Cleaning"] = ["Deep Cleaning", "Office Cleaning", "Carpet Cleaning", "Window Cleaning", "Post-Construction Cleaning"],
            ["Moving Services"] = ["Local Moving", "Packing Service", "Furniture Assembly", "Storage Solutions", "Office Relocation"],
            ["Technology & Smart Home"] = ["Camera Installation", "Network Setup", "Smart Home Setup", "Computer Repair", "CCTV Installation"],
            ["Gardening"] = ["Garden Design", "Lawn Care", "Tree Trimming", "Irrigation Setup", "Landscaping"],
            ["Pest Control"] = ["General Pest Control", "Termite Treatment", "Rodent Control", "Bed Bug Treatment", "Fumigation"],
            ["Automotive"] = ["Oil Change", "Brake Service", "Engine Diagnostics", "Tire Change", "Battery Replacement"],
            ["General Maintenance"] = ["Handyman Services", "Furniture Assembly", "TV Mounting", "Caulking", "Weatherproofing"]
        };

        var newServices = new List<WorkerService>();
        foreach (var profile in newProfiles)
        {
            var profileCategories = categories
                .Where(c => profile.Skills != null && profile.Skills.Contains(c.Name))
                .ToList();

            if (profileCategories.Count == 0)
                profileCategories = categories.OrderBy(_ => faker.Random.Int()).Take(faker.Random.Int(1, 3)).ToList();

            foreach (var cat in profileCategories.Take(faker.Random.Int(1, 3)))
            {
                var titles = serviceTitles.GetValueOrDefault(cat.Name, ["General Service"]);
                var title = faker.PickRandom(titles);

                var service = new WorkerService
                {
                    WorkerProfileId = profile.Id,
                    CategoryId = cat.Id,
                    Title = title,
                    Description = faker.Lorem.Sentence(10),
                    PriceType = faker.PickRandom("Fixed", "Hourly", "Quote"),
                    Price = faker.Random.Decimal(50, 2000),
                    EstimatedDurationMinutes = faker.Random.Int(30, 480),
                    MaterialsIncluded = faker.Random.Bool(0.3f) ? string.Join(", ", faker.Lorem.Words(faker.Random.Int(1, 4))) : null,
                    AvailableCities = profile.ServiceAreas,
                    Tags = string.Join(", ", faker.Lorem.Words(faker.Random.Int(2, 5))),
                    IsActive = true
                };
                newServices.Add(service);
            }
        }

        if (newServices.Count > 0)
        {
            context.WorkerServices.AddRange(newServices);
            await context.SaveChangesAsync();
            services.AddRange(newServices);
        }
        return services;
    }

    private async Task SeedAvailability(AppDbContext context, List<WorkerProfile> profiles)
    {
        var existingProfileIds = await context.WorkerAvailabilities.Select(a => a.WorkerProfileId).Distinct().ToListAsync();
        var newProfiles = profiles.Where(p => !existingProfileIds.Contains(p.Id)).ToList();
        if (newProfiles.Count == 0) return;

        var faker = new Faker();
        var availabilities = new List<WorkerAvailability>();

        foreach (var profile in newProfiles)
        {
            foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
            {
                if (day == DayOfWeek.Friday) continue;

                availabilities.Add(new WorkerAvailability
                {
                    WorkerProfileId = profile.Id,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(faker.Random.Int(7, 10), 0, 0),
                    EndTime = new TimeSpan(faker.Random.Int(16, 20), 0, 0),
                    IsAvailable = faker.Random.Bool(0.9f)
                });
            }
        }

        context.WorkerAvailabilities.AddRange(availabilities);
        await context.SaveChangesAsync();
    }

    private async Task SeedPortfolio(AppDbContext context, List<WorkerProfile> profiles)
    {
        var existingProfileIds = await context.WorkerPortfolioItems.Select(p => p.WorkerProfileId).Distinct().ToListAsync();
        var newProfiles = profiles.Where(p => !existingProfileIds.Contains(p.Id)).ToList();
        if (newProfiles.Count == 0) return;

        var faker = new Faker();
        var items = new List<WorkerPortfolioItem>();

        foreach (var profile in newProfiles.Take(newProfiles.Count / 2))
        {
            foreach (var _ in Enumerable.Range(0, faker.Random.Int(2, 6)))
            {
                items.Add(new WorkerPortfolioItem
                {
                    WorkerProfileId = profile.Id,
                    MediaType = faker.PickRandom("Image", "Image", "Video"),
                    MediaUrl = $"https://picsum.photos/seed/{faker.Random.AlphaNumeric(8)}/800/600",
                    Title = faker.Lorem.Sentence(faker.Random.Int(2, 6)).TrimEnd('.')
                });
            }
        }

        context.WorkerPortfolioItems.AddRange(items);
        await context.SaveChangesAsync();
    }

    private async Task<List<Booking>> SeedBookings(
        AppDbContext context,
        List<WorkerProfile> profiles,
        List<WorkerService> services,
        List<IdentityUser<int>> customers)
    {
        var existingCount = await context.Bookings.CountAsync();
        var targetCount = _args.Bookings;
        var toCreate = targetCount - existingCount;
        if (toCreate <= 0) return await context.Bookings.ToListAsync();
        var bookings = new List<Booking>();

        var faker = new Faker();
        var statusValues = new BookingStatus[]
        {
            BookingStatus.Completed,
            BookingStatus.Cancelled,
            BookingStatus.Started,
            BookingStatus.Pending,
            BookingStatus.Accepted,
            BookingStatus.Scheduled,
            BookingStatus.OnTheWay,
            BookingStatus.Paused,
            BookingStatus.Rejected,
        };
        var statusWeights = new float[]
        {
            0.35f, 0.10f, 0.08f, 0.15f, 0.10f, 0.12f, 0.05f, 0.03f, 0.02f,
        };

        for (int i = 0; i < toCreate; i++)
        {
            var profile = faker.PickRandom(profiles);
            var workerServices = services.Where(s => s.WorkerProfileId == profile.Id).ToList();
            var workerService = workerServices.Count > 0 ? faker.PickRandom(workerServices) : null;
            var customer = faker.PickRandom(customers);
            var status = faker.Random.WeightedRandom(statusValues, statusWeights);

            var createdAt = faker.Date.Between(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow);
            var price = workerService?.Price ?? profile.StartingPrice;
            var commission = price * 0.01m;

            var booking = new Booking
            {
                CustomerId = customer.Id,
                WorkerProfileId = profile.Id,
                WorkerServiceId = workerService?.Id,
                Status = status,
                ScheduledAt = createdAt.AddDays(faker.Random.Int(1, 14)),
                Address = $"{faker.Random.Int(1, 999)} {faker.PickRandom(StreetNames)} St, {faker.PickRandom(Cities)}",
                Notes = faker.Random.Bool(0.4f) ? faker.Lorem.Sentence() : null,
                TotalPrice = price,
                CommissionAmount = commission,
                CreatedAt = createdAt,
                StartedAt = (int)status >= (int)BookingStatus.Started ? createdAt.AddHours(faker.Random.Int(1, 48)) : null,
                CompletedAt = (int)status >= (int)BookingStatus.Completed ? createdAt.AddDays(faker.Random.Int(1, 7)) : null,
                CancelledAt = status == BookingStatus.Cancelled ? createdAt.AddDays(faker.Random.Int(1, 3)) : null,
                CancellationReason = status == BookingStatus.Cancelled ? faker.Lorem.Sentence() : null,
                CancelledBy = status == BookingStatus.Cancelled ? customer.Id.ToString() : null
            };

            bookings.Add(booking);
        }

        context.Bookings.AddRange(bookings);
        await context.SaveChangesAsync();
        return bookings;
    }

    private async Task SeedReviews(
        AppDbContext context,
        List<Booking> bookings,
        List<WorkerProfile> profiles,
        List<IdentityUser<int>> customers)
    {
        var existingBookingIds = await context.Reviews.Select(r => r.BookingId).Distinct().ToListAsync();
        var completedBookings = bookings.Where(b => b.Status == BookingStatus.Completed && !existingBookingIds.Contains(b.Id)).Take(_args.Reviews).ToList();
        if (completedBookings.Count == 0) return;

        var faker = new Faker();
        var reviewTexts = new[]
        {
            "Excellent work, very professional.",
            "Did a great job, highly recommend.",
            "Good service, reasonable price.",
            "Decent work, but took longer than expected.",
            "Very satisfied with the result.",
            "Would hire again for sure.",
            "Average experience, nothing special.",
            "Professional and on time.",
            "Great attention to detail.",
            "Fair price for the quality of work."
        };

        var reviews = new List<Review>();

        foreach (var booking in completedBookings)
        {
            reviews.Add(new Review
            {
                BookingId = booking.Id,
                CustomerId = booking.CustomerId,
                WorkerProfileId = booking.WorkerProfileId,
                Rating = faker.Random.Int(1, 5),
                Comment = faker.Random.Bool(0.7f) ? faker.PickRandom(reviewTexts) : null,
                CreatedAt = (booking.CompletedAt ?? booking.CreatedAt).AddHours(faker.Random.Int(1, 72)),
                WorkerReply = faker.Random.Bool(0.4f) ? "Thank you for your feedback!" : null
            });
        }

        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();
    }

    private async Task SeedPayments(AppDbContext context, List<Booking> bookings)
    {
        var existingBookingIds = await context.Payments.Select(p => p.BookingId).Distinct().ToListAsync();
        var completedBookings = bookings.Where(b => b.Status == BookingStatus.Completed && !existingBookingIds.Contains(b.Id)).Take(_args.Payments).ToList();
        if (completedBookings.Count == 0) return;

        var faker = new Faker();
        var payments = new List<Payment>();

        foreach (var booking in completedBookings)
        {
            payments.Add(new Payment
            {
                BookingId = booking.Id,
                Amount = booking.TotalPrice,
                CommissionAmount = booking.CommissionAmount,
                PaymentMethod = faker.PickRandom("Credit Card", "Cash", "Bank Transfer", "Wallet"),
                Status = faker.Random.Bool(0.9f) ? "Completed" : "Pending",
                CompletedAt = booking.CompletedAt,
                TransactionReference = faker.Random.Bool(0.9f) ? $"TXN-{faker.Random.AlphaNumeric(12).ToUpper()}" : null,
                CreatedAt = booking.CompletedAt ?? booking.CreatedAt
            });
        }

        context.Payments.AddRange(payments);
        await context.SaveChangesAsync();
    }

    private async Task SeedInvoices(AppDbContext context, List<Booking> bookings)
    {
        var existingBookingIds = await context.Invoices.Select(i => i.BookingId).Distinct().ToListAsync();
        var paidBookings = bookings.Where(b => b.Status == BookingStatus.Completed && !existingBookingIds.Contains(b.Id)).Take(_args.Invoices).ToList();
        if (paidBookings.Count == 0) return;

        var faker = new Faker();
        var invoices = new List<Invoice>();
        int invoiceCounter = 1001;

        foreach (var booking in paidBookings)
        {
            var workerAmount = booking.TotalPrice - booking.CommissionAmount;
            invoices.Add(new Invoice
            {
                BookingId = booking.Id,
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMM}-{invoiceCounter++:D4}",
                Amount = booking.TotalPrice,
                CommissionAmount = booking.CommissionAmount,
                WorkerAmount = workerAmount,
                FileUrl = faker.Random.Bool(0.5f) ? $"/invoices/invoice-{invoiceCounter - 1}.pdf" : null,
                CreatedAt = booking.CompletedAt ?? booking.CreatedAt
            });
        }

        context.Invoices.AddRange(invoices);
        await context.SaveChangesAsync();
    }

    private async Task<List<Conversation>> SeedConversations(
        AppDbContext context,
        List<WorkerProfile> profiles,
        List<IdentityUser<int>> customers)
    {
        var existingPairs = await context.ConversationParticipants
            .GroupBy(p => p.ConversationId)
            .Select(g => g.Select(p => p.UserId).OrderBy(id => id).ToList())
            .ToListAsync();
        var existingPairSet = existingPairs
            .Select(pair => string.Join(":", pair))
            .ToHashSet();

        var conversations = new List<Conversation>();
        var existingCount = await context.Conversations.CountAsync();
        var toCreate = _args.Conversations - existingCount;
        if (toCreate <= 0) return await context.Conversations.ToListAsync();

        var faker = new Faker();
        var usedPairs = new HashSet<string>(existingPairSet);
        var attempts = 0;

        for (int i = 0; i < toCreate && attempts < toCreate * 3; i++, attempts++)
        {
            var profile = faker.PickRandom(profiles);
            var customer = faker.PickRandom(customers);
            var pairKey = $"{profile.UserId}:{customer.Id}";
            if (usedPairs.Contains(pairKey)) { i--; continue; }
            usedPairs.Add(pairKey);

            var conversation = new Conversation
            {
                CreatedAt = faker.Date.Between(DateTime.UtcNow.AddMonths(-3), DateTime.UtcNow)
            };

            conversation.Participants = new List<ConversationParticipant>
            {
                new() { UserId = profile.UserId, JoinedAt = conversation.CreatedAt, Conversation = conversation },
                new() { UserId = customer.Id, JoinedAt = conversation.CreatedAt, Conversation = conversation }
            };

            conversations.Add(conversation);
        }

        context.Conversations.AddRange(conversations);
        await context.SaveChangesAsync();
        return conversations;
    }

    private async Task SeedMessages(
        AppDbContext context,
        List<Conversation> conversations,
        List<WorkerProfile> profiles,
        List<IdentityUser<int>> customers)
    {
        var existingMessageCount = await context.Messages.CountAsync();
        var toCreate = _args.Messages - existingMessageCount;
        if (toCreate <= 0) return;

        var existingConversationIds = await context.Messages.Select(m => m.ConversationId).Distinct().ToListAsync();
        var newConversations = conversations.Where(c => !existingConversationIds.Contains(c.Id)).ToList();
        if (newConversations.Count == 0 && existingConversationIds.Count > 0)
            newConversations = conversations;

        var faker = new Faker();
        var messages = new List<Message>();
        var sampleMessages = new[]
        {
            "Hi, I'm interested in your service.",
            "What's your availability next week?",
            "Can you provide a quote?",
            "Yes, I'm available on Monday.",
            "The total will be around $150.",
            "Great, let's schedule for Tuesday.",
            "I'll be there at 10 AM.",
            "Perfect, see you then.",
            "I'm running 15 minutes late.",
            "No problem, thanks for letting me know.",
            "The job is done. Please check.",
            "Looks great, thank you!",
            "Can you send me the invoice?",
            "Sure, I'll send it right away.",
            "Do you offer warranty on your work?",
            "Yes, 6 months warranty included.",
            "I need to reschedule.",
            "No problem, what time works for you?",
            "Thanks for the great service!",
            "You're welcome, happy to help!"
        };

        int messageCount = 0;
        var allUserIds = profiles.Select(p => p.UserId).Concat(customers.Select(c => c.Id)).ToList();

        foreach (var conversation in newConversations)
        {
            var participantIds = conversation.Participants?.Select(p => p.UserId).ToList()
                ?? await context.ConversationParticipants
                    .Where(p => p.ConversationId == conversation.Id)
                    .Select(p => p.UserId)
                    .ToListAsync();
            if (participantIds.Count < 2) continue;

            int msgPerConversation = Math.Max(1, toCreate / Math.Max(1, newConversations.Count));
            int actual = Math.Min(msgPerConversation, toCreate - messageCount);
            if (actual <= 0) break;

            for (int j = 0; j < actual; j++)
            {
                var senderId = participantIds[j % 2];
                var messageDate = conversation.CreatedAt.AddHours(j * faker.Random.Int(1, 12));

                var message = new Message
                {
                    ConversationId = conversation.Id,
                    SenderId = senderId,
                    MessageType = MessageType.Text,
                    Content = faker.PickRandom(sampleMessages),
                    CreatedAt = messageDate,
                    DeliveredAt = messageDate.AddSeconds(faker.Random.Int(1, 60)),
                    ReadAt = faker.Random.Bool(0.7f) ? messageDate.AddMinutes(faker.Random.Int(1, 120)) : null
                };
                messages.Add(message);
                messageCount++;
            }
        }

        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();

        var orderedMessages = messages.OrderBy(m => m.CreatedAt).ToList();
        var messageGroups = orderedMessages.GroupBy(m => m.ConversationId);
        foreach (var group in messageGroups)
        {
            var lastMsg = group.Last();
            var conv = conversations.FirstOrDefault(c => c.Id == group.Key);
            if (conv != null)
            {
                conv.LastMessageId = lastMsg.Id;
                conv.LastMessageContent = lastMsg.Content;
                conv.LastMessageAt = lastMsg.CreatedAt;
            }
        }
        await context.SaveChangesAsync();

        if (_args.Attachments > 0)
        {
            var attachableMessages = messages.Take(_args.Attachments).ToList();
            var attachments = new List<MessageAttachment>();
            foreach (var msg in attachableMessages)
            {
                attachments.Add(new MessageAttachment
                {
                    MessageId = msg.Id,
                    FileName = $"file_{faker.Random.AlphaNumeric(8)}.{faker.PickRandom("jpg", "png", "pdf", "docx")}",
                    FilePath = $"/uploads/{faker.Random.AlphaNumeric(16)}",
                    ContentType = faker.PickRandom("image/jpeg", "image/png", "application/pdf"),
                    FileSize = faker.Random.Long(1024, 10 * 1024 * 1024),
                    AttachmentType = faker.PickRandom("Image", "File", "Image"),
                    CreatedAt = msg.CreatedAt
                });
            }
            context.MessageAttachments.AddRange(attachments);
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedFavorites(
        AppDbContext context,
        List<IdentityUser<int>> customers,
        List<WorkerProfile> profiles,
        List<WorkerService> services)
    {
        var existingPairs = await context.Favorites
            .Select(f => f.CustomerId + ":" + (f.WorkerProfileId ?? 0))
            .Distinct()
            .ToListAsync();
        var usedFavs = new HashSet<string>(existingPairs);

        var faker = new Faker();
        var favorites = new List<Favorite>();

        foreach (var customer in customers)
        {
            var favProfiles = faker.PickRandom(profiles, faker.Random.Int(0, 5));
            foreach (var profile in favProfiles)
            {
                var key = $"{customer.Id}:{profile.Id}";
                if (usedFavs.Add(key))
                {
                    var service = services.FirstOrDefault(s => s.WorkerProfileId == profile.Id);
                    favorites.Add(new Favorite
                    {
                        CustomerId = customer.Id,
                        WorkerProfileId = profile.Id,
                        WorkerServiceId = service?.Id
                    });
                }
            }
        }

        context.Favorites.AddRange(favorites);
        await context.SaveChangesAsync();
    }

    private async Task SeedRefreshTokens(AppDbContext context, List<IdentityUser<int>> users)
    {
        if (await context.Set<RefreshToken>().AnyAsync()) return;

        var faker = new Faker();
        var tokens = new List<RefreshToken>();

        foreach (var user in users.Take(users.Count / 2))
        {
            for (int i = 0; i < faker.Random.Int(1, 3); i++)
            {
                tokens.Add(new RefreshToken
                {
                    UserId = user.Id,
                    Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray().Concat(Guid.NewGuid().ToByteArray()).ToArray()),
                    ExpiresAt = DateTime.UtcNow.AddDays(faker.Random.Int(1, 14)),
                    CreatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(1, 30)),
                    IsRevoked = faker.Random.Bool(0.2f)
                });
            }
        }

        context.Set<RefreshToken>().AddRange(tokens);
        await context.SaveChangesAsync();
    }

    private async Task SeedAuditLogs(AppDbContext context, List<IdentityUser<int>> users)
    {
        if (await context.AuditLogs.AnyAsync()) return;

        var faker = new Faker();
        var actions = new[] { "Created", "Updated", "Deleted", "Viewed", "Login", "Logout", "Payment" };
        var entities = new[] { "Booking", "User", "WorkerProfile", "Payment", "Review", "Message", "Conversation" };
        var logs = new List<AuditLog>();

        foreach (var user in users.Take(users.Count / 3))
        {
            foreach (var _ in Enumerable.Range(0, faker.Random.Int(3, 10)))
            {
                logs.Add(new AuditLog
                {
                    UserId = user.Id,
                    Action = faker.PickRandom(actions),
                    EntityType = faker.PickRandom(entities),
                    EntityId = faker.Random.Int(1, 1000).ToString(),
                    OldValues = faker.Random.Bool(0.3f) ? "{\"status\":\"old\"}" : null,
                    NewValues = faker.Random.Bool(0.5f) ? "{\"status\":\"new\"}" : null,
                    Timestamp = faker.Date.Between(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow),
                    IpAddress = faker.Internet.Ip()
                });
            }
        }

        context.AuditLogs.AddRange(logs);
        await context.SaveChangesAsync();
    }
}
