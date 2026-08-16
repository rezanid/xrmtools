namespace XrmTools.Serialization;
using Microsoft.VisualStudio.OLE.Interop;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

internal class PolymorphicContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (IsPolymorphicKnownType(property.PropertyType))
        {
            property.Converter = new KnownTypeConverter(property.PropertyType);
        }

        return property;
    }

    // TODO: Fix loading private assemblies from outside appbase directory.
    // This line causes:
    // Exception thrown: 'System.IO.FileLoadException' in mscorlib.dll
    // An exception of type 'System.IO.FileLoadException' occurred in mscorlib.dll but was not handled in user code
    // Could not load file or assembly 'XrmTools.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' or one of its dependencies.The private assembly was located outside the appbase directory. (Exception from HRESULT: 0x80131041)
    private bool IsPolymorphicKnownType(Type type) => type.IsArray ?
        type.GetElementType().GetCustomAttributes<KnownTypeAttribute>(true).Any(attr => attr.Type != type) :
        type.GetCustomAttributes<KnownTypeAttribute>(true).Any(attr => attr.Type != type);
}
