namespace XrmTools.Resiliency;
using Polly;
using Polly.Timeout;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;

// Based on Microsoft.Extensions.Http.Resilience
/// <summary>
/// Provides static predicates used within the current package.
/// </summary>
public static class HttpClientResiliencePredicates
{
    private const int InternalServerErrorCode = 500;

    private const int TooManyRequests = 429;

    /// <summary>
    /// Determines whether an outcome should be treated by resilience strategies as a
    /// transient failure.
    /// </summary>
    /// <returns>true if outcome is transient, false if not.</returns>
    public static bool IsTransient(Outcome<HttpResponseMessage> outcome)
    {
        Outcome<HttpResponseMessage> outcome2 = outcome;
        HttpResponseMessage result = outcome2.Result;
        if (result == null || !IsTransientHttpFailure(result))
        {
            Exception exception = outcome2.Exception;
            if (exception != null && IsTransientHttpException(exception))
            {
                return true;
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether an System.Net.Http.HttpResponseMessage should be treated by
    /// resilience strategies as a transient failure.
    /// </summary>
    /// <param name="outcome">The outcome of the user-specified callback.</param>
    /// <param name="cancellationToken">The System.Threading.CancellationToken associated with the execution.</param>
    /// <returns>true if outcome is transient, false if not.</returns>
    //[System.Diagnostics.CodeAnalysis.Experimental("EXTEXP0001", UrlFormat = "https://aka.ms/dotnet-extensions-warnings/{0}")]
    public static bool IsTransient(Outcome<HttpResponseMessage> outcome, CancellationToken cancellationToken)
    {
        if (!IsHttpConnectionTimeout(in outcome, in cancellationToken))
        {
            return IsTransient(outcome);
        }

        return true;
    }

    /// <summary>
    /// Determines whether an exception should be treated by resilience strategies as
    /// a transient failure.
    /// </summary>
    internal static bool IsTransientHttpException(Exception exception)
    {
        if (exception is null) throw new ArgumentNullException(nameof(exception));
        if (exception is HttpRequestException || exception is TimeoutRejectedException)
        {
            return true;
        }

        return false;
    }

    internal static bool IsHttpConnectionTimeout(in Outcome<HttpResponseMessage> outcome, in CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            Exception exception = outcome.Exception;
            if (exception is OperationCanceledException && exception.Source == "System.Private.CoreLib")
            {
                return exception.InnerException is TimeoutException;
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a response contains a transient failure.
    /// </summary>
    /// <remarks>The current handling implementation uses approach proposed by Polly: https://github.com/App-vNext/Polly.Extensions.Http/blob/master/src/Polly.Extensions.Http/HttpPolicyExtensions.cs.</remarks>
    internal static bool IsTransientHttpFailure(HttpResponseMessage response)
    {
        if (response is null) throw new ArgumentNullException(nameof(response));
        int statusCode = (int)response.StatusCode;
        if (statusCode < 500 && response.StatusCode != HttpStatusCode.RequestTimeout)
        {
            return statusCode == 429;
        }

        return true;
    }
}
