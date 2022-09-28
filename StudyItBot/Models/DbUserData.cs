namespace StudyItBot.Models;

// ReSharper disable once InconsistentNaming
public record DbUserData
{
    public static string GetKey(ulong id) => $"user:{id}";
    public string VerificationToken { get; set; } = Guid.NewGuid().ToString();
    public DateTime VerificationTokenExpireTime { get; set; } = DateTime.UtcNow;
    public string? Firstname { get; set; } = null;
    public bool Verified { get; set; } = false;
}