namespace KitchenRecipes.Application.DTOs;

public sealed record RegisterUserRequest(string FullName, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthUserDto(int Id, string FullName, string Email, string Role, bool IsActive);
public sealed record UpdateUserRoleRequest(string Role);
