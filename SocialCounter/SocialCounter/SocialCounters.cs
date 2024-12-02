using Microsoft.Extensions.Logging;

namespace SocialCounter;

public sealed class SocialCounters
{
    private readonly IEnumerable<SocialMediaClient> counters;
    private readonly ILogger<SocialCounters> logger;

    public SocialCounters(IEnumerable<SocialMediaClient> counters, ILogger<SocialCounters> logger)
    {
        this.counters = counters;
        this.logger = logger;
    }

    public async Task<List<SocialCountResult>> GetCountAsync(
        string handle,
        CancellationToken cancellationToken
    )
    {
        var results = new List<SocialCountResult>();

        await Task.WhenAll(
            this.counters.Select(async counter =>
            {
                try
                {
                    var result = await counter.GetCount(handle, cancellationToken);

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    // Log and continue
                    this.logger.LogError(
                        ex,
                        "Failed to get count for {Platform} handle {Handle}",
                        counter.Platform,
                        handle
                    );
                }
            })
        );

        return results;
    }
}
