using ForwardingLetterPortal.Interface.Constants;
using System.Security.Claims;

namespace ForwardingLetterPortal.API.Infrastructure
{
    public static class ClaimsExtensions
    {
        public static int OfficerId(this ClaimsPrincipal user)
            => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public static bool IsSuperAdmin(this ClaimsPrincipal user)
            => user.IsInRole(Roles.SuperAdmin);

        public static string? ClientIp(this HttpContext ctx)
            => ctx.Connection.RemoteIpAddress?.ToString();
    }
}
