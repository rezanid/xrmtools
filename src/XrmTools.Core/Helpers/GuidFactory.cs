#nullable enable
namespace XrmTools.Helpers;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

internal static class GuidFactory
{
    internal enum Namespace
    {
        PluginPackage = 0,
        PluginAssembly = 1,
        PluginType = 2,
        Step = 3,
        Image = 4,
        CustomApi = 5,
        CustomApiInput = 6,
        CustomApiOutput = 7
    }
    private static readonly Guid Namespace_PluginPackage = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Namespace_PluginAssembly = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid Namespace_PluginType = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid Namespace_Step = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid Namespace_Image = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid Namespace_CustommApi = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    private static readonly Guid Namespace_CustommApiInput = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
    private static readonly Guid Namespace_CustommApiOutput = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>
    /// Ensures that the Guid is set to a valid value. If not, it will be set to a deterministic Guid based on the namespace and name.
    /// </summary>
    public static Guid NewIfEmpty(this Guid? guid, Namespace ns, string name)
        => guid != null && guid.Value != Guid.Empty ? guid.Value : DeterministicGuid(ns, name);

    /// <summary>
    /// Generates a deterministic Guid based on the namespace and name.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">When an invalid <see cref="Namespace"/> is given.</exception>
    public static Guid DeterministicGuid(Namespace @namespace, string name) =>
        DeterministicGuid(@namespace switch
        {
            Namespace.PluginPackage => Namespace_PluginPackage,
            Namespace.PluginAssembly => Namespace_PluginAssembly,
            Namespace.PluginType => Namespace_PluginType,
            Namespace.Step => Namespace_Step,
            Namespace.Image => Namespace_Image,
            Namespace.CustomApi => Namespace_CustommApi,
            Namespace.CustomApiInput => Namespace_CustommApiInput,
            Namespace.CustomApiOutput => Namespace_CustommApiOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(@namespace))
        }, name);

    private static Guid DeterministicGuid(Guid ns, string name)
    {
        var namespaceBytes = ns.ToByteArray();
        SwapByteOrder(namespaceBytes);

        var nameBytes = Encoding.UTF8.GetBytes(name);
        var data = namespaceBytes.Concat(nameBytes).ToArray();

        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(data);

        hash[6] = (byte)((hash[6] & 0x0F) | 0x50); // Version 5
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // Variant

        var newGuid = new byte[16];
        Array.Copy(hash, 0, newGuid, 0, 16);
        SwapByteOrder(newGuid);

        return new Guid(newGuid);
    }

    private static void SwapByteOrder(byte[] guid)
    {
        void Swap(int a, int b) => (guid[a], guid[b]) = (guid[b], guid[a]);
        Swap(0, 3); Swap(1, 2); Swap(4, 5); Swap(6, 7);
    }
}
#nullable restore