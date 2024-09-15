namespace XrmGen.Logging;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmGen.Logging;

public class OutputLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new OutputLogger(categoryName);
    }

    public void Dispose()
    {
        // No resources to dispose in this example.
    }
}

