using Microsoft.Xrm.Sdk;
using System;
using System.Web.UI.WebControls;
using XrmTools;
using XrmTools.Meta.Attributes;

namespace XrmGenTest
{
    public class PluginBase<TPlugin> : IPlugin where TPlugin : PluginBase<TPlugin>
    {
        [DependencyProvider]
        public ITracingService TracingService
        {
            get => TryGet<ITracingService>(out var tracingService)
                ? tracingService
                : Set(Require<IServiceProvider>().GetService(typeof(ITracingService)) as ITracingService);
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                InternalExecute(serviceProvider);
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Something went wrong! " + ex.ToString(), ex);
            }
        }
        protected virtual void InternalExecute(IServiceProvider serviceProvider)
        {
            // This method can be overridden in derived classes to implement specific plugin logic.
        }

        protected static T Require<T>() => DependencyScope<TPlugin>.Current.Require<T>();
        protected static bool TryGet<T>(out T value) => DependencyScope<TPlugin>.Current.TryGet(out value);
        protected static T Set<T>(T instance) => DependencyScope<TPlugin>.Current.Set(instance);
    }

    [Plugin]
    [Step("Create", "contact", "firstname,lastname,description,accountrolecode", Stages.PreOperation, ExecutionMode.Synchronous)]
    [Step("Update", "contact", "firstname,lastname,description,accountrolecode", Stages.PreOperation, ExecutionMode.Synchronous)]
    public partial class ContactCreatePlugin : PluginBase<ContactCreatePlugin>, IPlugin
    {
        [DependencyProvider]
        public IOrganizationServiceFactory OrganizationServiceFactory
        {
            get => TryGet<IOrganizationServiceFactory>(out var organizationService)
                ? organizationService
                : Set(Require<IServiceProvider>().GetService(typeof(IOrganizationServiceFactory)) as IOrganizationServiceFactory);
        }

        [Dependency]
        public IContactPersister ContactPersister { get => Require<IContactPersister>(); }

        [Dependency]
        public IContactOrchestrator ContactOrchestrator { get => Require<IContactOrchestrator>(); }

        [Dependency]
        public IValidationService ValidationService { get => Require<IValidationService>(); }

        override protected void InternalExecute(IServiceProvider serviceProvider)
        {
            using (var scope = CreateScope(serviceProvider))
            {
                CreateTarget.Description = $"Hello {CreateTarget.FirstName} {CreateTarget.LastName}!";
            }
        }
    }
    
    public interface IContactPersister
    {
        Guid? Persist(Contact contact);
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

        public void Dispose() => throw new NotImplementedException();
        public Guid? Persist(Contact contact) => _organizationService.Create(contact);
    }
    
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
    
    public interface IValidationService
    {
        bool ValidateContact(Contact contact);
    }
    
    public class ValidationService : IValidationService
    {
        public void Dispose() => throw new NotImplementedException();
        public bool ValidateContact(Contact contact)
        {
            if (contact == null) throw new ArgumentNullException(nameof(contact));
            if (string.IsNullOrWhiteSpace(contact.FirstName) || string.IsNullOrWhiteSpace(contact.LastName))
            {
                return false;
            }
            return true;
        }
    }
}