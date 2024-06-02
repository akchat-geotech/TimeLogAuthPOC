using Microsoft.AspNetCore.Authorization;

namespace TimeLogAuthPOC.Attribute
{
    public class TimeLogHandler : AuthorizationHandler<TimeLogRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TimeLogRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == requirement.ClaimType && c.Value == requirement.ClaimValue))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}


public class TimeLogRequirement : IAuthorizationRequirement
{
    public string ClaimType { get; }
    public string ClaimValue { get; }

    public TimeLogRequirement(string claimType, string claimValue)
    {
        ClaimType = claimType;
        ClaimValue = claimValue;
    }
}