using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static SocialCounter.ServiceCollectionExtensions;

namespace SocialCounter.Instagram;

public static class ServiceCollectionExtensions
{
    public static SocialCounterBuilder AddInstagramCounter(
        this SocialCounterBuilder builder,
        Action<InstagramCounterOptions>? configureOptions = null
    )
    {
        var services = builder.Services;

        // Register Instagram-specific options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        // Register Instagram client with its own HttpClient
        services
            .AddHttpClient<InstagramCounterClient>()
            .ConfigureHttpClient(
                (sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<InstagramCounterOptions>>().Value;
                    client.BaseAddress = new Uri(options.BaseAddress);

                    // Common browser headers
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Accept",
                        "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Accept-Language",
                        "en-US,en;q=0.5"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Accept-Encoding",
                        "gzip, deflate, br"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Connection",
                        "keep-alive"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Upgrade-Insecure-Requests",
                        "1"
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
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:123.0) Gecko/20100101 Firefox/123.0"
                    );
                    client.DefaultRequestHeaders.TryAddWithoutValidation("DNT", "1");
                }
            )
            .ConfigurePrimaryHttpMessageHandler(
                () =>
                    new SocketsHttpHandler
                    {
                        AutomaticDecompression =
                            DecompressionMethods.GZip
                            | DecompressionMethods.Deflate
                            | DecompressionMethods.Brotli,
                        UseCookies = true,
                        CookieContainer = new CookieContainer(),
                        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
                    }
            );

        // Register as SocialMediaClient for collection injection
        services.AddTransient<SocialMediaClient>(sp =>
            sp.GetRequiredService<InstagramCounterClient>()
        );

        return builder;
    }
}
