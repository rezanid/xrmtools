namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using XrmTools.WebApi.Types;

public class BulkDeleteRequest : HttpRequestMessage
{
    private List<QueryExpression>? _querySet;
    private string? _jobName;
    private bool? _sendEmailNotification;
    private List<JObject>? _toRecipients;
    private List<JObject>? _cCRecipients;
    private string? _recurrencePattern;
    private DateTimeOffset? _startDateTime;
    private Guid? _sourceImportId;
    private bool? _runNow;

    public BulkDeleteRequest()
    {
        Method = HttpMethod.Post;
        RequestUri = new Uri(
            uriString: "BulkDelete",
            uriKind: UriKind.Relative);
    }


    public List<QueryExpression>? QuerySet
    {
        get => _querySet;
        set
        {
            _querySet = value;
            SetContent();
        }
    }
    public string? JobName
    {
        get => _jobName;
        set
        {
            _jobName = value;
            SetContent();
        }
    }
    public bool? SendEmailNotification
    {
        get => _sendEmailNotification;
        set
        {
            _sendEmailNotification = value;
            SetContent();
        }
    }
    public List<JObject>? ToRecipients
    {
        get => _toRecipients;
        set
        {
            _toRecipients = value;
            SetContent();
        }
    }
    public List<JObject>? CCRecipients
    {
        get => _cCRecipients;
        set
        {
            _cCRecipients = value;
            SetContent();
        }
    }
    public required string RecurrencePattern
    {
        get => _recurrencePattern ?? string.Empty;
        set
        {
            _recurrencePattern = value;
            SetContent();
        }
    }
    public DateTimeOffset? StartDateTime
    {
        get => _startDateTime;
        set
        {
            _startDateTime = value;
            SetContent();
        }
    }
    public Guid? SourceImportId
    {
        get => _sourceImportId;
        set
        {
            _sourceImportId = value;
            SetContent();
        }
    }
    public bool? RunNow
    {
        get => _runNow;
        set
        {
            _runNow = value;
            SetContent();
        }
    }

    private void SetContent()
    {
        JObject _content = [];

        if (_querySet != null)
        {
            _content.Add(nameof(QuerySet), JToken.FromObject(_querySet));
        }

        if (!string.IsNullOrWhiteSpace(_jobName))
        {
            _content.Add(nameof(JobName), _jobName);
        }

        if (_sendEmailNotification != null)
        {
            _content.Add(nameof(SendEmailNotification), _sendEmailNotification);
        }

        if (_toRecipients != null)
        {
            _content.Add(nameof(ToRecipients), JToken.FromObject(_toRecipients));
        }

        if (_cCRecipients != null)
        {
            _content.Add(nameof(CCRecipients), JToken.FromObject(_cCRecipients));
        }

        // Always required
        _content.Add(nameof(RecurrencePattern), _recurrencePattern);

        if (_startDateTime != null)
        {
            _content.Add(nameof(StartDateTime), JToken.FromObject(_startDateTime));
        }

        if (_sourceImportId != null)
        {
            _content.Add(nameof(SourceImportId), JToken.FromObject(_sourceImportId));
        }

        if (_runNow != null)
        {
            _content.Add(nameof(RunNow), JToken.FromObject(_runNow));
        }


        Content = new StringContent(
            content: _content.ToString(),
            encoding: System.Text.Encoding.UTF8,
            mediaType: "application/json");

    }
}