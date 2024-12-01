using static SocialCounter.ServiceCollectionExtensions;

namespace SocialCounter.X;

public class XCounterOptions : SocialCounterOptions
{
    public override string BaseAddress { get; set; } = "https://twstalker.com/";
}
