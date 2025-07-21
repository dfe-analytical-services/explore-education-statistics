#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

/**
 * Represents a file whose Stream disposals are managed so that the
 * consumer does not need to manage disposals.
 */
public interface IManagedStreamFile : IAsyncDisposable
{
    string Name { get; }

    long Length { get; }

    Stream OpenStream();
}

/**
 * A class that provides information about a form file and also manages its
 * stream disposals.
 */
public class ManagedStreamFormFile(IFormFile formFile) : IManagedStreamFile
{
    private readonly List<Stream> _openSourceStreams = [];

    public string Name { get; } = formFile.FileName;

    public long Length { get; } = formFile.Length;

    public Stream OpenStream()
    {
        var stream = formFile.OpenReadStream();
        _openSourceStreams.Add(stream);
        return stream;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var stream in _openSourceStreams)
        {
            await stream.DisposeAsync();
        }
    }
}
