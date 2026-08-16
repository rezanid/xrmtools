namespace XrmTools.ComponentModel;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public sealed class DependentPropertiesTypeDescriptionProvider<T> : TypeDescriptionProvider
{
    private static readonly TypeDescriptionProvider DefaultProvider = TypeDescriptor.GetProvider(typeof(T));

    public DependentPropertiesTypeDescriptionProvider() : base(DefaultProvider) { }

    public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
    {
        var baseDescriptor = base.GetTypeDescriptor(objectType, instance);
        return new DependentPropertiesTypeDescriptor(baseDescriptor, instance);
    }

    private sealed class DependentPropertiesTypeDescriptor : CustomTypeDescriptor
    {
        private readonly object instance;

        public DependentPropertiesTypeDescriptor(ICustomTypeDescriptor parent, object instance)
            : base(parent)
        {
            this.instance = instance;
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var props = base.GetProperties(attributes);

            // If there's no instance (designer may call this way), just return base.
            if (instance is null)
                return props;

            var list = new List<PropertyDescriptor>(props.Count);

            foreach (PropertyDescriptor p in props)
            {
                // Only wrap properties that have our attribute(s)
                if (p.Attributes.OfType<ReadOnlyWhenAttribute>().Any())
                    list.Add(new DependentPropertyDescriptor(p, instance));
                else
                    list.Add(p);
            }

            return new PropertyDescriptorCollection(list.ToArray(), readOnly: true);
        }
    }
}