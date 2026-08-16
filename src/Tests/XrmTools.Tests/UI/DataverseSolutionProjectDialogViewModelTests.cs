#nullable enable
namespace XrmTools.Tests.UI;

using FluentAssertions;
using System;
using System.IO;
using Xunit;
using XrmTools.DataverseSolutions;
using XrmTools.UI;
using XrmTools.WebApi.Entities;

public sealed class DataverseSolutionProjectDialogViewModelTests
{
    [Fact]
    public void TryCreateRequest_CloneMode_UsesSelectedSolutionUniqueName()
    {
        var parentDirectory = CreateTemporaryDirectory();
        try
        {
            var viewModel = new DataverseSolutionProjectDialogViewModel(parentDirectory)
            {
                ProjectName = "IgnoredProjectName"
            };
            viewModel.SetSolutions(
            [
                new Solution
                {
                    FriendlyName = "Contoso Core",
                    UniqueName = "ContosoCore"
                }
            ]);

            var succeeded = viewModel.TryCreateRequest(out var request, out var validationError);

            succeeded.Should().BeTrue();
            validationError.Should().BeNull();
            request.Should().NotBeNull();
            request!.Mode.Should().Be(DataverseSolutionProjectCreationMode.Clone);
            request.ProjectName.Should().Be("ContosoCore");
            request.SolutionUniqueName.Should().Be("ContosoCore");
            viewModel.CloneDestinationPath.Should().Be(Path.Combine(parentDirectory, "ContosoCore"));
        }
        finally
        {
            Directory.Delete(parentDirectory, recursive: true);
        }
    }

    [Fact]
    public void CloneDestinationPath_ChangesWithSelectedSolutionAndLocation()
    {
        var viewModel = new DataverseSolutionProjectDialogViewModel(@"C:\Projects");
        var firstSolution = new Solution { UniqueName = "FirstSolution" };
        var secondSolution = new Solution { UniqueName = "SecondSolution" };
        viewModel.SetSolutions([firstSolution, secondSolution]);

        viewModel.CloneDestinationPath.Should().Be(@"C:\Projects\FirstSolution");

        viewModel.SelectedSolution = secondSolution;
        viewModel.ParentDirectory = @"D:\Source";

        viewModel.CloneDestinationPath.Should().Be(@"D:\Source\SecondSolution");
    }

    [Fact]
    public void TryCreateRequest_EmptyMode_UsesEnteredProjectName()
    {
        var parentDirectory = CreateTemporaryDirectory();
        try
        {
            var viewModel = new DataverseSolutionProjectDialogViewModel(parentDirectory)
            {
                CreateEmptyProject = true,
                ProjectName = "NewSolution",
                PublisherName = "Contoso",
                PublisherPrefix = "contoso"
            };

            var succeeded = viewModel.TryCreateRequest(out var request, out var validationError);

            succeeded.Should().BeTrue();
            validationError.Should().BeNull();
            request.Should().NotBeNull();
            request!.Mode.Should().Be(DataverseSolutionProjectCreationMode.Empty);
            request.ProjectName.Should().Be("NewSolution");
            request.SolutionUniqueName.Should().BeNull();
        }
        finally
        {
            Directory.Delete(parentDirectory, recursive: true);
        }
    }

    [Fact]
    public void TryCreateRequest_CloneMode_RejectsExistingSolutionFolder()
    {
        var parentDirectory = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(parentDirectory, "ContosoCore"));
            var viewModel = new DataverseSolutionProjectDialogViewModel(parentDirectory);
            viewModel.SetSolutions([new Solution { UniqueName = "ContosoCore" }]);

            var succeeded = viewModel.TryCreateRequest(out var request, out var validationError);

            succeeded.Should().BeFalse();
            request.Should().BeNull();
            validationError.Should().Contain("ContosoCore");
        }
        finally
        {
            Directory.Delete(parentDirectory, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "XrmTools.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
#nullable restore
