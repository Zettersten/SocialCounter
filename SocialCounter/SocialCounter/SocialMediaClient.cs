using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Net;

namespace SocialCounter;

public abstract class SocialMediaClient
{
    private readonly ILogger logger;
    private readonly HttpClient httpClient;
    private readonly ResiliencePipeline<HttpResponseMessage> resiliencePipeline;

    protected SocialMediaClient(HttpClient httpClient, ILogger logger)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.resiliencePipeline = BuildResiliencePipeline();
    }

    public abstract string Platform { get; }

    // Protected abstract method that implementations must provide
    public abstract Task<SocialCountResult> GetCount(
        string handle,
        CancellationToken cancellationToken
    );

    // Protected method for HTTP operations with retry
    private async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> action,
        CancellationToken cancellationToken = default
    )
    {
        return await this.resiliencePipeline.ExecuteAsync(
            async token => await action(token),
            cancellationToken
        );
    }

    // Simple helper methods for HTTP operations
    protected Task<HttpResponseMessage> GetAsync(
        string requestUri,
        CancellationToken cancellationToken = default
    )
    {
        return this.ExecuteWithRetryAsync(
            token => httpClient.GetAsync(requestUri, token),
            cancellationToken
        );
    }

    protected Task<HttpResponseMessage> PostAsync(
        string requestUri,
        HttpContent content,
        CancellationToken cancellationToken = default
    )
    {
        return this.ExecuteWithRetryAsync(
            token => httpClient.PostAsync(requestUri, content, token),
            cancellationToken
        );
    }

    private ResiliencePipeline<HttpResponseMessage> BuildResiliencePipeline()
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(
                new RetryStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<TaskCanceledException>()
                        .HandleResult(response =>
                            response.StatusCode
                                is HttpStatusCode.RequestTimeout
                                    or HttpStatusCode.BadGateway
                                    or HttpStatusCode.ServiceUnavailable
                                    or HttpStatusCode.GatewayTimeout
                                    or HttpStatusCode.TooManyRequests
                        ),

                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,

                    OnRetry = arguments =>
                    {
                        var attempt = arguments.AttemptNumber;
                        var delay = arguments.RetryDelay;

                        if (arguments.Outcome.Result != null)
                        {
                            logger.LogWarning(
                                "Request to {PlatformName} failed with status code {StatusCode}. Attempt {Attempt} of 3. Waiting {Delay}ms before next retry.",
                                Platform,
                                arguments.Outcome.Result.StatusCode,
                                attempt,
                                delay.TotalMilliseconds
                            );
                        }
                        else
                        {
                            logger.LogWarning(
                                arguments.Outcome.Exception,
                                "Request to {PlatformName} failed with exception. Attempt {Attempt} of 3. Waiting {Delay}ms before next retry.",
                                Platform,
                                attempt,
                                delay.TotalMilliseconds
                            );
                        }

                        return ValueTask.CompletedTask;
                    }
                }
            )
            .AddTimeout(TimeSpan.FromSeconds(10))
            .Build();
    }
}
