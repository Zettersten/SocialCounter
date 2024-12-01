namespace SocialCounter;

public sealed record SocialCountResult(string Platform, string Handle, int Count)
{
    public static SocialCountResult Empty(string platform, string handle) =>
        new(platform, handle, 0);
}
