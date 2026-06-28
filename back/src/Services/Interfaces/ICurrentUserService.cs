namespace src.Services.Interfaces;

public interface ICurrentUserService
{
    int? GetUserId();
    string? GetUserEmail();
    List<string> GetUserRoles();
    bool IsInRole(string role);
}