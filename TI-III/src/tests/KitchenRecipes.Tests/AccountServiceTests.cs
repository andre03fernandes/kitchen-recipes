using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Application.Services;
using KitchenRecipes.Domain.Common;

namespace KitchenRecipes.Tests;

public sealed class AccountServiceTests
{
    [Fact]
    public async Task RegisterAsync_FirstUser_BecomesAdmin()
    {
        await using var context = TestApplicationDbContextFactory.Create();
        var service = new AccountService(context);

        var result = await service.RegisterAsync(new RegisterUserRequest("First Admin", "admin@test.local", "P@ssw0rd123!"));

        Assert.Equal(UserRoles.Admin, result.Role);
        Assert.Equal("admin@test.local", result.Email);
    }

    [Fact]
    public async Task RegisterAsync_SecondUser_BecomesRegularUser()
    {
        await using var context = TestApplicationDbContextFactory.Create();
        var service = new AccountService(context);

        await service.RegisterAsync(new RegisterUserRequest("First Admin", "admin@test.local", "P@ssw0rd123!"));
        var second = await service.RegisterAsync(new RegisterUserRequest("Second User", "user@test.local", "P@ssw0rd123!"));

        Assert.Equal(UserRoles.User, second.Role);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithValidPassword_ReturnsUser()
    {
        await using var context = TestApplicationDbContextFactory.Create();
        var service = new AccountService(context);

        await service.RegisterAsync(new RegisterUserRequest("Test User", "user@test.local", "P@ssw0rd123!"));
        var result = await service.ValidateCredentialsAsync(new LoginRequest("user@test.local", "P@ssw0rd123!"));

        Assert.NotNull(result);
        Assert.Equal("user@test.local", result.Email);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsArgumentException()
    {
        await using var context = TestApplicationDbContextFactory.Create();
        var service = new AccountService(context);

        await service.RegisterAsync(new RegisterUserRequest("Test User", "user@test.local", "P@ssw0rd123!"));

        var action = async () => await service.RegisterAsync(new RegisterUserRequest("Other User", "user@test.local", "P@ssw0rd123!"));

        var exception = await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Equal("An account with this email already exists.", exception.Message);
    }
}
