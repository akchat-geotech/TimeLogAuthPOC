using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace TimeLogAuthPOC.Entity
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [AllowNull]
        public string ProfilePicture { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return $"{LastName} {FirstName}";
            }
        }

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public IList<UserProjectRole> UserProjectRoles { get; set; }
    }
}
