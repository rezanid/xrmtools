namespace XrmTools.Resiliency;

using Polly.CircuitBreaker;
using System.Net.Http;
using System.Threading.Tasks;

// Based on Microsoft.Extensions.Http.Resilience.HttpCircuitBreakerStrategyOptions     
/// <summary>
/// Implementation of the Polly.CircuitBreaker.CircuitBreakerStrategyOptions`1 for
/// System.Net.Http.HttpResponseMessage results.
/// </summary>
public class HttpCircuitBreakerStrategyOptions : CircuitBreakerStrategyOptions<HttpResponseMessage>
{
    /// <summary>
    /// Initializes a new instance of the Microsoft.Extensions.Http.Resilience.HttpCircuitBreakerStrategyOptions
    /// class.
    /// </summary>
    /// <remarks>
    /// By default the options is set to handle only transient failures, that is, timeouts,
    /// 5xx responses, and System.Net.Http.HttpRequestException exceptions.
    /// </remarks>
    public HttpCircuitBreakerStrategyOptions() 
        => ShouldHandle = static args => new ValueTask<bool>(HttpClientResiliencePredicates.IsTransient(args.Outcome));
}