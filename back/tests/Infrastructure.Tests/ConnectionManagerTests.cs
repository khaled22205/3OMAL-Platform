using FluentAssertions;
using Infrastructure.Services;

namespace Infrastructure.Tests;

public class ConnectionManagerTests
{
    private readonly ConnectionManager _sut = new();

    [Fact]
    public void AddConnection_Should_store_user_and_connection()
    {
        _sut.AddConnection(1, "conn1");
        _sut.IsUserOnline(1).Should().BeTrue();
        _sut.GetConnections(1).Should().Contain("conn1");
    }

    [Fact]
    public void AddConnection_Should_support_multiple_connections_for_same_user()
    {
        _sut.AddConnection(1, "conn1");
        _sut.AddConnection(1, "conn2");
        _sut.GetConnections(1).Should().HaveCount(2);
    }

    [Fact]
    public void RemoveConnection_Should_cleanup_user_when_last_connection_removed()
    {
        _sut.AddConnection(1, "conn1");
        _sut.RemoveConnection("conn1");
        _sut.IsUserOnline(1).Should().BeFalse();
    }

    [Fact]
    public void RemoveConnection_Should_not_remove_user_if_other_connections_exist()
    {
        _sut.AddConnection(1, "conn1");
        _sut.AddConnection(1, "conn2");
        _sut.RemoveConnection("conn1");
        _sut.IsUserOnline(1).Should().BeTrue();
        _sut.GetConnections(1).Should().ContainSingle("conn2");
    }

    [Fact]
    public void GetUserId_Should_return_correct_user()
    {
        _sut.AddConnection(1, "conn1");
        var userId = _sut.GetUserId("conn1");
        userId.Should().Be(1);
    }

    [Fact]
    public void GetUserId_Should_return_null_for_unknown_connection()
    {
        var userId = _sut.GetUserId("unknown");
        userId.Should().BeNull();
    }

    [Fact]
    public void IsUserOnline_Should_return_false_for_unknown_user()
    {
        _sut.IsUserOnline(999).Should().BeFalse();
    }
}
