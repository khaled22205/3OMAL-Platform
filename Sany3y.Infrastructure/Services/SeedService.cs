using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;

namespace Sany3y.Infrastructure.Services
{
    public class SeedService
    {
        private static readonly List<string> Roles = new() { "Admin", "Technician", "Client" };
        private static readonly List<Dictionary<string, string>> Categories = new()
        {
            new() { { "Name", "بناء وتشييد" }, { "Description", "أعمال تأسيس وبناء المنازل والعمارات وتجهيز الأساسات." } },
            new() { { "Name", "كهرباء" }, { "Description", "تمديدات كهرباء، صيانة الأعطال، وتركيب اللوحات والإضاءة." } },
            new() { { "Name", "سباكة" }, { "Description", "تأسيس وصيانة مواسير المياه والصرف الصحي وتركيب الحمامات." } },
            new() { { "Name", "دهانات وتشطيبات" }, { "Description", "تشطيب الشقق والدهانات الداخلية والخارجية وتنفيذ الديكورات." } },
            new() { { "Name", "نجارة" }, { "Description", "تصنيع وتركيب الأبواب والشبابيك والمفروشات الخشبية." } },
            new() { { "Name", "حدادة" }, { "Description", "تشكيل المعادن، بوابات حديد، شبابيك حديد، ومظلات." } },
            new() { { "Name", "ألوميتال" }, { "Description", "تصميم وتركيب شبابيك وأبواب ألوميتال وأوجه زجاج حديث." } },
            new() { { "Name", "مقاولات عامة" }, { "Description", "إدارة وتنفيذ مشاريع البناء بكافة مراحلها." } },
            new() { { "Name", "رخام وسيراميك" }, { "Description", "توريد وتركيب رخام وسيراميك الأرضيات والحائط." } },
            new() { { "Name", "نقاشة" }, { "Description", "دهانات زخرفية، نقشات، وتغيير ديكور الغرف." } },
            new() { { "Name", "تكييف وتبريد" }, { "Description", "تركيب وصيانة المكيفات المنزلية ومبردات الهواء." } },
            new() { { "Name", "صيانة أجهزة" }, { "Description", "صيانة وتصليح الأجهزة المنزلية مثل الغسالات والثلاجات." } },
            new() { { "Name", "تركيبات" }, { "Description", "تركيب باركيه، ستائر، إضاءة، كنترول وغيرها." } },
            new() { { "Name", "تنظيف وتجهيز" }, { "Description", "تنظيف شقق، فيلات، محلات، وتجهيزها للاستلام." } },
            new() { { "Name", "نقل عفش وخدمات لوجستية" }, { "Description", "نقل الأثاث والعفش بين المدن وترتيب المخازن." } },
            new() { { "Name", "خدمات أخرى" }, { "Description", "أي خدمة صنايعية أو فنية لا تندرج تحت التصنيفات السابقة." } }
        };

        public static async System.Threading.Tasks.Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var addressRepository = scope.ServiceProvider.GetRequiredService<IRepository<Address>>();
            var categoryRepository = scope.ServiceProvider.GetRequiredService<IRepository<Category>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                // Seed roles
                logger.LogInformation("Seeding roles...");
                await SeedRolesAsync(roleManager, logger);

                // Seed admin user
                logger.LogInformation("Seeding admin user...");
                await SeedAdminUserAsync(userManager, addressRepository, logger);

                // Seed categories
                logger.LogInformation("Seeding categories...");
                await SeedCategoriesAsync(categoryRepository, logger);

                // Seed schedules
                logger.LogInformation("Seeding technician schedules...");
                await SeedTechnicianSchedulesAsync(context, userManager, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async System.Threading.Tasks.Task SeedTechnicianSchedulesAsync(AppDbContext context, UserManager<User> userManager, ILogger logger)
        {
            var technicians = await userManager.GetUsersInRoleAsync("Technician");
            if (!technicians.Any())
            {
                logger.LogInformation("No technicians found to seed schedules.");
                return;
            }

            foreach (var tech in technicians)
            {
                var hasSlots = context.TechnicianSchedules.Any(s => s.TechnicianId == tech.Id);
                if (!hasSlots)
                {
                    logger.LogInformation($"Seeding 3 mock slots for technician: {tech.FirstName} {tech.LastName} (ID: {tech.Id})");
                    
                    var today = DateTime.Today;
                    
                    var slot1 = new TechnicianSchedule
                    {
                        TechnicianId = tech.Id,
                        Date = today,
                        StartTime = new TimeSpan(10, 0, 0), // 10 AM
                        EndTime = new TimeSpan(12, 0, 0),   // 12 PM
                        IsBooked = false
                    };
                    
                    var slot2 = new TechnicianSchedule
                    {
                        TechnicianId = tech.Id,
                        Date = today.AddDays(1),
                        StartTime = new TimeSpan(14, 0, 0), // 2 PM
                        EndTime = new TimeSpan(16, 0, 0),   // 4 PM
                        IsBooked = false
                    };
                    
                    var slot3 = new TechnicianSchedule
                    {
                        TechnicianId = tech.Id,
                        Date = today.AddDays(2),
                        StartTime = new TimeSpan(16, 0, 0), // 4 PM
                        EndTime = new TimeSpan(18, 0, 0),   // 6 PM
                        IsBooked = false
                    };

                    context.TechnicianSchedules.AddRange(slot1, slot2, slot3);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async System.Threading.Tasks.Task SeedRolesAsync(RoleManager<Role> roleManager, ILogger logger)
        {
            foreach (var roleName in Roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new Role { Name = roleName });
                    if (result.Succeeded)
                        logger.LogInformation($"✅ Role '{roleName}' created successfully.");
                    else
                        logger.LogError($"❌ Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private static async System.Threading.Tasks.Task SeedAdminUserAsync(UserManager<User> userManager, IRepository<Address> addressRepository, ILogger logger)
        {
            const string adminEmail = "sany3y.admin@gmail.com";
            const string adminPassword = "Admin@123";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin != null)
            {
                logger.LogInformation("Admin user already exists.");
                return;
            }

            var address = new Address { Governorate = "القاهرة", City = "القاهرة الجديدة", Street = "القاهرة الجديدة" };
            await addressRepository.Add(address);

            var adminUser = new User
            {
                NationalId = 10000000000001,
                FirstName = "Sany3y",
                LastName = "Admin",
                UserName = "Admin",
                Email = adminEmail,
                Gender = 'M',
                BirthDate = new DateTime(2005, 11, 16),
                PhoneNumber = ".",
                AddressId = address.Id,
                PasswordHash = adminPassword,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Admin user created and assigned 'Admin' role.");
            }
            else
            {
                logger.LogError($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        private static async System.Threading.Tasks.Task SeedCategoriesAsync(IRepository<Category> categoryRepository, ILogger logger)
        {
            var allCategories = await categoryRepository.GetAll();
            if (allCategories != null && allCategories.Count > 0)
                return;

            foreach (var category in Categories)
            {
                if (!allCategories.Any(c => c.Name == category["Name"]))
                {
                    await categoryRepository.Add(new Category { Name = category["Name"], Description = category["Description"] });
                        logger.LogInformation($"✅ Category '{category["Name"]}' added successfully.");
                }
                else
                    logger.LogInformation($"⚠️ Category '{category["Name"]}' already exists.");
            }
        }
    }
}
