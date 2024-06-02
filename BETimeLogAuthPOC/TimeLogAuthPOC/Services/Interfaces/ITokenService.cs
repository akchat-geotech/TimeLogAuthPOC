using TimeLogAuthPOC.Entity;

namespace TimeLogAuthPOC.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateToken(User user, string projectId);
        Task<string> GenerateToken(User user);
    }
}
