namespace XrmTools.Authentication;
using Microsoft.Identity.Client;

public static class ClientApplicationExtensions
{
    public static IConfidentialClientApplication AsConfidentialClient(this IClientApplicationBase app) => app as IConfidentialClientApplication;
    public static IPublicClientApplication AsPublicClient(this IClientApplicationBase app) => app as IPublicClientApplication;
    public static IByRefreshToken AsRefreshTokenClient(this IClientApplicationBase app) => app as IByRefreshToken;
}