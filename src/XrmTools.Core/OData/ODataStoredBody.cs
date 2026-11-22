#nullable enable
namespace XrmTools.OData;

using System;
using System.IO;
using System.Text;

// The response pane and named capture each own a lease. Open readers survive eviction.
internal sealed class ODataStoredBody
{
    private readonly string path;
    private readonly Encoding encoding;
    private int owners = 1;

    public ODataStoredBody(string path, Encoding encoding)
    {
        this.path = path;
        this.encoding = encoding;
    }

    public ODataStoredBody Retain()
    {
        lock (this)
        {
            if (owners == 0) throw new ObjectDisposedException(nameof(ODataStoredBody));
            owners++;
            return this;
        }
    }

    public Stream Open()
    {
        lock (this)
        {
            if (owners == 0) throw new FormatException("The captured response has expired. Send its named request again.");
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
        }
    }

    public TextReader OpenText() => new StreamReader(Open(), encoding, detectEncodingFromByteOrderMarks: true);

    public void Release()
    {
        lock (this)
        {
            if (--owners != 0) return;
            try { File.Delete(path); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
