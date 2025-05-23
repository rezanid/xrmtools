namespace XrmTools.Extensions
{
    using Microsoft.Xrm.Sdk;

    internal static class OrganizationServiceExtensions
    {
        /// <summary>
        /// Executes a typed organization request and returns a typed organization response.
        /// </summary>
        /// <remarks>This method converts the typed request into a standard <see
        /// cref="OrganizationRequest"/>, executes it using the provided <see cref="Microsoft.Xrm.Sdk.IOrganizationService"/>, and then
        /// converts the resulting <see cref="Microsoft.Xrm.Sdk.OrganizationResponse"/> into a typed response.</remarks>
        /// <typeparam name="TRequest">The type of the request, which must implement <see cref="ITypedOrganizationRequest"/>.</typeparam>
        /// <typeparam name="TResponse">The type of the response, which must implement <see cref="ITypedOrganizationResponse"/> and have a
        /// parameterless constructor.</typeparam>
        /// <param name="service">The <see cref="Microsoft.Xrm.Sdk.IOrganizationService"/> used to execute the request.</param>
        /// <param name="request">The typed request to execute. Must not be <see langword="null"/>.</param>
        /// <returns>A typed response of type <typeparamref name="TResponse"/> containing the results of the executed request.</returns>
        public static TResponse ExecuteTyped<TRequest, TResponse>(
            this IOrganizationService service,
            TRequest request)
            where TRequest : ITypedOrganizationRequest
            where TResponse : ITypedOrganizationResponse, new()
        {
            var orgRequest = request.ToOrganizationRequest();
            var orgResponse = service.Execute(orgRequest);

            var typedResponse = new TResponse();
            typedResponse.LoadFromOrganizationResponse(orgResponse);
            return typedResponse;
        }
    }
}