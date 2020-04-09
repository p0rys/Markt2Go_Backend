using System.Collections.Generic;
using System.Threading.Tasks;

namespace Markt2Go.Services.Auth0Service
{
    public interface IAuth0Service
    {
        Task<IDictionary<string, object>> GetUserInfo(string userToken);
        Task<bool> DeleteUser(string id);
    }
}