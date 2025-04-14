﻿using Microsoft.Xrm.Sdk;
using System;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;

namespace XrmGenTest;

[Plugin]
[Step("Create", "contact", "firstname,lastname", Stages.PostOperation, ExecutionMode.Synchronous)]
[Image(ImageTypes.PostImage, "firstname,lastname")]
public partial class ContactCreatePlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new InvalidPluginExecutionException(nameof(serviceProvider));
        }
        Initialize(serviceProvider);
    }
}
