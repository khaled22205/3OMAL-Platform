namespace src.DTOs.Admin;

public class DashboardStatsResponse
{
    public int TotalUsers { get; set; }
    public int TotalWorkers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int CompletedBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public int TotalCategories { get; set; }
    public int PendingApprovals { get; set; }
    public List<MonthlyStats> MonthlyBookings { get; set; } = [];
    public List<TopCategoryStats> TopCategories { get; set; } = [];
}

public class MonthlyStats
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}

public class TopCategoryStats
{
    public string CategoryName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
}

public class UserManagementResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUserUpdateRequest
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
    public bool? LockoutEnabled { get; set; }
}