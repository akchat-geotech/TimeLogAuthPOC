using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimeLogAuthPOC.Entity;
using TimeLogAuthPOC.Entity.Enum;

namespace TimeLogAuthPOC.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Project> Project { get; set; }
        public DbSet<UserProjectRole> UserProjectRole { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserProjectRole>()
                .HasKey(upr => new { upr.UserId, upr.RoleId, upr.ProjectId });

            builder.Entity<UserProjectRole>()
                .HasOne(upr => upr.User)
                .WithMany(u => u.UserProjectRoles)
                .HasForeignKey(upr => upr.UserId);

            builder.Entity<UserProjectRole>()
                .HasOne(upr => upr.Role)
                .WithMany(r => r.UserProjectRoles)
                .HasForeignKey(upr => upr.RoleId);

            builder.Entity<UserProjectRole>()
                .HasOne(upr => upr.Project)
                .WithMany(p => p.UserProjectRoles)
                .HasForeignKey(upr => upr.ProjectId);
        }

        public async Task SeedProjectsRolesAndUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            await SeedProjects();
            await SeedRolesAndClaims(roleManager);

            var userEmail = "akchat.gupta@geotechinfo.net";
            var user = await userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                user = new User
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FirstName = "Akchat",
                    LastName = "Gupta",
                    ProfilePicture = ""
                };
                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception($"Error creating user: {string.Join(",", result.Errors.Select(e => e.Description))}");
                }
            }

            var projects = await Project.ToListAsync();
            foreach (var project in projects)
            {
                await AssignUserToProjectRole(userManager, roleManager, user, UserRole.Manager, project.ProjectId);
                await AssignUserToProjectRole(userManager, roleManager, user, UserRole.ProjectMember, project.ProjectId);
            }
        }

        private async Task SeedProjects()
        {
            var projects = new[]
            {
                new Project { ProjectId = "1", Name = "Project1" },
                new Project { ProjectId = "2", Name = "Project2" }
            };

            foreach (var project in projects)
            {
                if (await Project.FindAsync(project.ProjectId) == null)
                {
                    Project.Add(project);
                }
            }
            await SaveChangesAsync();
        }

        public async Task AssignUserToProjectRole(UserManager<User> userManager, RoleManager<Role> roleManager, User user, UserRole role, string projectId)
        {
            var roleName = role.ToString();
            var applicationRole = await roleManager.FindByNameAsync(roleName);
            if (applicationRole == null)
            {
                applicationRole = new Role { Name = roleName };
                await roleManager.CreateAsync(applicationRole);
            }

            var userProjectRole = new UserProjectRole
            {
                UserId = user.Id,
                RoleId = applicationRole.Id,
                ProjectId = projectId
            };

            if (!UserProjectRole.Any(upr => upr.UserId == user.Id && upr.RoleId == applicationRole.Id && upr.ProjectId == projectId))
            {
                UserProjectRole.Add(userProjectRole);
                await SaveChangesAsync();
            }
        }

        private async Task SeedRolesAndClaims(RoleManager<Role> roleManager)
        {
            var roleNames = Enum.GetNames(typeof(UserRole));
            foreach (var roleName in roleNames)
            {
                if (await roleManager.FindByNameAsync(roleName) == null)
                {
                    await roleManager.CreateAsync(new Role { Name = roleName });
                }
            }

            var roleClaims = new Dictionary<string, List<(string resource, string action)>>
            {
                ["Manager"] = new List<(string, string)>
                {
                    ("project", "create"),
                    ("project", "edit"),
                    ("project", "update"),
                    ("project", "assign"),
                    ("project", "remove_member"),
                    ("project", "change_role"),
                    ("timelog", "view"),
                    ("timelog", "edit")
                },
                ["ProjectLead"] = new List<(string, string)>
                {
                    ("project", "view"),
                    ("project", "add_member"),
                    ("project", "remove_member"),
                    ("project", "change_role"),
                    ("timelog", "create"),
                    ("timelog", "edit"),
                    ("timelog", "view")
                },
                ["SubLead"] = new List<(string, string)>
                {
                    ("timelog", "create"),
                    ("timelog", "edit"),
                    ("timelog", "view")
                },
                ["ProjectMember"] = new List<(string, string)>
                {
                    ("timelog", "create"),
                    ("timelog", "view")
                }
            };

            foreach (var roleClaim in roleClaims)
            {
                foreach (var claim in roleClaim.Value)
                {
                    await AddClaimToRoleAsync(roleManager, roleClaim.Key, claim.resource, claim.action);
                }
            }
        }

        public async Task AddClaimToRoleAsync(RoleManager<Role> roleManager, string roleName, string resource, string action)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var claim = new Claim(resource, action);
                var claims = await roleManager.GetClaimsAsync(role);
                if (!claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                {
                    await roleManager.AddClaimAsync(role, claim);
                }
            }
        }
    }
}
