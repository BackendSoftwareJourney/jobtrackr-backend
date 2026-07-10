using JobTrackr.Domain.Entities;

namespace JobTrackr.Application.Auth
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}
