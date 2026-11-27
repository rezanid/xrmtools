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
        var keyword = new Mock<IClassificationType>().Object;
        var other = new Mock<IClassificationType>().Object;
        registry.Setup(r => r.GetClassificationType(It.IsAny<string>()))
            .Returns((string name) => name == PredefinedClassificationTypeNames.Keyword ? keyword : other);
        var classifier = new ODataClassifier(buffer.Object, registry.Object);
        string prefix = string.Concat(Enumerable.Repeat("### Section\nGET /accounts\n\n", 500));
        string headers = prefix + "### Last\nGET /accounts\nAccept: json";
        string body = prefix + "### Last\nGET /accounts\n\nAccept: json";
        var first = Snapshot(headers);
        var second = Snapshot(body);
        int changes = 0;
        classifier.ClassificationChanged += (_, e) => { changes++; Assert.Same(second.Object, e.ChangeSpan.Snapshot); };
        var span = new SnapshotSpan(first.Object, headers.LastIndexOf("Accept"), 6);
        Assert.Equal(keyword, Assert.Single(classifier.GetClassificationSpans(span)).ClassificationType);
        Assert.Equal(keyword, Assert.Single(classifier.GetClassificationSpans(span)).ClassificationType);
        first.Verify(s => s.GetText(), Times.Once);
        buffer.Raise(b => b.Changed += null, new TextContentChangedEventArgs(first.Object, second.Object, EditOptions.None, null));
        Assert.Equal(1, changes);
        Assert.Empty(classifier.GetClassificationSpans(new SnapshotSpan(second.Object, body.LastIndexOf("Accept"), 6)));
        second.Verify(s => s.GetText(), Times.Once);
    }

    [Fact]
    public void UsesNativeStandardClassificationsAndPreservesHeadingAndDirectiveDistinctions()
    {
        const string text = "@id = one\n### Heading\n# @name request\nGET /a/{{id}} HTTP/1.1\nAccept: json\n// comment";
        var registry = new Mock<IClassificationTypeRegistryService>();
        registry.Setup(r => r.GetClassificationType(It.IsAny<string>())).Returns((string name) =>
        {
            var type = new Mock<IClassificationType>();
            type.SetupGet(t => t.Classification).Returns(name);
            return type.Object;
        });
        var classifier = new ODataClassifier(new Mock<ITextBuffer>().Object, registry.Object);
        var snapshot = Snapshot(text).Object;
        var spans = classifier.GetClassificationSpans(new SnapshotSpan(snapshot, 0, text.Length));
        Assert.Equal(new[]
        {
            ("@id", PredefinedClassificationTypeNames.SymbolDefinition),
            ("### Heading", PredefinedClassificationTypeNames.PreprocessorKeyword),
            ("# ", PredefinedClassificationTypeNames.Comment),
            ("@name", PredefinedClassificationTypeNames.Keyword),
            (" request", PredefinedClassificationTypeNames.Comment),
            ("GET", PredefinedClassificationTypeNames.MarkupNode),
            ("/a/", PredefinedClassificationTypeNames.Text),
            ("{{id}}", PredefinedClassificationTypeNames.MarkupAttribute),
            ("HTTP/1.1", PredefinedClassificationTypeNames.Type),
            ("Accept", PredefinedClassificationTypeNames.Keyword),
            ("json", PredefinedClassificationTypeNames.Literal),
            ("// comment", PredefinedClassificationTypeNames.Comment)
        }, spans.Select(s => (text.Substring(s.Span.Start.Position, s.Span.Length), s.ClassificationType.Classification)).ToArray());
    }

    private static Mock<ITextSnapshot> Snapshot(string text)
    {
        var snapshot = new Mock<ITextSnapshot>();
        snapshot.SetupGet(s => s.Length).Returns(text.Length);
        snapshot.Setup(s => s.GetText()).Returns(text);
        return snapshot;
    }
}
