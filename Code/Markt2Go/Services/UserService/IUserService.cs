using System.Collections.Generic;
using System.Threading.Tasks;
using Markt2Go.DTOs.User;

namespace Markt2Go.Services.UserService
{
    public interface IUserService
    {
        Task<ServiceResponse<List<GetUserDTO>>> GetAllUsers();
        Task<ServiceResponse<GetUserDTO>> GetUser(string id);

        Task<ServiceResponse<GetUserDTO>> AddUser(string id, AddUserDTO newUser);
        Task<ServiceResponse<GetUserDTO>> UpdateUser(UpdateUserDTO updatedUser);
        Task<ServiceResponse<GetUserDTO>> DeleteUser(string id);

        Task<ServiceResponse> UpdateUserSellerId(string userId, long sellerId);
    }
}