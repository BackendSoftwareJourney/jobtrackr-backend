using System.ComponentModel.DataAnnotations;

namespace JobTrackr.Application.Common;

public static class EmailValidation
{
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    public static bool IsValid(string email)
    {
        return _emailAddressAttribute.IsValid(email);
    }

    public static string Normalize(string email)
    {
        return email.ToLowerInvariant();
    }
}