namespace XrmTools.Resiliency;
using Polly;
using Polly.Retry;
using System;
using System.Net.Http;
using System.Threading.Tasks;

// Based on the Microsoft.Extensions.Http.Resilience.HttpRetryStrategyOptions
/// <summary>
/// Implementation of the Polly.Retry.RetryStrategyOptions`1 for System.Net.Http.HttpResponseMessage
/// results.
/// </summary>
public class HttpRetryStrategyOptions : RetryStrategyOptions<HttpResponseMessage>
{
    private bool _shouldRetryAfterHeader;

    /// <summary>
    /// Gets or sets a value indicating whether to use the Retry-After header for the
    /// retry delays.
    /// </summary>
    /// <value>Defaults to true.</value>
    /// <remarks>
    /// If the property is set to true then the generator will resolve the delay based
    /// on the Retry-After header rules, otherwise it will return null and the retry
    /// strategy delay will generate the delay based on the configured options.
    /// </remarks>
    public bool ShouldRetryAfterHeader
    {
        get
        {
            return _shouldRetryAfterHeader;
        }
        set
        {
            _shouldRetryAfterHeader = value;
            if (_shouldRetryAfterHeader)
            {
                base.DelayGenerator = delegate (RetryDelayGeneratorArguments<HttpResponseMessage> args)
                {
                    HttpResponseMessage result = args.Outcome.Result;
                    if (result != null)
                    {
                        HttpResponseMessage response = result;
                        if (RetryAfterHelper.TryParse(response, TimeProvider.System, out var retryAfter))
                        {
                            return new ValueTask<TimeSpan?>(retryAfter);
                        }
                    }

                    return default(ValueTask<TimeSpan?>);
                };
            }
            else
            {
                base.DelayGenerator = null;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the Microsoft.Extensions.Http.Resilience.HttpRetryStrategyOptions
    /// class.
    /// </summary>
    /// <remarks>
    /// By default, the options are configured to handle only transient failures. Specifically,
    /// this includes HTTP status codes 408, 429, 500 and above, as well as System.Net.Http.HttpRequestException
    /// and Polly.Timeout.TimeoutRejectedException exceptions.
    public HttpRetryStrategyOptions()
    {
        base.ShouldHandle = (RetryPredicateArguments<HttpResponseMessage> args) => new ValueTask<bool>(HttpClientResiliencePredicates.IsTransient(args.Outcome, args.Context.CancellationToken));
        base.BackoffType = DelayBackoffType.Exponential;
        ShouldRetryAfterHeader = true;
        base.UseJitter = true;
    }
}
