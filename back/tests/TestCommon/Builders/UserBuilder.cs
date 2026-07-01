using Domain.Entities;

namespace TestCommon.Builders;

public class UserBuilder
{
    private int _id = 1;
    private string _email = "test@example.com";
    private string? _phone = "1234567890";
    private readonly List<string> _roles = ["Customer"];
    private string? _firstName = "Test";
    private string? _lastName = "User";

    public UserBuilder WithId(int id) { _id = id; return this; }
    public UserBuilder WithEmail(string email) { _email = email; return this; }
    public UserBuilder WithPhone(string? phone) { _phone = phone; return this; }
    public UserBuilder WithRole(string role) { _roles.Clear(); _roles.Add(role); return this; }
    public UserBuilder WithRoles(params string[] roles) { _roles.Clear(); _roles.AddRange(roles); return this; }
    public UserBuilder WithName(string first, string last) { _firstName = first; _lastName = last; return this; }

    public (int id, string email, string? phone, List<string> roles) Build()
        => (_id, _email, _phone, _roles);

    public (int id, string firstName, string lastName, string? photo) BuildBrief()
        => (_id, _firstName!, _lastName!, null);

    public static (int id, string email, string? phone, List<string> roles) CreateCustomer(int id = 1)
        => new UserBuilder().WithId(id).WithRole("Customer").Build();

    public static (int id, string email, string? phone, List<string> roles) CreateWorker(int id = 2)
        => new UserBuilder().WithId(id).WithEmail("worker@test.com").WithRole("Worker").Build();
}
