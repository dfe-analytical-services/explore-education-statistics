#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;

public static class FileExtensions
{
    public static readonly List<FileType> PublicFileTypes = new()
    {
        Ancillary,
        Chart,
        Data,
        Image,
        DataGuidance
    };

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum FileSizeUnit : byte
    {
        B,
        Kb,
        Mb,
        Gb,
        Tb
    }

    public static string DisplaySize(this File file)
    {
        return DisplaySize(file.ContentLength);
    }

    public static string DisplaySize(this long contentLength)
    {
        var unit = FileSizeUnit.B;
        while (contentLength >= 1024 && unit < FileSizeUnit.Tb)
        {
            contentLength /= 1024;
            unit++;
        }

        return $"{contentLength:0.##} {unit}";
    }

    public static string Path(this File file)
    {
        return $"{FilesPath(file.RootPath, file.Type)}{file.Id}";
    }

    public static string Path(Guid rootPath, FileType type, Guid fileId)
    {
        return $"{FilesPath(rootPath, type)}{fileId}";
    }

    public static string PublicPath(this File file, Guid releaseVersionId)
    {
        if (!PublicFileTypes.Contains(file.Type))
        {
            throw new ArgumentOutOfRangeException(nameof(file.Type), file.Type, "Cannot create public path for file type");
        }

        // Public release files are located in blob storage under the latest release version in path /<releaseVersionId>/<type>/<fileId>.
        // This is not necessarily the same release version that they were originally created for at the time of uploading.
        return $"{FilesPath(releaseVersionId, file.Type)}{file.Id}";
    }

    public static string ZipFileEntryName(this File file)
    {
        return file.Type switch
        {
            Ancillary => $"supporting-files/{file.Filename}",
            Data => $"data/{AddTimestampToFileName(file.Filename)}",
            _ => throw new ArgumentOutOfRangeException(nameof(file.Type), "Unexpected file type"),
        };
    }

    private static string? AddTimestampToFileName(string fileName)
    {
        var fileNamePart = System.IO.Path.GetFileNameWithoutExtension(fileName);
        var fileExtensionPart = System.IO.Path.GetExtension(fileName);

        return $"{fileNamePart}{Stopwatch.GetTimestamp()}{fileExtensionPart}";
    }
}
