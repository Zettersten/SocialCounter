using Microsoft.Extensions.Hosting;
using SocialCounter.Facebook;
using SocialCounter.Instagram;
using SocialCounter.LinkedIn;
using SocialCounter.TikTok;
using SocialCounter.X;
using SocialCounter.Youtube;

namespace SocialCounter.Tests;

internal class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(services =>
        {
            services
                .AddSocialCounters()
                .AddYoutubeCounter()
                .AddInstagramCounter()
                .AddXCounter()
                .AddTikTokCounter()
                .AddFacebookCounter()
                .AddLinkedInCounter();
        });
    }
}
