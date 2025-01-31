#nullable enable
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;
using ZstdSharp;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public class CompressionUtilsTests
{
    private const string Content = "Test content";

    [Fact]
    public async Task CompressToStream_UnsupportedContentEncoding()
    {
        await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await CompressionUtils.CompressToStream(
                stream: Content.ToStream(),
                targetStream: new MemoryStream(),
                contentEncoding: "other"));
    }

    [Fact]
    public async Task CompressToStream_Gzip()
    {
        // Compress some content to a stream using the method under test
        await using var compressedStream = new MemoryStream();
        await CompressionUtils.CompressToStream(
            stream: Content.ToStream(),
            targetStream: compressedStream,
            contentEncoding: ContentEncodings.Gzip);

        Assert.Equal(0, compressedStream.Position);
        Assert.True(compressedStream.Length > 0);
        Assert.NotEqual(Content.ToStream(), compressedStream);

        // Decompress the stream using the compression class directly and assert it contains the original content
        await using var decompressedStream = new MemoryStream();
        await using (var decompressor = new GZipStream(compressedStream, CompressionMode.Decompress))
        {
            await decompressor.CopyToAsync(decompressedStream);
        }

        decompressedStream.SeekToBeginning();
        Assert.Equal(Content, decompressedStream.ReadToEnd());
    }

    [Fact]
    public async Task CompressToStream_Zstandard()
    {
        // Compress some content to a stream using the method under test
        await using var compressedStream = new MemoryStream();
        await CompressionUtils.CompressToStream(
            stream: Content.ToStream(),
            targetStream: compressedStream,
            contentEncoding: ContentEncodings.Zstd);

        Assert.Equal(0, compressedStream.Position);
        Assert.True(compressedStream.Length > 0);
        Assert.NotEqual(Content.ToStream(), compressedStream);

        // Decompress the stream using the compression class directly and assert it contains the original content
        await using var decompressedStream = new MemoryStream();
        await using (var decompressor = new DecompressionStream(compressedStream))
        {
            await decompressor.CopyToAsync(decompressedStream);
        }

        decompressedStream.SeekToBeginning();
        Assert.Equal(Content, decompressedStream.ReadToEnd());
    }

    [Fact]
    public async Task DecompressToStream_UnsupportedContentEncoding()
    {
        await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await CompressionUtils.DecompressToStream(
                stream: Content.ToStream(),
                targetStream: new MemoryStream(),
                contentEncoding: "other"));
    }

    [Fact]
    public async Task DecompressToStream_Gzip()
    {
        // Compress some content to a stream using the compression class directly
        await using var compressedStream = new MemoryStream();
        await using (var compressor = new GZipStream(compressedStream, CompressionMode.Compress, leaveOpen: true))
        {
            compressor.WriteText(Content);
        }

        // Decompress the stream using the method under test and assert it contains the original content
        await using var decompressedStream = new MemoryStream();
        await CompressionUtils.DecompressToStream(
            stream: compressedStream,
            targetStream: decompressedStream,
            contentEncoding: ContentEncodings.Gzip);

        Assert.Equal(0, decompressedStream.Position);
        Assert.Equal(Content, decompressedStream.ReadToEnd());
    }

    [Fact]
    public async Task DecompressToStream_Zstandard()
    {
        // Compress some content to a stream using the compression class directly
        await using var compressedStream = new MemoryStream();
        await using (var compressor = new CompressionStream(compressedStream, level: 11, leaveOpen: true))
        {
            compressor.WriteText(Content);
        }

        // Decompress the stream using the method under test and assert it contains the original content
        await using var decompressedStream = new MemoryStream();
        await CompressionUtils.DecompressToStream(
            stream: compressedStream,
            targetStream: decompressedStream,
            contentEncoding: ContentEncodings.Zstd);

        Assert.Equal(0, decompressedStream.Position);
        Assert.Equal(Content, decompressedStream.ReadToEnd());
    }

    [Fact]
    public async Task DecompressToString_UnsupportedContentEncoding()
    {
        await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await CompressionUtils.DecompressToString(
                bytes: Encoding.UTF8.GetBytes(Content),
                contentEncoding: "other"));
    }

    [Fact]
    public async Task DecompressToString_Gzip()
    {
        // Compress some content to a byte array using the compression class directly
        await using var compressedStream = new MemoryStream();
        await using (var compressor = new GZipStream(compressedStream, CompressionMode.Compress, leaveOpen: true))
        {
            compressor.WriteText(Content);
        }

        // Decompress the byte array using the method under test
        var result = await CompressionUtils.DecompressToString(
            bytes: compressedStream.ToArray(),
            contentEncoding: ContentEncodings.Gzip);

        Assert.Equal(Content, result);
    }

    [Fact]
    public async Task DecompressToString_Zstandard()
    {
        // Compress some content to a byte array using the compression class directly
        await using var compressedStream = new MemoryStream();
        await using (var compressor = new CompressionStream(compressedStream, level: 11, leaveOpen: true))
        {
            compressor.WriteText(Content);
        }

        // Decompress the byte array using the method under test
        var result = await CompressionUtils.DecompressToString(
            bytes: compressedStream.ToArray(),
            contentEncoding: ContentEncodings.Zstd);

        Assert.Equal(Content, result);
    }
}
