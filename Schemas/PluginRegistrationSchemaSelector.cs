using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmGen.Schemas;

/// <summary>
/// This notifies VS to use the pluginregistration-schema for plugin definition (JSON) files.
/// </summary>
[Export(typeof(IJsonSchemaSelector))]
public class PluginRegistrationSchemaSelector : IJsonSchemaSelector
{

}
