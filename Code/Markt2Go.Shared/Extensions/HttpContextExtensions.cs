using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Markt2Go.Shared.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetUserIdFromToken(this HttpContext context)
        {
            var userId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                return userId.Value;
            }
            else
            {
                throw new System.ArgumentNullException("UserId can't be null");
            }
        }
    }
}