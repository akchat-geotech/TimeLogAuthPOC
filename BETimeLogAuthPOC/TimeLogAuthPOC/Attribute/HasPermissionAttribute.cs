using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;

namespace TimeLogAuthPOC.Attribute
{
    public class HasPermissionAttribute : TypeFilterAttribute
    {
        public HasPermissionAttribute(string resource, string action) : base(typeof(HasPermissionFilter))
        {
            Arguments = new object[] { new PermissionRequirement(resource, action) };
        }

        public class HasPermissionFilter : IAuthorizationFilter
        {
            private readonly PermissionRequirement _requirement;

            public HasPermissionFilter(PermissionRequirement requirement)
            {
                _requirement = requirement;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var user = context.HttpContext.User;

                if (!user.Identity.IsAuthenticated)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                var hasPermission = user.Claims.Any(c =>
                    user.HasClaim(claim => claim.Type == _requirement.ClaimType && claim.Value == _requirement.ClaimValue)
                );

                if (!hasPermission)
                {
                    context.Result = new ForbidResult();
                }
            }
        }

        public class PermissionRequirement
        {
            public string ClaimType { get; }
            public string ClaimValue { get; }

            public PermissionRequirement(string claimType, string claimValue)
            {
                ClaimType = claimType;
                ClaimValue = claimValue;
            }

        }
    }
}
