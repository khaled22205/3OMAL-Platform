namespace Domain.DomainServices;

public static class CommissionCalculator
{
    public static decimal Calculate(decimal price, double percentage)
    {
        return price * (decimal)(percentage / 100);
    }
}
