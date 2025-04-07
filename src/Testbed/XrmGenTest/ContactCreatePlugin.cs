using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmTools.Meta.Attributes;
using XrmTools.Meta.Model;

namespace XrmGenTest
{
    [Plugin("ContactCreate")]
    [Step("contact", "Create", "firstname,lastname", Stages.PostOperation, ExecutionMode.Synchronous)]
    [Image(ImageTypes.PostImage, "Target", "firstname,lastname")]
    public partial class ContactCreatePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}
