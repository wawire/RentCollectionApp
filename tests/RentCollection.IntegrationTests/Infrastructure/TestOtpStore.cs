using System.Collections.Concurrent;

namespace RentCollection.IntegrationTests.Infrastructure;

public class TestOtpStore
{
    private readonly ConcurrentDictionary<string, string> _codes = new();

    public void SetEmailCode(string email, string code)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        _codes[$"email:{email.ToLowerInvariant()}"] = code;
    }

    public void SetPhoneCode(string phone, string code)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return;
        }

        _codes[$"phone:{phone}"] = code;
    }

    public string? GetEmailCode(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return _codes.TryGetValue($"email:{email.ToLowerInvariant()}", out var code) ? code : null;
    }

    public string? GetPhoneCode(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        return _codes.TryGetValue($"phone:{phone}", out var code) ? code : null;
    }
}
