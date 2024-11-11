namespace XrmTools.Core.Tests.Tokens;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using XrmTools.Tokens;

[TestClass]
public class CredentialTokenExpanderTest
{
    [TestMethod]
    public void Expand_WhenTokenIsCredentialToken_ShouldReturnExpandedValue()
    {
        // Arrange
        var credentialManagerMock = new Mock<ICredentialManager>();
        var tokenExpander = new CredentialTokenExpander(credentialManagerMock.Object);
        var target = "MyApp";
        var username = "testuser";
        var password = "testpassword";
        credentialManagerMock.Setup(x => x.ReadCredentials(target)).Returns((username, password));
        var token = "cred:MyApp:username";

        // Act
        var expandedValue = tokenExpander.Expand(token);

        // Assert
        expandedValue.Should().Be(username);
    }

    [TestMethod]
    public void Expand_WhenTokenIsNotCredentialToken_ShouldReturnSameToken()
    {
        // Arrange
        var credentialManagerMock = new Mock<ICredentialManager>();
        var tokenExpander = new CredentialTokenExpander(credentialManagerMock.Object);
        var token = "otherToken";

        // Act
        var expandedValue = tokenExpander.Expand(token);

        // Assert
        expandedValue.Should().Be(token);
    }

    [TestMethod]
    public void CanExpand_WhenTokenIsCredentialToken_ShouldReturnTrue()
    {
        // Arrange
        var credentialManagerMock = new Mock<ICredentialManager>();
        var tokenExpander = new CredentialTokenExpander(credentialManagerMock.Object);
        var token = "cred:MyApp:username";

        // Act
        var canExpand = tokenExpander.CanExpand(token);

        // Assert
        canExpand.Should().BeTrue();
    }

    [TestMethod]
    public void CanExpand_WhenTokenIsNotCredentialToken_ShouldReturnFalse()
    {
        // Arrange
        var credentialManagerMock = new Mock<ICredentialManager>();
        var tokenExpander = new CredentialTokenExpander(credentialManagerMock.Object);
        var token = "otherToken";

        // Act
        var canExpand = tokenExpander.CanExpand(token);

        // Assert
        canExpand.Should().BeFalse();
    }
}
