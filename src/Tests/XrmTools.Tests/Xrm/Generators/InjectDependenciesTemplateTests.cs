namespace XrmTools.Tests.Xrm.Generators;

using FluentAssertions;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using XrmTools.Meta.Model;
using XrmTools.Meta.Model.Configuration;
using XrmTools.Xrm.Generators;
using Xunit;

public class InjectDependenciesTemplateTests
{
    [Fact]
    public void Render_ReusedDisposableDependency_DeclaresLocalBeforeScopeRegistration()
    {
        var templatePath = GetTemplatePath();
        var templateContent = "{{-func find_method(methods, name)\n  for m in methods\n    if m.name == name\n      ret m\n    end\n  end\nend -}}\n" + File.ReadAllText(templatePath);
        var template = Template.Parse(templateContent, templatePath);

        template.HasErrors.Should().BeFalse(string.Join(Environment.NewLine, template.Messages));

        var root = JsonSerializer.Deserialize<Dependency>("""
            {
              "PropertyName": null,
              "FullTypeName": "XrmGenTest.ContactGreetingPlugin",
              "ShortTypeName": "ContactGreetingPlugin",
              "ResolvedFullTypeName": "XrmGenTest.ContactGreetingPlugin",
              "ResolvedShortTypeName": "ContactGreetingPlugin",
              "Dependencies": [
                {
                  "PropertyName": "Tracing",
                  "FullTypeName": "Microsoft.Xrm.Sdk.ITracingService",
                  "ShortTypeName": "ITracingService",
                  "ResolvedFullTypeName": "Microsoft.Xrm.Sdk.ITracingService",
                  "ResolvedShortTypeName": "ITracingService",
                  "Dependencies": [],
                  "IsProperty": true,
                  "IsLocalVariableNeeded": true,
                  "IsDisposable": false,
                  "ProvidedByProperty": null,
                  "ProvidedByName": null
                },
                {
                  "PropertyName": "Logging",
                  "FullTypeName": "XrmGenTest.ILoggingService",
                  "ShortTypeName": "ILoggingService",
                  "ResolvedFullTypeName": "XrmGenTest.LoggingService",
                  "ResolvedShortTypeName": "LoggingService",
                  "Dependencies": [
                    {
                      "PropertyName": "serviceProvider",
                      "FullTypeName": "System.IServiceProvider",
                      "ShortTypeName": "serviceProvider",
                      "ResolvedFullTypeName": "",
                      "ResolvedShortTypeName": "",
                      "Dependencies": [],
                      "IsProperty": false,
                      "IsLocalVariableNeeded": false,
                      "IsDisposable": false,
                      "ProvidedByProperty": null,
                      "ProvidedByName": null
                    }
                  ],
                  "IsProperty": true,
                  "IsLocalVariableNeeded": false,
                  "IsDisposable": false,
                  "ProvidedByProperty": null,
                  "ProvidedByName": null
                },
                {
                  "PropertyName": "OrgServiceFactory",
                  "FullTypeName": "Microsoft.Xrm.Sdk.IOrganizationServiceFactory",
                  "ShortTypeName": "IOrganizationServiceFactory",
                  "ResolvedFullTypeName": "Microsoft.Xrm.Sdk.IOrganizationServiceFactory",
                  "ResolvedShortTypeName": "IOrganizationServiceFactory",
                  "Dependencies": [],
                  "IsProperty": true,
                  "IsLocalVariableNeeded": true,
                  "IsDisposable": false,
                  "ProvidedByProperty": null,
                  "ProvidedByName": null
                },
                {
                  "PropertyName": "Context",
                  "FullTypeName": "Microsoft.Xrm.Sdk.IPluginExecutionContext",
                  "ShortTypeName": "IPluginExecutionContext",
                  "ResolvedFullTypeName": "Microsoft.Xrm.Sdk.IPluginExecutionContext",
                  "ResolvedShortTypeName": "IPluginExecutionContext",
                  "Dependencies": [],
                  "IsProperty": true,
                  "IsLocalVariableNeeded": false,
                  "IsDisposable": false,
                  "ProvidedByProperty": null,
                  "ProvidedByName": null
                },
                {
                  "PropertyName": "ContactPersister",
                  "FullTypeName": "XrmGenTest.IContactPersister",
                  "ShortTypeName": "IContactPersister",
                  "ResolvedFullTypeName": "XrmGenTest.ContactPersister",
                  "ResolvedShortTypeName": "ContactPersister",
                  "Dependencies": [
                    {
                      "PropertyName": "organizationServiceFactory",
                      "FullTypeName": "Microsoft.Xrm.Sdk.IOrganizationServiceFactory",
                      "ShortTypeName": "IOrganizationServiceFactory",
                      "ResolvedFullTypeName": "Microsoft.Xrm.Sdk.IOrganizationServiceFactory",
                      "ResolvedShortTypeName": "IOrganizationServiceFactory",
                      "Dependencies": [],
                      "IsProperty": false,
                      "IsLocalVariableNeeded": true,
                      "IsDisposable": false,
                      "ProvidedByProperty": null,
                      "ProvidedByName": null
                    },
                    {
                      "PropertyName": "tracing",
                      "FullTypeName": "Microsoft.Xrm.Sdk.ITracingService",
                      "ShortTypeName": "ITracingService",
                      "ResolvedFullTypeName": "Microsoft.Xrm.Sdk.ITracingService",
                      "ResolvedShortTypeName": "ITracingService",
                      "Dependencies": [],
                      "IsProperty": false,
                      "IsLocalVariableNeeded": true,
                      "IsDisposable": false,
                      "ProvidedByProperty": null,
                      "ProvidedByName": null
                    }
                  ],
                  "IsProperty": true,
                  "IsLocalVariableNeeded": true,
                  "IsDisposable": true,
                  "ProvidedByProperty": null,
                  "ProvidedByName": null
                },
                {
                  "PropertyName": "ContactOrchestrator",
                  "FullTypeName": "XrmGenTest.IContactOrchestrator",
                  "ShortTypeName": "IContactOrchestrator",
                  "ResolvedFullTypeName": "XrmGenTest.ContactOrchestrator",
                  "ResolvedShortTypeName": "ContactOrchestrator",
                  "Dependencies": [
                    {
                      "PropertyName": "contactPersister",
                      "FullTypeName": "XrmGenTest.IContactPersister",
                      "ShortTypeName": "IContactPersister",
                      "ResolvedFullTypeName": "XrmGenTest.ContactPersister",
                      "ResolvedShortTypeName": "ContactPersister",
                      "Dependencies": [
                        {
                          "PropertyName": "organizationServiceFactory",
                          "FullTypeName": "Microsoft.Xrm.Sdk.IOrganizationServiceFactory",
                          "ShortTypeName": "IOrganizationServiceFactory",
                          "ResolvedFullTypeName": "Microsoft.Xrm.Sdk.IOrganizationServiceFactory",
                          "ResolvedShortTypeName": "IOrganizationServiceFactory",
                          "Dependencies": [],
                          "IsProperty": false,
                          "IsLocalVariableNeeded": true,
                          "IsDisposable": false,
                          "ProvidedByProperty": null,
                          "ProvidedByName": null
                        },
                        {
                          "PropertyName": "tracing",
                          "FullTypeName": "Microsoft.Xrm.Sdk.ITracingService",
                          "ShortTypeName": "ITracingService",
                          "ResolvedFullTypeName": "Microsoft.Xrm.Sdk.ITracingService",
                          "ResolvedShortTypeName": "ITracingService",
                          "Dependencies": [],
                          "IsProperty": false,
                          "IsLocalVariableNeeded": true,
                          "IsDisposable": false,
                          "ProvidedByProperty": null,
                          "ProvidedByName": null
                        }
                      ],
                      "IsProperty": false,
                      "IsLocalVariableNeeded": true,
                      "IsDisposable": true,
                      "ProvidedByProperty": null,
                      "ProvidedByName": null
                    },
                    {
                      "PropertyName": "tracing",
                      "FullTypeName": "Microsoft.Xrm.Sdk.ITracingService",
                      "ShortTypeName": "ITracingService",
                      "ResolvedFullTypeName": "Microsoft.Xrm.Sdk.ITracingService",
                      "ResolvedShortTypeName": "ITracingService",
                      "Dependencies": [],
                      "IsProperty": false,
                      "IsLocalVariableNeeded": true,
                      "IsDisposable": false,
                      "ProvidedByProperty": null,
                      "ProvidedByName": null
                    }
                  ],
                  "IsProperty": true,
                  "IsLocalVariableNeeded": false,
                  "IsDisposable": true,
                  "ProvidedByProperty": null,
                  "ProvidedByName": null
                },
                {
                  "PropertyName": "ValidationService",
                  "FullTypeName": "XrmGenTest.IValidationService",
                  "ShortTypeName": "IValidationService",
                  "ResolvedFullTypeName": "XrmGenTest.ValidationService",
                  "ResolvedShortTypeName": "ValidationService",
                  "Dependencies": [],
                  "IsProperty": true,
                  "IsLocalVariableNeeded": false,
                  "IsDisposable": false,
                  "ProvidedByProperty": null,
                  "ProvidedByName": null
                },
                {
                  "PropertyName": "OrganizationService",
                  "FullTypeName": "Microsoft.Xrm.Sdk.IOrganizationService",
                  "ShortTypeName": "IOrganizationService",
                  "ResolvedFullTypeName": "Microsoft.Xrm.Sdk.IOrganizationService",
                  "ResolvedShortTypeName": "IOrganizationService",
                  "Dependencies": [],
                  "IsProperty": true,
                  "IsLocalVariableNeeded": false,
                  "IsDisposable": false,
                  "ProvidedByProperty": "UserOrgService",
                  "ProvidedByName": null
                }
              ],
              "IsProperty": false,
              "IsLocalVariableNeeded": false,
              "IsDisposable": false,
              "ProvidedByProperty": null,
              "ProvidedByName": null
            }
            """);

        root.Should().NotBeNull();
        new DependencyPreparation().Prepare(root!);

        var scriptObject = new ScriptObject();
        scriptObject.Import(typeof(ScribanExtensions));
        scriptObject.Add("dependencies", root!.Dependencies);
        scriptObject.Add("plugintype", new PluginTypeConfig
        {
            TypeName = "XrmGenTest.ContactGreetingPlugin",
            BaseTypeMethods = []
        });

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);

        var output = template.Render(context);

        output.Should().Contain("var contactPersister = new XrmGenTest.ContactPersister(");
        output.Should().Contain("scope.Set<XrmGenTest.IContactPersister>(contactPersister);");
        output.Should().Contain("scope.Set<XrmGenTest.IContactOrchestrator>(scope.SetAndTrack(new XrmGenTest.ContactOrchestrator(contactPersister");
        output.Should().NotContain("var iTracingService =");
        output.Should().NotContain("var iOrganizationServiceFactory =");
        output.Should().NotContain("\r\n\r\n\r\n");
        output.Split(["var contactPersister = new XrmGenTest.ContactPersister("], StringSplitOptions.None).Length.Should().Be(2);

        output.IndexOf("var contactPersister = new XrmGenTest.ContactPersister(", StringComparison.Ordinal)
            .Should().BeLessThan(output.IndexOf("scope.Set<XrmGenTest.IContactPersister>(contactPersister);", StringComparison.Ordinal));
    }

    private static string GetTemplatePath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "src", "XrmTools", "CodeGenTemplates", "InjectDependencies.sbncs");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException("Could not locate InjectDependencies.sbncs from the test output directory.");
    }
}
