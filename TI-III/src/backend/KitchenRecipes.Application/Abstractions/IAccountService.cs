using KitchenRecipes.Application.DTOs;

namespace KitchenRecipes.Application.Abstractions;

public interface IAccountService
{
    Task<AuthUserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<AuthUserDto?> ValidateCredentialsAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthUserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuthUserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateRoleAsync(int userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
}
