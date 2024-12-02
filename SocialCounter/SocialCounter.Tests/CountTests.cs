using SocialCounter.Facebook;
using SocialCounter.Instagram;
using SocialCounter.LinkedIn;
using SocialCounter.TikTok;
using SocialCounter.X;
using SocialCounter.Youtube;

namespace SocialCounter.Tests;

public class CountTests
{
    private readonly SocialCounters socialCounters;
    private readonly InstagramCounterClient instagramCounterClient;
    private readonly XCounterClient xCounterClient;
    private readonly TikTokCounterClient tikTokCounterClient;
    private readonly FacebookCounterClient facebookCounterClient;
    private readonly LinkedInCounterClient linkedInCounter;
    private readonly YoutubeCounterClient youtubeCounterClient;

    public CountTests(
        SocialCounters socialCounters,
        InstagramCounterClient instagramCounterClient,
        XCounterClient xCounterClient,
        TikTokCounterClient tikTokCounterClient,
        FacebookCounterClient facebookCounterClient,
        LinkedInCounterClient linkedInCounter,
        YoutubeCounterClient youtubeCounterClient
    )
    {
        this.socialCounters = socialCounters;
        this.instagramCounterClient = instagramCounterClient;
        this.xCounterClient = xCounterClient;
        this.tikTokCounterClient = tikTokCounterClient;
        this.facebookCounterClient = facebookCounterClient;
        this.linkedInCounter = linkedInCounter;
        this.youtubeCounterClient = youtubeCounterClient;
    }

    [Fact]
    public async Task Should_Return_Instagram_Count()
    {
        var count = await this.instagramCounterClient.GetCount("zettersten", CancellationToken.None);

        Assert.NotNull(count);
        Assert.True(count.Count > 0);
    }

    [Fact]
    public async Task Should_Return_Youtube_Count()
    {
        var count = await this.youtubeCounterClient.GetCount("erikzettersten", CancellationToken.None);

        Assert.NotNull(count);
        Assert.True(count.Count > 0);
    }

    [Fact]
    public async Task Should_Return_X_Count()
    {
        var count = await this.xCounterClient.GetCount("zettersten", CancellationToken.None);

        Assert.NotNull(count);
        Assert.True(count.Count > 0);
    }

    [Fact]
    public async Task Should_Return_TikTok_Count()
    {
        var count = await this.tikTokCounterClient.GetCount("garyvee", CancellationToken.None);

        Assert.NotNull(count);
        Assert.True(count.Count > 0);
    }

    [Fact]
    public async Task Should_Return_Facebook_Count()
    {
        var count = await this.facebookCounterClient.GetCount(
            "VeeFriendsOfficial",
            CancellationToken.None
        );

        Assert.NotNull(count);
        Assert.True(count.Count > 0);
    }

    [Fact]
    public async Task Should_Return_LinkedIn_Count()
    {
        var count = await this.linkedInCounter.GetCount("zettersten", CancellationToken.None);

        Assert.NotNull(count);
        Assert.True(count.Count > 0);
    }

    [Fact]
    public async Task Should_Return_All_Counts()
    {
        var counts = await this.socialCounters.GetCountAsync("zettersten", CancellationToken.None);

        Assert.NotNull(counts);
        Assert.NotEmpty(counts);

        Assert.All(
            counts,
            count =>
            {
                Assert.NotNull(count);

                if (count.Platform != "TikTok")
                {
                    Assert.True(count.Count > 0);
                }
            }
        );
    }
}
