using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TimeLogAuthPOC.Attribute;
using TimeLogAuthPOC.Config;
using TimeLogAuthPOC.Data;
using TimeLogAuthPOC.Entity;
using TimeLogAuthPOC.Services;
using TimeLogAuthPOC.Services.Interfaces;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace TimeLogAuthPOC.Controllers
{
    [Route("api/[controller]")]
    [EnableCors]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly GoogleAuthConfig _googleAuthConfig;
        private readonly JWTConfig _jwtConfig;
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;

        public AuthController(IOptions<GoogleAuthConfig> googleAuthConfig, IOptions<JWTConfig> jwtConfig, UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext context, ITokenService tokenService)
        {
            _googleAuthConfig = googleAuthConfig.Value;
            _jwtConfig = jwtConfig.Value;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _tokenService = tokenService;
        }
        [HttpPost("signin-google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLoginAsync([FromBody] TokenRequest request)
        {
            string token = "";
            if (string.IsNullOrWhiteSpace(request.TokenId))
            {
                return BadRequest();
            }

            Payload payload;
            try
            {
                payload = await ValidateAsync(request.TokenId, new ValidationSettings
                {
                    Audience = [_googleAuthConfig.ClientId],
                });
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return Unauthorized();
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                // User not found, create a new user
                user = new User
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    ProfilePicture = payload.Picture,
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error creating user.");
                }

                await _userManager.AddToRoleAsync(user, "ProjectMember");
                token = await _tokenService.GenerateToken(user);
                return Ok(new { Token = token });
            }
            var lastAccessedProjectId = await GetLastAccessedProjectId(user);

            token = await _tokenService.GenerateToken(user, lastAccessedProjectId);
            return Ok(new { Token = token });
        }
        private async Task<string> GetLastAccessedProjectId(User user)
        {
            return await _context.Users
                         .Where(u => u.Id == user.Id)
                         .SelectMany(u => u.UserProjectRoles)
                         .Select(upr => upr.ProjectId)
                         .FirstOrDefaultAsync();
        }
        [HttpGet("auth-required")]
        [Authorize(Policy = "TimeLogCreatePolicy")]
        public async Task<IActionResult> AuthRequired()
        {
            return Ok("Authorized");
        }
    }
    public class TokenRequest
    {
        public string TokenId { get; set; }
    }
}
