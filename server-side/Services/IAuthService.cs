using server_side.Entities;

namespace server_side.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterUser(UserDto userDto);
        Task<string?> ValidateUser(UserDto userDto);
    }
}