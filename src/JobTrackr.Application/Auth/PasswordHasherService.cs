using Microsoft.AspNetCore.Identity;

namespace JobTrackr.Application.Auth
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly PasswordHasher<object> _passwordHasher = new();

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(new object(), password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            var result = _passwordHasher.VerifyHashedPassword(new object(), passwordHash, password);

            return result == PasswordVerificationResult.Success;
        }
    }
}
