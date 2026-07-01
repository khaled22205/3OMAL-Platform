using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Application.Features.Auth;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequest>();
        return services;
    }
}
