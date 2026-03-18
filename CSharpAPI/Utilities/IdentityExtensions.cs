using System.Security.Claims;
using System.Security.Principal;

namespace API.Utilities
{
    public static class IdentityExtensions
    {
        public static int GetCodUser(this IIdentity identity)
        {

            var claimsIdentity = (ClaimsIdentity)identity;
            var claim = claimsIdentity.Claims.Where(x => x.Type.Equals("CodUser"))
                .Select(x => x.Value).First();

            return int.Parse(claim);
        }
    }
}
