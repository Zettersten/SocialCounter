using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static SocialCounter.ServiceCollectionExtensions;

namespace SocialCounter;

public static class ServiceCollectionExtensions
{
    public sealed class SocialCounterBuilder(
        IServiceCollection services,
        IHttpClientBuilder httpClientBuilder
    )
    {
        public IServiceCollection Services { get; } = services;

        public IHttpClientBuilder HttpClientBuilder { get; } = httpClientBuilder;
    }

    public class SocialCounterOptions
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(1);
        public virtual string BaseAddress { get; set; } = string.Empty;

        public IDictionary<string, string> DefaultHeaders { get; set; } =
            new Dictionary<string, string>();
    }

    private const string DEFAULT_CLIENT_NAME = "SocialCounter.DefaultClient";

    public static SocialCounterBuilder AddSocialCounters(
        this IServiceCollection services,
        Action<SocialCounterOptions>? configureOptions = null
    )
    {
        // Register base options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        // Register default options
        services
            .AddOptions<SocialCounterOptions>()
            .Configure(options =>
            {
                options.Timeout = TimeSpan.FromSeconds(25);
                options.MaxRetryAttempts = 3;
                options.InitialRetryDelay = TimeSpan.FromSeconds(1);
            });

        // Register base HttpClient
        var httpClientBuilder = services
            .AddHttpClient(DEFAULT_CLIENT_NAME)
            .ConfigureHttpClient(
                (sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<SocialCounterOptions>>().Value;

                    if (!string.IsNullOrEmpty(options.BaseAddress))
                    {
                        client.BaseAddress = new Uri(options.BaseAddress);
                    }

                    client.Timeout = options.Timeout;
                    client.DefaultRequestVersion = new Version(2, 0);

                    foreach (var header in options.DefaultHeaders)
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation(
                            header.Key,
                            header.Value
                        );
                    }
                }
            );

        services.AddTransient<SocialCounters>();

        return new SocialCounterBuilder(services, httpClientBuilder);
    }

    public static SocialCounterBuilder ConfigureHttpClient(
        this SocialCounterBuilder builder,
        Action<IServiceProvider, HttpClient> configureClient
    )
    {
        builder.HttpClientBuilder.ConfigureHttpClient(configureClient);
        return builder;
    }
}
