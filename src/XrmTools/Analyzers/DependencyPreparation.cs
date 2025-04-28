namespace XrmTools.Meta.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

public interface IDependencyPreparation
{
    void Prepare(Dependency root);
}

[Export(typeof(IDependencyPreparation))]
public class DependencyPreparation : IDependencyPreparation
{
    public void Prepare(Dependency root)
    {
        var typeCount = new Dictionary<string, int>(StringComparer.Ordinal);

        // First pass: count types
        void CountTypes(Dependency node)
        {
            if (typeCount.ContainsKey(node.FullTypeName))
                typeCount[node.FullTypeName]++;
            else
                typeCount[node.FullTypeName] = 1;

            foreach (var child in node.Dependencies)
                CountTypes(child);
        }

        CountTypes(root);

        // Second pass: mark reused types
        void MarkLocals(Dependency node)
        {
            if (typeCount[node.FullTypeName] > 1)
                node.IsLocalVariableNeeded = true;

            foreach (var child in node.Dependencies)
                if (child.FullTypeName != "System.IServiceProvider") MarkLocals(child);
        }

        MarkLocals(root);
    }
}
