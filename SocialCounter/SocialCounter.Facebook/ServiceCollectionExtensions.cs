using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using static SocialCounter.ServiceCollectionExtensions;

namespace SocialCounter.Facebook;

public static class ServiceCollectionExtensions
{
    public static SocialCounterBuilder AddFacebookCounter(
        this SocialCounterBuilder builder,
        Action<FacebookCounterOptions>? configureOptions = null
    )
    {
        var services = builder.Services;
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        services
            .AddHttpClient<FacebookCounterClient>()
            .ConfigureHttpClient(
                (sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<FacebookCounterOptions>>().Value;
                    client.BaseAddress = new Uri(options.BaseAddress);
                    client.Timeout = options.Timeout;
                    client.DefaultRequestVersion = new Version(2, 0);

                    // Clear any existing headers
                    client.DefaultRequestHeaders.Clear();

                    // Add modern browser headers
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Accept",
                        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Accept-Language",
                        "en-US,en;q=0.9"
                    );
                    //client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br, zstd");

                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Cache-Control",
                        "no-cache"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Pragma", "no-cache");

                    // Security and fetch metadata
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Sec-Ch-Ua",
                        "\"Microsoft Edge\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\""
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Ch-Ua-Mobile", "?0");
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Sec-Ch-Ua-Platform",
                        "\"Windows\""
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Sec-Ch-Ua-Platform-Version",
                        "\"19.0.0\""
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Sec-Ch-Prefers-Color-Scheme",
                        "dark"
                    );
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

                    // Modern Windows Edge User-Agent
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0"
                    );

                    // Additional headers
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Upgrade-Insecure-Requests",
                        "1"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Priority", "u=0, i");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Viewport-Width", "435");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Dpr", "1.5");

                    // Set host explicitly
                    client.DefaultRequestHeaders.Host = "www.facebook.com";
                }
            )
            .ConfigurePrimaryHttpMessageHandler(
                () =>
                    new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.None,
                        AllowAutoRedirect = true,
                        MaxAutomaticRedirections = 5,
                        UseCookies = true,
                        CookieContainer = new CookieContainer()
                    }
            );

        services.AddTransient<SocialMediaClient>(sp =>
            sp.GetRequiredService<FacebookCounterClient>()
        );

        return builder;
    }
}
