using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace SocialCounter.Facebook;

public sealed partial class FacebookCounterClient : SocialMediaClient
{
    // Updated pattern to match the new format
    [GeneratedRegex(@"""text"":""([\d,.KMB]+) followers""")]
    private static partial Regex FollowerCountRegex();

    private readonly ILogger<FacebookCounterClient> logger;

    public FacebookCounterClient(HttpClient httpClient, ILogger<FacebookCounterClient> logger)
        : base(httpClient, logger)
    {
        this.logger = logger;
    }

    public override string Platform => "Facebook";

    public override async Task<SocialCountResult> GetCount(
        string handle,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await GetAsync($"/{handle}/followers", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogDebug("Received content for Facebook page {Handle}", handle);
            var followers = ExtractFollowerCount(content, handle);
            return new SocialCountResult(Platform, handle, followers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get Facebook follower count for {Handle}", handle);
            return SocialCountResult.Empty(Platform, handle);
        }
    }

    private static int ExtractFollowerCount(string content, string handle)
    {
        var match = FollowerCountRegex().Match(content);
        if (!match.Success || match.Groups.Count < 2)
        {
            throw new FormatException($"Could not find follower count for Facebook page {handle}");
        }
        var followerText = match.Groups[1].Value.Trim();
        return ParseFollowerCount(followerText, handle);
    }

    private static int ParseFollowerCount(string followerText, string handle)
    {
        try
        {
            // Remove commas and spaces
            followerText = followerText.Replace(",", "").Replace(" ", "");
            // Handle suffixes
            if (followerText.EndsWith("K", StringComparison.OrdinalIgnoreCase))
            {
                var number = double.Parse(followerText[..^1]);
                return (int)(number * 1_000);
            }
            if (followerText.EndsWith("M", StringComparison.OrdinalIgnoreCase))
            {
                var number = double.Parse(followerText[..^1]);
                return (int)(number * 1_000_000);
            }
            if (followerText.EndsWith("B", StringComparison.OrdinalIgnoreCase))
            {
                var number = double.Parse(followerText[..^1]);
                return (int)(number * 1_000_000_000);
            }
            return int.Parse(followerText);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException)
        {
            throw new FormatException(
                $"Failed to parse Facebook follower count '{followerText}' for {handle}",
                ex
            );
        }
    }
}
