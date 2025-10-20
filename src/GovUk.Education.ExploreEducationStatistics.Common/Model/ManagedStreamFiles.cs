using System.IO.Compression;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

/// <summary>
/// Represents a file whose Stream disposals are managed so that the
/// consumer does not need to manage disposals.
/// </summary>
public interface IManagedStreamFile : IAsyncDisposable
{
    string Name { get; }

    long Length { get; }

    Stream GetStream();
}

/// <summary>
/// A class that provides information about a form file and also manages its
/// stream disposals.
/// </summary>
public class ManagedStreamFormFile(IFormFile formFile) : IManagedStreamFile
{
    public string Name => formFile.FileName;

    public long Length => formFile.Length;

    /// <summary>
    /// Return the stream of the backing <see cref="IFormFile"/>. Subsequent calls to
    /// <see cref="GetStream"/> will return the same Stream, so Stream.SeekToBeginning()
    /// is recommended to be used.
    /// </summary>
    public Stream GetStream()
    {
        return formFile.OpenReadStream();
    }

    /// <summary>
    /// Dispose the backing <see cref="IFormFile"/>'s Stream.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await formFile.OpenReadStream().DisposeAsync();
    }
}

/**
 * A class that provides information about a ZIP form file and also manages its
 * stream access and disposals and those of its entries.
 */
public class ManagedStreamZipEntry(ZipArchiveEntry entry) : IManagedStreamFile
{
    private readonly List<Stream> _openStreams = [];

    public string Name => entry.Name;

    public long Length => entry.Length;

    /// <summary>
    /// Return a new compression Stream from the backing <see cref="ZipArchiveEntry"/>.
    /// Subsequent calls will return new compression Streams.
    /// The reason that we return new Streams upon each call is that
    /// DeflateStreams are forward-only and so Stream.SeekToBeginning()
    /// is not valid for use with them in order to return reading to the
    /// start of the Stream.
    /// </summary>
    public Stream GetStream()
    {
        var stream = entry.Open();
        _openStreams.Add(stream);
        return stream;
    }

    /// <summary>
    /// Dispose of any Streams opened from calls to <see cref="GetStream"/>.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        foreach (var stream in _openStreams)
        {
            await stream.DisposeAsync();
        }
    }
}

/**
 * Represents a ZIP file whose main zip stream and entry stream accesses
 * and disposals are managed so that the consumer does not need to manage
 * them.
 */
public interface IManagedStreamZipFile : IAsyncDisposable
{
    List<IManagedStreamFile> GetEntries();
}

public class ManagedStreamZipFormFile : IManagedStreamZipFile
{
    private readonly ZipArchive _archive;
    private readonly Stream _sourceStream;
    private List<IManagedStreamFile>? _entries;

    public ManagedStreamZipFormFile(IFormFile formFile)
    {
        _sourceStream = formFile.OpenReadStream();
        _archive = new ZipArchive(_sourceStream, ZipArchiveMode.Read, leaveOpen: false);
    }

    public List<IManagedStreamFile> GetEntries()
    {
        return _entries ??= _archive
            .Entries.Select(entry => new ManagedStreamZipEntry(entry))
            .OfType<IManagedStreamFile>()
            .ToList();
    }

    /// <summary>
    /// Dispose of the main <see cref="ZipArchive"/>'s Streams and also
    /// any of its <see cref="ZipArchiveEntry"/> children.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _sourceStream.DisposeAsync();
        _archive.Dispose();

        if (_entries != null)
        {
            foreach (var entry in _entries)
            {
                await entry.DisposeAsync();
            }
        }
    }
}
