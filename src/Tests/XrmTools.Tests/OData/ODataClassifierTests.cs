namespace XrmTools.Tests.OData;

using System.Linq;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Moq;
using XrmTools.OData;
using Xunit;

public class ODataClassifierTests
{
    [Fact]
    public void ReusesSnapshotAndReclassifiesWhenHeaderBodyBoundaryChanges()
    {
        var buffer = new Mock<ITextBuffer>();
        var registry = new Mock<IClassificationTypeRegistryService>();
        var identifier = new Mock<IClassificationType>().Object;
        var other = new Mock<IClassificationType>().Object;
        registry.Setup(r => r.GetClassificationType(It.IsAny<string>()))
            .Returns((string name) => name == PredefinedClassificationTypeNames.Identifier ? identifier : other);
        var classifier = new ODataClassifier(buffer.Object, registry.Object);
        string prefix = string.Concat(Enumerable.Repeat("### Section\nGET /accounts\n\n", 500));
        string headers = prefix + "### Last\nGET /accounts\nAccept: json";
        string body = prefix + "### Last\nGET /accounts\n\nAccept: json";
        var first = Snapshot(headers);
        var second = Snapshot(body);
        int changes = 0;
        classifier.ClassificationChanged += (_, e) => { changes++; Assert.Same(second.Object, e.ChangeSpan.Snapshot); };
        var span = new SnapshotSpan(first.Object, headers.LastIndexOf("Accept"), 6);
        Assert.Equal(identifier, Assert.Single(classifier.GetClassificationSpans(span)).ClassificationType);
        Assert.Equal(identifier, Assert.Single(classifier.GetClassificationSpans(span)).ClassificationType);
        first.Verify(s => s.GetText(), Times.Once);
        buffer.Raise(b => b.Changed += null, new TextContentChangedEventArgs(first.Object, second.Object, EditOptions.None, null));
        Assert.Equal(1, changes);
        Assert.Empty(classifier.GetClassificationSpans(new SnapshotSpan(second.Object, body.LastIndexOf("Accept"), 6)));
        second.Verify(s => s.GetText(), Times.Once);
    }

    private static Mock<ITextSnapshot> Snapshot(string text)
    {
        var snapshot = new Mock<ITextSnapshot>();
        snapshot.SetupGet(s => s.Length).Returns(text.Length);
        snapshot.Setup(s => s.GetText()).Returns(text);
        return snapshot;
    }
}
