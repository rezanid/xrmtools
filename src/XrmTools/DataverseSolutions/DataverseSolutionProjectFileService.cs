#nullable enable
namespace XrmTools.DataverseSolutions;

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using XrmTools.Options;

internal interface IDataverseSolutionProjectFileService
{
    string FindGeneratedSolutionRoot(string outputDirectory);

    string CopyGeneratedProjectDirectory(string outputDirectory, string projectDirectory);

    string FinalizeProjectFile(string projectDirectory, string projectName, DataverseSolutionProjectSdk projectSdk);

    string WriteMinimalProjectFile(string projectDirectory, string projectName);

    Exception? TryDeleteDirectory(string directoryPath);

    SolutionContentReplacementResult ReplaceSolutionContent(string sourceSolutionRoot, string targetSolutionRoot);
}

internal sealed class SolutionContentReplacementResult
{
    public string? RetainedBackupPath { get; init; }

    public Exception? BackupCleanupException { get; init; }
}

internal sealed class SolutionContentReplacementException : IOException
{
    public SolutionContentReplacementException(string message, string backupPath, Exception innerException)
        : base(message, innerException)
    {
        BackupPath = backupPath;
    }

    public string BackupPath { get; }
}

[Export(typeof(IDataverseSolutionProjectFileService))]
internal sealed class DataverseSolutionProjectFileService : IDataverseSolutionProjectFileService
{
    internal const string ProjectSdk = "AlbanianXrm.CDSProj.Sdk/1.0.11";

    public string FindGeneratedSolutionRoot(string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
        {
            throw new InvalidOperationException($"PAC CLI did not create the output directory '{outputDirectory}'.");
        }

        var manifests = Directory
            .EnumerateFiles(outputDirectory, "Solution.xml", SearchOption.AllDirectories)
            .Where(path => string.Equals(Path.GetFileName(Path.GetDirectoryName(path)), "Other", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (manifests.Length != 1)
        {
            throw new InvalidOperationException($"Expected one generated Solution.xml under '{outputDirectory}', but found {manifests.Length}.");
        }

        return Directory.GetParent(Path.GetDirectoryName(manifests[0])!)!.FullName;
    }

    public string CopyGeneratedProjectDirectory(string outputDirectory, string projectDirectory)
    {
        var outputPath = Path.GetFullPath(outputDirectory);
        var destinationPath = Path.GetFullPath(projectDirectory);
        var solutionRoot = FindGeneratedSolutionRoot(outputPath);
        var generatedProjectPath = Directory.GetParent(solutionRoot)?.FullName
            ?? throw new InvalidOperationException($"Could not determine the generated project directory from '{solutionRoot}'.");

        if (string.Equals(generatedProjectPath, destinationPath, StringComparison.OrdinalIgnoreCase))
        {
            return destinationPath;
        }

        if (Directory.Exists(destinationPath) || File.Exists(destinationPath))
        {
            throw new InvalidOperationException($"The project path '{destinationPath}' already exists.");
        }

        try
        {
            CopyDirectory(generatedProjectPath, destinationPath);
        }
        catch (Exception copyException)
        {
            var cleanupException = TryDeleteDirectory(destinationPath);
            var message = $"Copying the PAC-generated project failed. The original output was retained at '{generatedProjectPath}'.";
            if (cleanupException is not null)
            {
                throw new IOException(
                    $"{message} The partial destination at '{destinationPath}' could not be removed.",
                    new AggregateException(copyException, cleanupException));
            }

            throw new IOException(message, copyException);
        }

        return destinationPath;
    }

    public string FinalizeProjectFile(
        string projectDirectory,
        string projectName,
        DataverseSolutionProjectSdk projectSdk)
    {
        if (projectSdk == DataverseSolutionProjectSdk.AlbanianXrm)
        {
            return WriteMinimalProjectFile(projectDirectory, projectName);
        }

        if (projectSdk != DataverseSolutionProjectSdk.MicrosoftPowerApps)
        {
            throw new ArgumentOutOfRangeException(nameof(projectSdk), projectSdk, "Unsupported Dataverse solution project SDK.");
        }

        var generatedProjectFiles = Directory
            .EnumerateFiles(projectDirectory, "*.cdsproj", SearchOption.TopDirectoryOnly)
            .ToArray();
        if (generatedProjectFiles.Length != 1)
        {
            throw new InvalidOperationException(
                $"Expected one PAC-generated .cdsproj file in '{projectDirectory}', but found {generatedProjectFiles.Length}.");
        }

        return generatedProjectFiles[0];
    }

    public string WriteMinimalProjectFile(string projectDirectory, string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectDirectory))
        {
            throw new ArgumentException("The project directory is required.", nameof(projectDirectory));
        }

        if (string.IsNullOrWhiteSpace(projectName)
            || projectName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("The project name is not a valid file name.", nameof(projectName));
        }

        Directory.CreateDirectory(projectDirectory);
        foreach (var generatedProjectFile in Directory.EnumerateFiles(projectDirectory, "*.cdsproj", SearchOption.TopDirectoryOnly))
        {
            File.Delete(generatedProjectFile);
        }

        var projectFilePath = Path.Combine(projectDirectory, projectName + ".cdsproj");
        File.WriteAllText(
            projectFilePath,
            $"<Project Sdk=\"{ProjectSdk}\">{Environment.NewLine}</Project>{Environment.NewLine}");
        return projectFilePath;
    }

    public SolutionContentReplacementResult ReplaceSolutionContent(string sourceSolutionRoot, string targetSolutionRoot)
    {
        var sourcePath = Path.GetFullPath(sourceSolutionRoot);
        var targetPath = Path.GetFullPath(targetSolutionRoot);
        if (!Directory.Exists(sourcePath))
        {
            throw new DirectoryNotFoundException($"The cloned solution content was not found at '{sourcePath}'.");
        }

        if (string.Equals(sourcePath.TrimEnd(Path.DirectorySeparatorChar), targetPath.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The cloned and target solution directories must be different.");
        }

        var targetParent = Path.GetDirectoryName(targetPath)
            ?? throw new InvalidOperationException($"Could not determine the parent directory of '{targetPath}'.");
        Directory.CreateDirectory(targetParent);

        var operationId = Guid.NewGuid().ToString("N").Substring(0, 12);
        var backupPath = Path.Combine(targetParent, $".xrb-{operationId}");
        var backupCreated = false;
        try
        {
            if (Directory.Exists(targetPath))
            {
                CopyDirectory(targetPath, backupPath);
                backupCreated = true;
            }
            else
            {
                Directory.CreateDirectory(targetPath);
            }

            ClearDirectoryContents(targetPath);
            CopyDirectory(sourcePath, targetPath);
        }
        catch (Exception replacementException)
        {
            if (backupCreated)
            {
                try
                {
                    ClearDirectoryContents(targetPath);
                    CopyDirectory(backupPath, targetPath);
                }
                catch (Exception rollbackException)
                {
                    throw new SolutionContentReplacementException(
                        $"Replacing solution content failed, and the original content could not be restored. The backup was retained at '{backupPath}'.",
                        backupPath,
                        new AggregateException(replacementException, rollbackException));
                }

                throw new SolutionContentReplacementException(
                    $"Replacing solution content failed. The original content was restored, and a backup was retained at '{backupPath}'.",
                    backupPath,
                    replacementException);
            }

            throw;
        }

        if (backupCreated)
        {
            var cleanupException = TryDeleteDirectory(backupPath);
            if (cleanupException is not null)
            {
                return new SolutionContentReplacementResult
                {
                    RetainedBackupPath = backupPath,
                    BackupCleanupException = cleanupException
                };
            }
        }

        return new SolutionContentReplacementResult();
    }

    public Exception? TryDeleteDirectory(string directoryPath)
    {
        const int maximumAttempts = 3;
        Exception? lastException = null;
        for (var attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            try
            {
                DeleteDirectory(ToExtendedPath(directoryPath));
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                lastException = ex;
                if (attempt < maximumAttempts)
                {
                    System.Threading.Thread.Sleep(100 * attempt);
                }
            }
        }

        return lastException;
    }

    private static void ClearDirectoryContents(string directoryPath)
    {
        var extendedDirectoryPath = ToExtendedPath(directoryPath);
        foreach (var filePath in Directory.EnumerateFiles(extendedDirectoryPath))
        {
            File.SetAttributes(filePath, FileAttributes.Normal);
            File.Delete(filePath);
        }

        foreach (var childDirectoryPath in Directory.EnumerateDirectories(extendedDirectoryPath))
        {
            DeleteDirectory(childDirectoryPath);
        }
    }

    private static void DeleteDirectory(string directoryPath)
    {
        foreach (var filePath in Directory.EnumerateFiles(directoryPath))
        {
            File.SetAttributes(filePath, FileAttributes.Normal);
            File.Delete(filePath);
        }

        foreach (var childDirectoryPath in Directory.EnumerateDirectories(directoryPath))
        {
            DeleteDirectory(childDirectoryPath);
        }

        Directory.Delete(directoryPath, recursive: false);
    }

    private static void CopyDirectory(string sourcePath, string destinationPath)
    {
        var extendedSourcePath = ToExtendedPath(sourcePath);
        var extendedDestinationPath = ToExtendedPath(destinationPath);
        Directory.CreateDirectory(extendedDestinationPath);
        foreach (var filePath in Directory.EnumerateFiles(extendedSourcePath))
        {
            File.Copy(filePath, Path.Combine(extendedDestinationPath, Path.GetFileName(filePath)));
        }

        foreach (var directoryPath in Directory.EnumerateDirectories(extendedSourcePath))
        {
            CopyDirectory(directoryPath, Path.Combine(extendedDestinationPath, Path.GetFileName(directoryPath)));
        }
    }

    private static string ToExtendedPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (fullPath.StartsWith(@"\\?\", StringComparison.Ordinal))
        {
            return fullPath;
        }

        return fullPath.StartsWith(@"\\", StringComparison.Ordinal)
            ? @"\\?\UNC\" + fullPath.Substring(2)
            : @"\\?\" + fullPath;
    }
}
#nullable restore
