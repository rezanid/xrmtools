#nullable enable
namespace XrmTools.Helpers;

using Microsoft.VisualStudio.Shell;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
[ComVisible(false)]
public sealed class ProvideSolutionProperties(string propertyName) : RegistrationAttribute
{
    public string PropertyName => propertyName;

    public override void Register(RegistrationContext context)
    {
        context.Log.WriteLine(string.Format(CultureInfo.InvariantCulture, "ProvideSolutionProperties: ({0} = {1})", context.ComponentType.GUID.ToString("B"), PropertyName));
        RegistrationAttribute.Key? key = null;
        try
        {
            key = context.CreateKey(string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", "SolutionPersistence", PropertyName));
            key.SetValue(string.Empty, context.ComponentType.GUID.ToString("B"));
        }
        finally
        {
            key?.Close();
        }
    }

    public override void Unregister(RegistrationAttribute.RegistrationContext context) 
        => context.RemoveKey(string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", "SolutionPersistence", PropertyName));
}
#nullable restore