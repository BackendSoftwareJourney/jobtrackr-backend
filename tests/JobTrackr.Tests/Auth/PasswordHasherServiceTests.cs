using JobTrackr.Application.Auth;

namespace JobTrackr.Tests.Auth
{
    public class PasswordHasherServiceTests
    {
        [Fact]
        public void HashAndVerifyPassword_WithCorrectPassword_ReturnsTrue()
        {
            var passwordHasher = new PasswordHasherService();
            const string password = "Password123";

            var passwordHash = passwordHasher.HashPassword(password);
            var isValid = passwordHasher.VerifyPassword(password, passwordHash);

            Assert.NotEqual(password, passwordHash);
            Assert.True(isValid);
        }
    }
}