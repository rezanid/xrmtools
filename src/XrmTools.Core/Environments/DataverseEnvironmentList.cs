#nullable enable
namespace XrmTools;
using System;
using System.Collections.Generic;

public class DataverseEnvironmentList : List<DataverseEnvironment>
{
    public new void Add(DataverseEnvironment environment)
    {
        if (Contains(environment))
        {
            throw new ArgumentException($"An environment with the URL '{environment.Url}' already exists.");
        }

        base.Add(environment);
    }

    public new void Insert(int index, DataverseEnvironment environment)
    {
        if (Contains(environment))
        {
            throw new ArgumentException($"An environment with the URL '{environment.Url}' already exists.");
        }

        base.Insert(index, environment);
    }

    public new void AddRange(IEnumerable<DataverseEnvironment> environments)
    {
        foreach (var environment in environments)
        {
            if (Contains(environment))
            {
                throw new ArgumentException($"An environment with the URL '{environment.Url}' already exists.");
            }
        }

        base.AddRange(environments);
    }
}
#nullable restore