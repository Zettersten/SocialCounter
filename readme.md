# SocialCounter 📊

[![NuGet version](https://badge.fury.io/nu/SocialCounter.svg)](https://badge.fury.io/nu/SocialCounter)

Welcome to **SocialCounter**, a high-performance .NET library designed to fetch and aggregate social media metrics across multiple platforms. Built with reliability and extensibility in mind, it provides a unified way to retrieve follower counts and other social metrics while handling rate limiting, retries, and failures gracefully.

## Overview

The **SocialCounter** library offers:

- **Unified Interface**: A single, consistent API to fetch social metrics across different platforms
- **Built-in Resilience**: Automatic retry policies with exponential backoff and jitter
- **Parallel Processing**: Concurrent requests to multiple social media platforms
- **Extensible Design**: Easy to add new social media platform implementations
- **High Performance**: Optimized for efficient HTTP operations with modern .NET features
- **Configurable Timeouts**: Customizable timeout settings per platform or globally

## Getting Started

### Installation

Install the SocialCounter package from NuGet:

```sh
dotnet add package SocialCounter
```

### Basic Setup

Configure SocialCounter in your ASP.NET Core application:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSocialCounters(options =>
    {
        options.Timeout = TimeSpan.FromSeconds(10);
        options.MaxRetryAttempts = 3;
        options.InitialRetryDelay = TimeSpan.FromSeconds(1);
    });

    // Register your platform-specific implementations
}
```

### Basic Usage

```csharp
public class SocialMetricsService
{
    private readonly SocialCounters _counters;

    public SocialMetricsService(SocialCounters counters)
    {
        _counters = counters;
    }

    public async Task<List<SocialCountResult>> GetMetricsAsync(string handle)
    {
        var results = await _counters.GetCountAsync(handle, CancellationToken.None);
        return results;
    }
}
```

## Features

### Resilient HTTP Operations

SocialCounter includes built-in resilience features:

- Automatic retries for transient failures
- Exponential backoff with jitter
- Configurable timeout policies
- Comprehensive error handling and logging

### Supported HTTP Status Code Retries

- 408 Request Timeout
- 502 Bad Gateway
- 503 Service Unavailable
- 504 Gateway Timeout
- 429 Too Many Requests

### Configuration Options

The `SocialCounterOptions` class provides various configuration settings:

| Option | Description | Default |
|--------|-------------|---------|
| `Timeout` | Maximum time for any single request | 10 seconds |
| `MaxRetryAttempts` | Maximum number of retry attempts | 3 |
| `InitialRetryDelay` | Initial delay between retries | 1 second |
| `BaseAddress` | Base URL for HTTP requests | Empty |
| `DefaultHeaders` | Default headers for all requests | Empty Dictionary |

## Advanced Usage

### Custom Configuration

```csharp
services.AddSocialCounters(options =>
{
    options.Timeout = TimeSpan.FromSeconds(15);
    options.DefaultHeaders.Add("User-Agent", "MyApp/1.0");
    options.BaseAddress = "https://api.example.com";
})
.ConfigureHttpClient((provider, client) =>
{
    client.DefaultRequestHeaders.Add("Authorization", "Bearer token");
});
```

### Implementing a Custom Platform Client

```csharp
public class TwitterClient : SocialMediaClient
{
    public TwitterClient(HttpClient httpClient, ILogger<TwitterClient> logger) 
        : base(httpClient, logger)
    {
    }

    public override string Platform => "Twitter";

    public override async Task<SocialCountResult> GetCount(
        string handle, 
        CancellationToken cancellationToken)
    {
        var response = await GetAsync($"/users/{handle}/metrics", cancellationToken);
        // Process response and return count
        return new SocialCountResult(Platform, handle, followersCount);
    }
}
```

## Error Handling

SocialCounter provides comprehensive error handling:

- Failed requests are logged with detailed information
- Exceptions don't break the entire operation
- Each platform is processed independently
- Empty results are returned for failed platforms

## Performance Considerations

- Parallel processing of multiple social media platforms
- Efficient HTTP client usage with connection pooling
- Optimized retry policies to prevent unnecessary delays
- Configurable timeouts to prevent long-running requests

## Requirements

- .NET 8.0 or higher
- ASP.NET Core for dependency injection
- HTTP client support

## Best Practices

1. Configure appropriate timeouts for your use case
2. Implement platform-specific error handling
3. Use cancellation tokens for long-running operations
4. Monitor and log failed requests
5. Handle rate limiting appropriately

## License

This library is available under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues and feature requests, please open an issue in the GitHub repository.

---

Thank you for using **SocialCounter**. We look forward to seeing how you use it in your projects!
