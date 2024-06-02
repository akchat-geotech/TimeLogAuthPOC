using Microsoft.AspNetCore.Identity;

namespace TimeLogAuthPOC.Entity
{
    public class Role : IdentityRole
    {
        public IList<UserProjectRole> UserProjectRoles { get; set; }
    }
}
