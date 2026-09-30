namespace JobTrackr.Application.Common;

public static class PasswordValidation
{
    public const int MinimumLength = 8;

    public static bool HasMinimumLength(string password)
    {
        return password.Length >= MinimumLength;
    }

    public static bool HasBasicComplexity(string password)
    {
        return password.Any(char.IsUpper) &&
            password.Any(char.IsLower) &&
            password.Any(char.IsDigit);
    }
}