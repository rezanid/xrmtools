using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using XrmTools;
using XrmTools.Meta.Attributes;

namespace XrmGenTest;

[Plugin]
[Step("Create", "contact", "firstname,lastname,description,accountrolecode", Stages.PreOperation, ExecutionMode.Synchronous)]
[Step("Update", "contact", "firstname,lastname,description,accountrolecode", Stages.PreOperation, ExecutionMode.Synchronous)]
[Image(ImageTypes.PreImage, "firstname,lastname,description,accountrolecode")]
public partial class ContactCreatePlugin : IPlugin
{
    [Dependency]
    public IServiceProvider ServiceProvider
    {
        get => DependencyScope<ContactCreatePlugin>.Current.Require<IServiceProvider>();
        set => DependencyScope<ContactCreatePlugin>.Current.Set(value);
    }

    [DependencyProvider]
    public IOrganizationService OrganizationService 
    {
        get
        {
            if (DependencyScope<ContactCreatePlugin>.Current.TryGet<IOrganizationService>(out var service))
            {
                return service;
            }
            service = DependencyScope<ContactCreatePlugin>.Current.Require<IServiceProvider>().Get<IOrganizationServiceFactory>()
                .CreateOrganizationService(null);
            DependencyScope<ContactCreatePlugin>.Current.Set(service);
            return service;
        }
    }

    [DependencyProvider("User")]
    public IOrganizationService OrganizationServiceUser
    {
        get
        {
            if (DependencyScope<ContactCreatePlugin>.Current.TryGet<IOrganizationService>(out var service))
            {
                return service;
            }
            service = DependencyScope<ContactCreatePlugin>.Current.Require<IServiceProvider>().Get<IOrganizationServiceFactory>()
                .CreateOrganizationService(null);
            DependencyScope<ContactCreatePlugin>.Current.Set(service);
            return service;
        }
    }

    [Dependency]
    public SomeService ServiceFactory
    {
        get;set;
    }

    [Dependency]
    public IContactPersister ContactPersister { get; set; }

    public ITracingService MyProperty { get; set; }
    public void Execute(IServiceProvider serviceProvider)
    {
        CreateTarget.Description = $"Hello {CreateTarget.FirstName} {CreateTarget.LastName}!";
        
    }
}

public class SomeService(IServiceProvider provider)
{
    
}

public interface IContactPersister
{
    void Persist(Contact contact);
}

public class ContactPersister(IOrganizationService service) : IContactPersister
{
    [Dependency("User")]
    public IOrganizationService OrgService { get; set; }
    
    public void Persist(Contact contact)
    {
        service.Create(contact);
    }
}