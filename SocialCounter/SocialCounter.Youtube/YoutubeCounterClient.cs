using Microsoft.Extensions.Logging;

namespace SocialCounter.Youtube;

public sealed partial class YoutubeCounterClient : SocialMediaClient
{
    private readonly ILogger<YoutubeCounterClient> logger;

    public YoutubeCounterClient(HttpClient httpClient, ILogger<YoutubeCounterClient> logger)
        : base(httpClient, logger)
    {
        this.logger = logger;
    }

    public override string Platform => "Youtube";

    public override async Task<SocialCountResult> GetCount(
        string handle,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await this.GetAsync($"/@{handle.StripAtSignFromHandle()}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            this.logger.LogDebug("Received content for {Handle}, parsing follower count", handle);

            var followers = ExtractFollowerCount(content, handle);

            return new SocialCountResult(this.Platform, handle, followers);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to get TikTok follower count for {Handle}", handle);

            return SocialCountResult.Empty(this.Platform, handle);
        }
    }

    private static int ExtractFollowerCount(string content, string handle)
    {
        try
        {
            var subscriberText = content.Split("subscribers\"}")[0];
            subscriberText = subscriberText.Split("content\":\"")[^1].Trim();

            // Remove any commas in numbers like "1,234"
            subscriberText = subscriberText.Replace(",", "");

            var multiplier = 1;
            if (subscriberText.EndsWith('K'))
            {
                multiplier = 1000;
                subscriberText = subscriberText.TrimEnd('K');
            }
            else if (subscriberText.EndsWith('M'))
            {
                multiplier = 1000000;
                subscriberText = subscriberText.TrimEnd('M');
            }

            if (decimal.TryParse(subscriberText, out var number))
            {
                return (int)(number * multiplier);
            }

            throw new FormatException(
                $"Invalid subscriber count value '{subscriberText}' for YouTube handle {handle}"
            );
        }
        catch (Exception ex)
        {
            throw new FormatException($"Failed to extract YouTube subscriber count for {handle}", ex);
        }
    }
}
