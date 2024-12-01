using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace SocialCounter.LinkedIn;

public sealed partial class LinkedInCounterClient : SocialMediaClient
{
    // This pattern looks for LinkedIn's follower count format like "2.2K+ followers"
    [GeneratedRegex(@"(\d+(?:\.\d+)?K?\+?)\s*followers?")]
    private static partial Regex FollowerCountRegex();

    private readonly ILogger<LinkedInCounterClient> logger;

    public LinkedInCounterClient(HttpClient httpClient, ILogger<LinkedInCounterClient> logger)
        : base(httpClient, logger)
    {
        this.logger = logger;
    }

    public override string Platform => "LinkedIn";

    public override async Task<SocialCountResult> GetCount(
        string handle,
        CancellationToken cancellationToken
    )
    {
        try
        {
            // LinkedIn profiles are accessed by their handle
            var response = await GetAsync(
                $"/search?q=\"https%3A%2F%2Fwww.linkedin.com%2Fin%2F{handle}\"",
                cancellationToken
            );
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogDebug("Received content for LinkedIn profile {Handle}", handle);
            var followers = ExtractFollowerCount(content, handle);
            return new SocialCountResult(Platform, handle, followers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get LinkedIn follower count for {Handle}", handle);

            return SocialCountResult.Empty(Platform, handle);
        }
    }

    private static int ExtractFollowerCount(string content, string handle)
    {
        var match = FollowerCountRegex().Match(content);
        if (!match.Success || match.Groups.Count < 2)
        {
            throw new FormatException(
                $"Could not find follower count for LinkedIn profile {handle}"
            );
        }

        var followerText = match.Groups[1].Value.Trim();
        return ParseFollowerCount(followerText, handle);
    }

    private static int ParseFollowerCount(string followerText, string handle)
    {
        try
        {
            // Remove any plus signs that indicate "more than"
            followerText = followerText.Replace("+", "");

            // Handle K suffix (thousands)
            if (followerText.EndsWith("K", StringComparison.OrdinalIgnoreCase))
            {
                var number = double.Parse(followerText[..^1]);
                // Round up since LinkedIn uses + to indicate "more than"
                return (int)Math.Ceiling(number * 1_000);
            }

            // If no suffix, parse directly
            return (int)Math.Ceiling(double.Parse(followerText));
        }
        catch (Exception ex) when (ex is FormatException or OverflowException)
        {
            throw new FormatException(
                $"Failed to parse LinkedIn follower count '{followerText}' for {handle}",
                ex
            );
        }
    }
}
