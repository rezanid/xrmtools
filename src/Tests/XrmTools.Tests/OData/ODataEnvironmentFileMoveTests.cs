namespace XrmTools.Tests.OData;

using System;
using System.IO;
using System.Text.Json;
using XrmTools.OData;
using Xunit;

public class ODataEnvironmentFileMoveTests
{
    [Fact]
    public void FindsTheNearestFileAndFallsBackWhenItIsDeleted()
    {
        string root = CreateDirectory();
        string parent = Path.Combine(root, "parent");
        string child = Path.Combine(parent, "child");
        Directory.CreateDirectory(child);
        string document = Path.Combine(child, "requests.odata");
        string parentFile = Path.Combine(parent, "odata-client.env.json");
        string childFile = Path.Combine(child, "odata-client.env.json");
        File.WriteAllText(parentFile, "{}");

        try
        {
            Assert.Equal(parentFile, ODataEnvironmentFile.FindNearest(document));
            File.WriteAllText(childFile, "{}");
            Assert.Equal(childFile, ODataEnvironmentFile.FindNearest(document));
            File.Delete(childFile);
            Assert.Equal(parentFile, ODataEnvironmentFile.FindNearest(document));
            File.Delete(parentFile);
            Assert.Null(ODataEnvironmentFile.FindNearest(document));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void MoveUpdatesTheNearestAncestorAndPreservesOtherProfilesAndEntries()
    {
        string root = CreateDirectory();
        string parent = Path.Combine(root, "parent");
        string child = Path.Combine(parent, "child");
        Directory.CreateDirectory(child);
        string document = Path.Combine(child, "requests.odata");
        string sidecar = Path.Combine(parent, "odata-client.env.json");
        File.WriteAllText(sidecar, "{\"Development\":{\"existing\":\"keep\"},\"Test\":{\"account1\":\"test-value\"}}");

        try
        {
            var result = ODataEnvironmentFile.MoveVariable(document, "Development", "account1", "dev-value");
            Assert.Equal(ODataEnvironmentFileUpdateKind.Updated, result.Kind);
            Assert.Equal(sidecar, result.Path);
            Assert.False(File.Exists(Path.Combine(child, "odata-client.env.json")));
            using var json = JsonDocument.Parse(File.ReadAllText(sidecar));
            Assert.Equal("keep", json.RootElement.GetProperty("Development").GetProperty("existing").GetString());
            Assert.Equal("dev-value", json.RootElement.GetProperty("Development").GetProperty("account1").GetString());
            Assert.Equal("test-value", json.RootElement.GetProperty("Test").GetProperty("account1").GetString());
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void MoveCreatesASiblingAndAddsOrReusesSelectedProfileValues()
    {
        string root = CreateDirectory();
        string document = Path.Combine(root, "requests.odata");
        string sidecar = Path.Combine(root, "odata-client.env.json");
        try
        {
            var created = ODataEnvironmentFile.MoveVariable(document, "Development", "account1", "dev-value");
            Assert.Equal(ODataEnvironmentFileUpdateKind.Updated, created.Kind);
            Assert.Equal(sidecar, created.Path);
            var reused = ODataEnvironmentFile.MoveVariable(document, "Development", "account1", "dev-value");
            Assert.Equal(ODataEnvironmentFileUpdateKind.AlreadyPresent, reused.Kind);
            using var json = JsonDocument.Parse(File.ReadAllText(sidecar));
            Assert.Equal("dev-value", json.RootElement.GetProperty("Development").GetProperty("account1").GetString());
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void ConflictAndFailedWriteLeaveTheSidecarAndDeclarationUnchanged()
    {
        string root = CreateDirectory();
        string conflictDirectory = Path.Combine(root, "conflict");
        string failedDirectory = Path.Combine(root, "failed");
        Directory.CreateDirectory(conflictDirectory);
        Directory.CreateDirectory(failedDirectory);
        string conflictDocument = Path.Combine(conflictDirectory, "requests.odata");
        string conflictSidecar = Path.Combine(conflictDirectory, "odata-client.env.json");
        string failedDocument = Path.Combine(failedDirectory, "requests.odata");
        string failedSidecar = Path.Combine(failedDirectory, "odata-client.env.json");
        const string declaration = "@account1 = local-value";
        File.WriteAllText(conflictDocument, declaration);
        File.WriteAllText(failedDocument, declaration);
        File.WriteAllText(conflictSidecar, "{\"Development\":{\"account1\":\"existing-value\"}}");
        Directory.CreateDirectory(failedSidecar);
        try
        {
            var conflict = ODataEnvironmentFile.MoveVariable(conflictDocument, "Development", "account1", "local-value");
            Assert.Equal(ODataEnvironmentFileUpdateKind.Conflict, conflict.Kind);
            Assert.Equal("{\"Development\":{\"account1\":\"existing-value\"}}", File.ReadAllText(conflictSidecar));

            var failed = ODataEnvironmentFile.MoveVariable(failedDocument, "Development", "account1", "local-value");
            Assert.Equal(ODataEnvironmentFileUpdateKind.Failed, failed.Kind);
            Assert.True(Directory.Exists(failedSidecar));
            Assert.Equal(declaration, File.ReadAllText(failedDocument));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private static string CreateDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "XrmTools-OData-Move-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
