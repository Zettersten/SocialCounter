using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using static SocialCounter.ServiceCollectionExtensions;

namespace SocialCounter.TikTok;

public static class ServiceCollectionExtensions
{
    public static SocialCounterBuilder AddTikTokCounter(
        this SocialCounterBuilder builder,
        Action<TikTokCounterClient>? configureOptions = null
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
            .AddHttpClient<TikTokCounterClient>()
            .ConfigureHttpClient(
                (sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<TikTokCounterOptions>>().Value;

                    client.BaseAddress = new Uri(options.BaseAddress);

                    // Clear any existing headers
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestVersion = new Version(2, 0);
                    client.Timeout = options.Timeout;

                    // Add headers one by one with proper formatting
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Accept",
                        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8"
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
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Connection",
                        "keep-alive"
                    );
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
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0"
                    );

                    // Set host explicitly
                    client.DefaultRequestHeaders.Host = "www.tiktok.com";
                }
            )
            .ConfigurePrimaryHttpMessageHandler(
                () =>
                    new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.All,
                        AllowAutoRedirect = false,
                        MaxAutomaticRedirections = 1,
                        UseCookies = true,
                        CookieContainer = new CookieContainer(),
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) =>
                            true // Be careful with this in production
                    }
            );

        // Register as SocialMediaClient for collection injection
        services.AddTransient<SocialMediaClient>(sp =>
            sp.GetRequiredService<TikTokCounterClient>()
        );

        return builder;
    }
}
