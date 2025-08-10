namespace XrmTools.WebApi.Messages;

using System;

/// <summary>
/// Contains the response from the CreateMultipleRequest
/// </summary>
public class CreateMultipleResponse
{
    /// <summary>
    /// The ID values of the created records
    /// </summary>
    public Guid[] Ids { get; set; }
}