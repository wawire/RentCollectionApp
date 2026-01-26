namespace RentCollection.Application.Helpers;

public static class PasswordPolicy
{
    public const int MinimumLength = 12;

    public static bool IsValid(string password, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(password))
        {
            error = "Password is required.";
            return false;
        }

        if (password.Length < MinimumLength)
        {
            error = $"Password must be at least {MinimumLength} characters.";
            return false;
        }

        return true;
    }
}
