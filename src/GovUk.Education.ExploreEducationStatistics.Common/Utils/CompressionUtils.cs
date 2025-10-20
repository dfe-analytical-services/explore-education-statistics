using System.IO.Compression;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using ZstdSharp;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class CompressionUtils
{
    public static Stream GetCompressionStream(
        Stream targetStream,
        string contentEncoding,
        CompressionMode compressionMode
    )
    {
        switch (contentEncoding)
        {
            case ContentEncodings.Gzip:
            {
                return new GZipStream(targetStream, compressionMode, leaveOpen: true);
            }
            case ContentEncodings.Zstd:
            {
                switch (compressionMode)
                {
                    case CompressionMode.Compress:
                        return new CompressionStream(targetStream, level: 11, leaveOpen: true);
                    case CompressionMode.Decompress:
                        return new DecompressionStream(targetStream, leaveOpen: true);
                    default:
                        throw new ArgumentException($"Unsupported compression mode {compressionMode}");
                }
            }
            default:
                throw new NotSupportedException($"Content encoding {contentEncoding} is not supported");
        }
    }

    public static async Task CompressToStream(
        Stream stream,
        Stream targetStream,
        string contentEncoding,
        CancellationToken cancellationToken = default
    )
    {
        stream.SeekToBeginning();

        switch (contentEncoding)
        {
            case ContentEncodings.Gzip:
            {
                await using var compressor = new GZipStream(targetStream, CompressionMode.Compress, leaveOpen: true);
                await stream.CopyToAsync(compressor, cancellationToken);
                break;
            }
            case ContentEncodings.Zstd:
            {
                await using var compressor = new CompressionStream(targetStream, level: 11, leaveOpen: true);
                await stream.CopyToAsync(compressor, cancellationToken);
                break;
            }
            default:
                throw new NotSupportedException($"Content encoding {contentEncoding} is not supported");
        }

        targetStream.SeekToBeginning();
    }

    public static async Task DecompressToStream(
        Stream stream,
        Stream targetStream,
        string contentEncoding,
        CancellationToken cancellationToken = default
    )
    {
        stream.SeekToBeginning();

        switch (contentEncoding)
        {
            case ContentEncodings.Gzip:
            {
                await using var decompressor = new GZipStream(stream, CompressionMode.Decompress);
                await decompressor.CopyToAsync(targetStream, cancellationToken);
                break;
            }
            case ContentEncodings.Zstd:
            {
                await using var decompressor = new DecompressionStream(stream);
                await decompressor.CopyToAsync(targetStream, cancellationToken);
                break;
            }
            default:
                throw new NotSupportedException($"Content encoding {contentEncoding} is not supported");
        }

        targetStream.SeekToBeginning();
    }

    public static async Task<string> DecompressToString(
        byte[] bytes,
        string contentEncoding,
        CancellationToken cancellationToken = default
    )
    {
        switch (contentEncoding)
        {
            case ContentEncodings.Gzip:
            {
                await using var stream = new MemoryStream(bytes);
                await using var targetStream = new MemoryStream();
                await DecompressToStream(stream, targetStream, contentEncoding, cancellationToken);
                return targetStream.ReadToEnd();
            }
            case ContentEncodings.Zstd:
            {
                using var decompressor = new Decompressor();
                return Encoding.UTF8.GetString(decompressor.Unwrap(bytes));
            }
            default:
                throw new NotSupportedException($"Content encoding {contentEncoding} is not supported");
        }
    }
}
