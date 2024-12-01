using static SocialCounter.ServiceCollectionExtensions;

namespace SocialCounter.TikTok;

public class TikTokCounterOptions : SocialCounterOptions
{
    public override string BaseAddress { get; set; } = "https://www.tiktok.com";
}
