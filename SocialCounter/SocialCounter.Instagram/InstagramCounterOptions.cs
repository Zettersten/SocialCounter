using static SocialCounter.ServiceCollectionExtensions;

namespace SocialCounter.Instagram;

public class InstagramCounterOptions : SocialCounterOptions
{
    public override string BaseAddress { get; set; } = "https://www.instagram.com";
}
