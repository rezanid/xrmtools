namespace XrmTools.WebResourceProjectTemplate
{
    using EnvDTE;
    using Microsoft.VisualStudio.TemplateWizard;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Materializes the scaffold before the JavaScript Project System loads the generated
    /// .esproj. JSPS does not materialize ProjectItem entries from legacy .vstemplate files.
    /// </summary>
    public sealed class WebResourceProjectWizard : IWizard
    {
        private const string ResourcePrefix = "XrmTools.WebResourceProjectTemplate.Content.";

        private string destinationDirectory;
        private Dictionary<string, string> replacements;

        private static readonly ScaffoldFile[] ScaffoldFiles =
        {
            new ScaffoldFile("gitignore.txt", ".gitignore"),
            new ScaffoldFile("package.json", "package.json"),
            new ScaffoldFile("README.md", "README.md"),
            new ScaffoldFile("tsconfig.json", "tsconfig.json"),
            new ScaffoldFile("src.AccountExperience.ts", Path.Combine("src", "AccountExperience.ts"))
        };

        public void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind,
            object[] customParams)
        {
            if (runKind != WizardRunKind.AsNewProject)
            {
                return;
            }

            string destinationDirectory;
            if (!replacementsDictionary.TryGetValue("$destinationdirectory$", out destinationDirectory) ||
                string.IsNullOrWhiteSpace(destinationDirectory))
            {
                throw new WizardBackoutException("The web-resource project destination could not be resolved.");
            }

            this.destinationDirectory = destinationDirectory;
            replacements = new Dictionary<string, string>(replacementsDictionary);
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public void ProjectFinishedGenerating(Project project)
        {
            if (destinationDirectory == null || replacements == null)
            {
                throw new WizardBackoutException("The web-resource project wizard was not initialized.");
            }

            WriteScaffold(destinationDirectory, replacements);
            destinationDirectory = null;
            replacements = null;
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        private static void WriteScaffold(
            string destinationDirectory,
            Dictionary<string, string> replacements)
        {
            var destinationRoot = Path.GetFullPath(destinationDirectory);
            var destinationPrefix = destinationRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var createdFiles = new List<string>();

            try
            {
                Directory.CreateDirectory(destinationRoot);

                foreach (var scaffoldFile in ScaffoldFiles)
                {
                    var targetPath = Path.GetFullPath(Path.Combine(destinationRoot, scaffoldFile.RelativePath));
                    if (!targetPath.StartsWith(destinationPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new WizardBackoutException("A web-resource template path escaped the project directory.");
                    }

                    if (File.Exists(targetPath))
                    {
                        throw new WizardBackoutException(
                            "The web-resource template cannot overwrite the existing file '" + targetPath + "'.");
                    }

                    var parentDirectory = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(parentDirectory))
                    {
                        Directory.CreateDirectory(parentDirectory);
                    }

                    var content = ReadResource(scaffoldFile.ResourceName);
                    foreach (var replacement in replacements)
                    {
                        content = content.Replace(replacement.Key, replacement.Value ?? string.Empty);
                    }

                    File.WriteAllText(targetPath, content, new UTF8Encoding(false));
                    createdFiles.Add(targetPath);
                }
            }
            catch
            {
                foreach (var createdFile in createdFiles)
                {
                    try
                    {
                        File.Delete(createdFile);
                    }
                    catch
                    {
                    }
                }

                throw;
            }
        }

        private static string ReadResource(string resourceName)
        {
            var assembly = typeof(WebResourceProjectWizard).Assembly;
            using (var stream = assembly.GetManifestResourceStream(ResourcePrefix + resourceName))
            {
                if (stream == null)
                {
                    throw new WizardBackoutException(
                        "The embedded web-resource template file '" + resourceName + "' was not found.");
                }

                using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private sealed class ScaffoldFile
        {
            public ScaffoldFile(string resourceName, string relativePath)
            {
                ResourceName = resourceName;
                RelativePath = relativePath;
            }

            public string ResourceName { get; private set; }

            public string RelativePath { get; private set; }
        }
    }
}
