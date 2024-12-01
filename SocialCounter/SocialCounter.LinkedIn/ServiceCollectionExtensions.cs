using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using static SocialCounter.ServiceCollectionExtensions;

namespace SocialCounter.LinkedIn;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLinkedInCounter(
        this SocialCounterBuilder builder,
        Action<LinkedInCounterOptions>? configureOptions = null
    )
    {
        var services = builder.Services;

        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        services
            .AddHttpClient<LinkedInCounterClient>()
            .ConfigureHttpClient(
                (sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<LinkedInCounterOptions>>().Value;
                    client.BaseAddress = new Uri(options.BaseAddress);
                    client.Timeout = options.Timeout;

                    // Clear any existing headers
                    client.DefaultRequestHeaders.Clear();

                    // Add headers that mimic a mobile browser (better for Facebook)
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Accept",
                        "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Accept-Language",
                        "en-US,en;q=0.9"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Accept-Encoding",
                        "gzip, deflate, br"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Cache-Control",
                        "no-cache"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Pragma", "no-cache");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("DNT", "1");

                    // Mobile User-Agent tends to work better with Facebook
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "User-Agent",
                        "Mozilla/5.0 (iPhone; CPU iPhone OS 16_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Mobile/15E148 Safari/604.1"
                    );

                    // Add basic Facebook-specific headers
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Sec-Fetch-Dest",
                        "document"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Sec-Fetch-Mode",
                        "navigate"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "none");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-User", "?1");
                }
            )
            .ConfigurePrimaryHttpMessageHandler(
                () =>
                    new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.All,
                        AllowAutoRedirect = true,
                        MaxAutomaticRedirections = 5,
                        UseCookies = true,
                        CookieContainer = new CookieContainer()
                    }
            );

        services.AddTransient<SocialMediaClient>(sp =>
            sp.GetRequiredService<LinkedInCounterClient>()
        );

        return services;
    }
}
