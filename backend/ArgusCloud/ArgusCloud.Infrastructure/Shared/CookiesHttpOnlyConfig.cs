using Microsoft.AspNetCore.Http;

namespace ArgusCloud.Infrastructure.Shared
{
    public static class CookiesHttpOnlyConfig
    {
        public static readonly CookieOptions cookieOptions = new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
        };
    }
}
