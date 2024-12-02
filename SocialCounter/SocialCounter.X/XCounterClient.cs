using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace SocialCounter.X;

public sealed partial class XCounterClient : SocialMediaClient
{
    private readonly ILogger<XCounterClient> logger;

    [GeneratedRegex(@"(?<=@\w+ Twitter profile\. )([\d,.KMB]+) Followers")]
    private static partial Regex MetaTagRegex();

    public XCounterClient(HttpClient httpClient, ILogger<XCounterClient> logger)
        : base(httpClient, logger)
    {
        this.logger = logger;
    }

    public override string Platform => "X";

    public override async Task<SocialCountResult> GetCount(
        string handle,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await this.GetAsync($"/{handle}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var followers = ExtractFollowerCount(content, handle);

            return new SocialCountResult(this.Platform, handle, followers);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to get LinkedIn follower count for {Handle}", handle);

            return SocialCountResult.Empty(this.Platform, handle);
        }
    }

    private static int ExtractFollowerCount(string content, string handle)
    {
        var match = MetaTagRegex().Match(content);
        if (!match.Success || match.Groups.Count < 2)
        {
            throw new FormatException($"Could not find follower count in meta tag for {handle}");
        }

        var followerText = match.Groups[1].Value.Trim();
        return ParseFollowerCount(followerText, handle);
    }

    private static int ParseFollowerCount(string followerText, string handle)
    {
        try
        {
            // Remove commas for regular numbers
            followerText = followerText.Replace(",", "");

            // Handle decimal numbers with suffixes (e.g., 2.5K)
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

            // Plain number
            return int.Parse(followerText);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException)
        {
            throw new FormatException(
                $"Failed to parse follower count '{followerText}' for {handle}",
                ex
            );
        }
    }
}
