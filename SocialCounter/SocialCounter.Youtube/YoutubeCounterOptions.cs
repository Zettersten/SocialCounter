using static SocialCounter.ServiceCollectionExtensions;

namespace SocialCounter.Youtube;

public class YoutubeCounterOptions : SocialCounterOptions
{
    public override string BaseAddress { get; set; } = "https://www.youtube.com";
}
