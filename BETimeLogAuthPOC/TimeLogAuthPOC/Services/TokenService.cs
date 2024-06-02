using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TimeLogAuthPOC.Config;
using TimeLogAuthPOC.Data;
using TimeLogAuthPOC.Entity;
using TimeLogAuthPOC.Services.Interfaces;

namespace TimeLogAuthPOC.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly JWTConfig _jwtConfig;

        public TokenService(UserManager<User> userManager, RoleManager<Role> roleManager, IOptions<JWTConfig> jwtConfig, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtConfig = jwtConfig.Value;
            _context = context;
        }

        public async Task<string> GenerateToken(User user, string projectId)
        {
            var userProjectRoles = await _context.UserProjectRole
                .Include(upr => upr.Role)
                .Where(upr => upr.UserId == user.Id && upr.ProjectId == projectId)
                .ToListAsync();

            if (!userProjectRoles.Any())
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            foreach (var userProjectRole in userProjectRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userProjectRole.Role.Name));
                var roleClaims = await _roleManager.GetClaimsAsync(userProjectRole.Role);
                claims.AddRange(roleClaims);
            }

            return GenerateTokenFromClaims(claims);
        }

        public async Task<string> GenerateToken(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                var applicationRole = await _roleManager.FindByNameAsync(role);
                if (applicationRole != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(applicationRole);
                    claims.AddRange(roleClaims);
                }
            }

            return GenerateTokenFromClaims(claims);
        }

        private string GenerateTokenFromClaims(IEnumerable<Claim> claims)
        {
            var secret = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtConfig.Secret));
            var creds = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(Convert.ToInt32(_jwtConfig.ExpirationInDays)),
                signingCredentials: creds));
            return token;
        }
    }
}
