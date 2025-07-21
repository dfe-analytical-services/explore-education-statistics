using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

/**
 * Represents a ZIP file whose main zip stream and entry stream accesses
 * and disposals are managed so that the consumer does not need to manage
 * them.
 */
public interface IManagedStreamZipFile : IAsyncDisposable
{
    List<IManagedStreamZipEntry> GetEntries();
}

/**
 * Represents a ZIP file entry whose stream accesses and disposals are
 * managed so that the consumer does not need to manage them.
 */
public interface IManagedStreamZipEntry
{
    string Name { get; }

    long Length { get; }

    Stream Open();
}

/**
 * A class that provides information about a ZIP form file and also manages its
 * stream access and disposals and those of its entries.
 */
public class ManagedStreamZipEntry(ManagedStreamZipFormFile zipFormFile, ZipArchiveEntry entry) : IManagedStreamZipEntry
{
    public string Name { get; } = entry.Name;

    public long Length { get; } = entry.Length;

    public Stream Open()
    {
        return zipFormFile.OpenEntry(entry);
    }
}

public class ManagedStreamZipFormFile : IManagedStreamZipFile
{
    private readonly ZipArchive _archive;
    private readonly Stream _sourceStream;
    private readonly List<Stream> _openEntryStreams = new();

    public ManagedStreamZipFormFile(IFormFile formFile)
    {
        _sourceStream = formFile.OpenReadStream();
        _archive = new ZipArchive(_sourceStream, ZipArchiveMode.Read, leaveOpen: false);
    }

    public List<IManagedStreamZipEntry> GetEntries()
    {
        return _archive
            .Entries
            .Select(entry => new ManagedStreamZipEntry(this, entry))
            .OfType<IManagedStreamZipEntry>()
            .ToList();
    }

    internal Stream OpenEntry(ZipArchiveEntry entry)
    {
        var stream = entry.Open();
        _openEntryStreams.Add(stream);
        return stream;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var stream in _openEntryStreams)
        {
            await stream.DisposeAsync();
        }

        await _sourceStream.DisposeAsync();
        _archive.Dispose();
    }
}
