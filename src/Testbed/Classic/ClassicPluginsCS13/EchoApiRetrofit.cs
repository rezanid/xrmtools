using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmTools.Meta.Attributes;

namespace XrmGenTest;

// This is to test the RefactoringAction that should generate the rest of the code.
// The action should add the missing parameters along with the requst and response classes.

[CustomApi("test_Echo")]
public class EchoApiRetrofit
{
}
