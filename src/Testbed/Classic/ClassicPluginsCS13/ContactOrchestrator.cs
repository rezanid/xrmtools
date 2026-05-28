namespace XrmGenTest;

using Microsoft.Xrm.Sdk;
using System;

public interface IContactOrchestrator
{
    bool NeedToCreateContact(Contact contact);
}

public class ContactOrchestrator : IContactOrchestrator, IDisposable
{
    private readonly IContactPersister _contactPersister;
    private readonly ITracingService _tracing;
    
    public ContactOrchestrator(IContactPersister contactPersister, ITracingService tracing)
    {
        _contactPersister = contactPersister ?? throw new ArgumentNullException(nameof(contactPersister));
        _tracing = tracing ?? throw new ArgumentNullException(nameof(tracing));
    }

    public void Dispose() => throw new NotImplementedException();

    public bool NeedToCreateContact(Contact contact)
    {
        if (contact == null) throw new ArgumentNullException(nameof(contact));
        if (string.IsNullOrWhiteSpace(contact.FirstName) || string.IsNullOrWhiteSpace(contact.LastName))
        {
            _tracing.Trace("Contact creation skipped due to missing first or last name.");
            return false;
        }
        return true;
    }
}