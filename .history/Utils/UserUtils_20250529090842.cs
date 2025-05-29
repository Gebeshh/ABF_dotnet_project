using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TestNo_9999999.Utils
{
    public static class UserUtils
    {
        public static string GetUserName(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                return context.User.Identity.Name;
            }
            return null;
        }

        public static string GetUserId(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userIdClaim = context.User.FindFirst("UserId") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
                return userIdClaim?.Value;
            }
            return null;
        }

        public static string GetUserDepartment(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var departmentClaim = context.User.FindFirst("Department");
                return departmentClaim?.Value;
            }
            return null;
        }

        public static bool IsInRole(HttpContext context, string role)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                return context.User.IsInRole(role);
            }
            return false;
        }

        // Helper method to get claim value
        public static string GetClaimValue(HttpContext context, string claimType)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var claim = context.User.FindFirst(claimType);
                return claim?.Value;
            }
            return null;
        }
    }
}
