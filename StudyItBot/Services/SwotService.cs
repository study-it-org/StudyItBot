using System.Collections.Immutable;
using System.Text;
using Nager.PublicSuffix;
using NLog;
using StudyItBot.Utilities;

namespace StudyItBot.Services;

public class SwotService
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private DateTime _expireTime = DateTime.UtcNow;
    private ImmutableHashSet<string>? _swotSet;

    /// <summary>
    /// Validates an E-Mail using Jetbrains Swot Github Repository.
    /// </summary>
    /// <param name="email">email to validate</param>
    /// <returns>true if valid</returns>
    public async Task<bool> VerifyUniversityEmailAsync(string email)
    {
        var emailParts = email.Split("@", 2);
        if (emailParts.Length != 2)
            return false;
        var parts = GetDomainParts(email);
        return !(await IsStopListedAsync(parts)) && CheckSet(await GetSwotListAsync(), parts);
    }

    public async Task<bool> IsStopListedAsync(List<string> parts)
    {
        return CheckSet(await GetStopListAsync(), parts);
    }

    public static bool CheckSet(IReadOnlySet<string> set, List<string> parts)
    {
        var subj = new StringBuilder();
        foreach (var part in parts)
        {
            subj.Insert(0, part);
            if (set.Contains(subj.ToString()))
                return true;
            subj.Insert(0, ".");
        }

        return false;
    }

    public static List<string> GetDomainParts(string emailOrDomain)
    {
        return emailOrDomain.Trim().ToLower().SubstringAfter('@').SubstringAfter("://").SubstringBefore(':').Split('.')
            .Reverse().ToList();
    }

    public async Task<ImmutableHashSet<string>> GetSwotListAsync()
    {
        if (_swotSet != null && _expireTime > DateTime.UtcNow)
        {
            return _swotSet;
        }

        using (var httpClient = new HttpClient())
        {
            try
            {
                var str = await httpClient.GetStringAsync(
                    "https://github.com/JetBrains/swot/releases/download/latest/swot.txt");
                _swotSet = str.Split("\n").ToImmutableHashSet();
                _expireTime = DateTime.UtcNow + TimeSpan.FromHours(4);
                _logger.Debug($"Successfully fetched SwotList. Length: {_swotSet.Count}");
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        return _swotSet ?? ImmutableHashSet<string>.Empty;
    }

    public async Task<ImmutableHashSet<string>> GetStopListAsync()
    {
        return (await GetSwotListAsync()).Where(s => s.StartsWith("-")).Select(s => s[1..]).ToImmutableHashSet();
    }
}