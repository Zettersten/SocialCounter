using static SocialCounter.ServiceCollectionExtensions;

namespace SocialCounter.Facebook;

public sealed class FacebookCounterOptions : SocialCounterOptions
{
    public override string BaseAddress { get; set; } = "https://www.facebook.com";
}
