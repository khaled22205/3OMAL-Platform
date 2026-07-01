using System.Text.RegularExpressions;

namespace Domain.DomainServices;

public static partial class StringHelper
{
    public static string ToSeoUrl(string input)
    {
        var slug = input.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
        return slug;
    }

    public static string GenerateInvoiceNumber(int bookingId)
    {
        var now = DateTime.UtcNow;
        return $"INV-{now:yyyyMMdd}-{bookingId:D5}";
    }
}
