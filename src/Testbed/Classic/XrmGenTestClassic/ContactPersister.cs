namespace XrmGenTestClassic
{
    using Microsoft.Xrm.Sdk;
    using System;

    public interface IContactPersister
    {
        Guid? Persist(ApiContact contact);
    }
    
    public class ContactPersister : IContactPersister, IDisposable
    {
        private readonly IOrganizationService _organizationService;
        private readonly ITracingService _tracing;
        
        public ContactPersister(IOrganizationServiceFactory organizationServiceFactory, ITracingService tracing)
        {
            if (organizationServiceFactory is null) throw new ArgumentNullException(nameof(organizationServiceFactory));
            _organizationService = organizationServiceFactory.CreateOrganizationService(null);
            _tracing = tracing ?? throw new ArgumentNullException(nameof(tracing));
        }

        public void Dispose()
        {
            _tracing.Trace($"{nameof(ContactPersister)}.Dispose has been called.");
        }
        
        public Guid? Persist(ApiContact contact)
        {
            _tracing.Trace("Persisting contact entity.");
            contact.Id = _organizationService.Create(contact);
            _tracing.Trace("Contact entity persisted with ID: " + contact.Id);
            return contact.Id;
        }
    }
}