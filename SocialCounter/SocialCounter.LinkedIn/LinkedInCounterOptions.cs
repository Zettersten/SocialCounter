using static SocialCounter.ServiceCollectionExtensions;

namespace SocialCounter.LinkedIn;

public sealed class LinkedInCounterOptions : SocialCounterOptions
{
    public override string BaseAddress { get; set; } = "https://www.google.com/";
}
