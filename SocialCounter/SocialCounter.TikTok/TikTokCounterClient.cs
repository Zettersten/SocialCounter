using Microsoft.Extensions.Logging;

namespace SocialCounter.TikTok;

public sealed partial class TikTokCounterClient : SocialMediaClient
{
    private readonly ILogger<TikTokCounterClient> logger;

    public TikTokCounterClient(HttpClient httpClient, ILogger<TikTokCounterClient> logger)
        : base(httpClient, logger)
    {
        this.logger = logger;
    }

    public override string Platform => "TikTok";

    public override async Task<SocialCountResult> GetCount(
        string handle,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await GetAsync($"/@{handle}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            logger.LogDebug("Received content for {Handle}, parsing follower count", handle);

            var followers = ExtractFollowerCount(content, handle);

            return new SocialCountResult(Platform, handle, followers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get TikTok follower count for {Handle}", handle);

            return SocialCountResult.Empty(Platform, handle);
        }
    }

    private static int ExtractFollowerCount(string content, string handle)
    {
        try
        {
            // Look for the line containing "stats":{"followerCount":
            var statsStartIndex = content.IndexOf(
                "\"stats\":{\"followerCount\":",
                StringComparison.Ordinal
            );
            if (statsStartIndex == -1)
            {
                throw new FormatException(
                    $"Could not find 'stats' section for TikTok handle {handle}"
                );
            }

            // Find the start of the followerCount value
            statsStartIndex += "\"stats\":{\"followerCount\":".Length;

            // Find the end of the followerCount value (ends at the next comma)
            int statsEndIndex = content.IndexOf(',', statsStartIndex);
            if (statsEndIndex == -1)
            {
                throw new FormatException(
                    $"Could not parse 'followerCount' value for TikTok handle {handle}"
                );
            }

            // Extract the follower count as a substring
            var followerCountText = content[statsStartIndex..statsEndIndex];

            if (!int.TryParse(followerCountText, out var followerCount))
            {
                throw new FormatException(
                    $"Invalid follower count value '{followerCountText}' for TikTok handle {handle}"
                );
            }

            return followerCount;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Failed to extract TikTok follower count for {handle}", ex);
        }
    }
}
