using System.Globalization;
using System.Text.RegularExpressions;

namespace BLML.Phase3FormsUI.Resources;

public sealed class ResourceExtractor
{
    public sealed class BinaryResourceReference
    {
        public string FileName { get; init; } = string.Empty;

        public int Offset { get; init; }
    }

    private static readonly Regex ResourceReferenceRegex = new(@"^(?<FileName>[^:]+):(?<Offset>[0-9A-Fa-f]+)$", RegexOptions.Compiled);

    public bool TryParseResourceReference(string value, out BinaryResourceReference? reference)
    {
        var match = ResourceReferenceRegex.Match(value.Trim());
        if (!match.Success)
        {
            reference = null;
            return false;
        }

        reference = new BinaryResourceReference
        {
            FileName = match.Groups["FileName"].Value,
            Offset = int.Parse(match.Groups["Offset"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
        };

        return true;
    }

    public byte[] ParseHexPayload(string value)
    {
        var normalized = value.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal);

        return Convert.FromHexString(normalized);
    }

    public byte[] ExtractBinaryResource(string rootFolder, string resourceReference)
    {
        if (!TryParseResourceReference(resourceReference, out var reference) || reference is null)
        {
            throw new ArgumentException("Invalid resource reference.", nameof(resourceReference));
        }

        var resourcePath = Path.Combine(rootFolder, reference.FileName);
        return ExtractBinaryResource(resourcePath, reference.Offset);
    }

    public byte[] ExtractBinaryResource(string resourcePath, int offset)
    {
        using var stream = File.OpenRead(resourcePath);
        if (offset < 0 || offset > stream.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        stream.Position = offset;
        using var reader = new BinaryReader(stream);

        if (stream.Length - offset >= sizeof(int))
        {
            var length = reader.ReadInt32();
            if (length >= 0 && length <= stream.Length - stream.Position)
            {
                return reader.ReadBytes(length);
            }

            stream.Position = offset;
        }

        return reader.ReadBytes((int)(stream.Length - stream.Position));
    }

    public void ExportBinaryResource(string rootFolder, string resourceReference, string outputPath)
    {
        var bytes = ExtractBinaryResource(rootFolder, resourceReference);
        File.WriteAllBytes(outputPath, bytes);
    }
}
