namespace XrmTools.Core.Tests.Tokens;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using XrmTools.Tokens;

[TestClass]
public class EnvironmentTokenExpanderTests
{
    [TestMethod]
    public void CanExpand_WhenTokenIsEnvironment_ReturnsTrue()
    {
        // Arrange
        var tokenExpander = new EnvironmentTokenExpander();

        // Act
        var result = tokenExpander.CanExpand("env:something");

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void CanExpand_WhenTokenIsNotEnvironment_ReturnsFalse()
    {
        // Arrange
        var tokenExpander = new EnvironmentTokenExpander();

        // Act
        var result = tokenExpander.CanExpand("invalidtoken");

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Expand_WhenTokenIsEnvironment_ReturnsEnvironmentVariableValue()
    {
        // Arrange
        var tokenExpander = new EnvironmentTokenExpander();
        var environmentVariableName = "MyVariable";
        var environmentVariableValue = "MyValue";
        Environment.SetEnvironmentVariable(environmentVariableName, environmentVariableValue);

        // Act
        var result = tokenExpander.Expand("env:MyVariable");

        // Assert
        result.Should().Be(environmentVariableValue);

        // Cleanup
        Environment.SetEnvironmentVariable(environmentVariableName, null);
    }

    [TestMethod]
    public void Expand_WhenTokenIsNotEnvironment_ReturnsSameToken()
    {
        // Arrange
        var tokenExpander = new EnvironmentTokenExpander();
        var token = "otherToken";

        // Act
        var result = tokenExpander.Expand(token);

        // Assert
        result.Should().Be(token);
    }
}
