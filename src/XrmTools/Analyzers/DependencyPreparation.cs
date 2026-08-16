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
        var declaredLocalTypes = new HashSet<string>(StringComparer.Ordinal);

        static bool CanDeclareLocal(Dependency node)
            => node.IsLocalVariableNeeded
                && node.ProvidedByProperty is null
                && node.ProvidedByName is null
                && node.FullTypeName != "System.IServiceProvider"
                && node.FullTypeName != "System.String"
                && node.FullTypeName != "string"
                && !node.FullTypeName.StartsWith("Microsoft.Xrm.Sdk.", StringComparison.Ordinal);

        // First pass: count types
        void CountTypes(Dependency node)
        {
            if (typeCount.ContainsKey(node.ResolvedFullTypeName))
                typeCount[node.ResolvedFullTypeName]++;
            else
                typeCount[node.ResolvedFullTypeName] = 1;

            foreach (var child in node.Dependencies)
                CountTypes(child);
        }

        CountTypes(root);

        // Second pass: mark reused types
        void MarkLocals(Dependency node)
        {
            node.IsLocalVariableDeclaration = false;
            node.IsLocalVariableNeeded = typeCount[node.ResolvedFullTypeName] > 1;

            if (CanDeclareLocal(node) && declaredLocalTypes.Add(node.ResolvedFullTypeName))
                node.IsLocalVariableDeclaration = true;

            foreach (var child in node.Dependencies)
                if (child.ResolvedFullTypeName != "System.IServiceProvider") MarkLocals(child);
        }

        MarkLocals(root);
    }
}
