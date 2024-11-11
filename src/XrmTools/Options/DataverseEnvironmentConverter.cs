#nullable enable
namespace XrmTools.Options;
using System;
using System.ComponentModel;
using System.Globalization;

public class DataverseEnvironmentConverter : ExpandableObjectConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is DataverseEnvironment environment)
        {
            // Return the desired format when the object is collapsed
            if (DataverseEnvironment.Empty.Equals(environment))
            {
                return "Empty";
            }
            return $"{environment.Name} ({environment.Url})";
        }

        // Call the base class to handle other conversions
        return base.ConvertTo(context, culture, value, destinationType);
    }
}
#nullable restore