#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class DataSetFileBuilder
{
    private string? _fileName;
    private bool _fileSizeIsZero;

    public async Task<FileDto> Build(FileType fileType)
    {
        if (fileType is not FileType.Data and not FileType.Metadata and not FileType.BulkDataZipIndex)
        {
            throw new NotSupportedException("Builder only accepts Data, Metadata and BulkDataZipIndex file types");
        }

        if (_fileName is null)
        {
            if (fileType is FileType.BulkDataZipIndex)
            {
                _fileName = "dataset_names.csv";
            }
            else
            {
                _fileName = fileType == FileType.Metadata
                    ? "test-data.meta.csv"
                    : "test-data.csv";
            }
        }

        var memoryStream = new MemoryStream();

        if (!_fileSizeIsZero)
        {
            using var fileStream = File.OpenRead(GetPathForFile(_fileName));
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.SeekToBeginning();
        }

        return
            new()
            {
                FileName = _fileName,
                FileSize = memoryStream.Length,
                FileStream = memoryStream,
            };
    }

    public async Task<List<ZipDataSetFileViewModel>> BuildViewModelsFromZip(bool withReplacement = false)
    {
        _fileName ??= "bulk-data-zip-valid.zip";

        await using var stream = File.OpenRead(GetPathForFile(_fileName));
        using var archive = new ZipArchive(stream);

        var dataFile1 = archive.Entries.First(e => e.Name == "one.csv");
        var metaFile1 = archive.Entries.First(e => e.Name == "one.meta.csv");

        var dataFile2 = archive.Entries.First(e => e.Name == "two.csv");
        var metaFile2 = archive.Entries.First(e => e.Name == "two.meta.csv");

        return [
            new () {
                Title = "First data set",
                DataFileId = Guid.NewGuid(),
                DataFileName = dataFile1.Name,
                DataFileSize = dataFile1.Length,
                MetaFileId = Guid.NewGuid(),
                MetaFileName = metaFile1.Name,
                MetaFileSize = metaFile1.Length,
                ReplacingFileId = withReplacement ? Guid.NewGuid() : null,
            },
            new() {
                Title = "Second data set",
                DataFileId = Guid.NewGuid(),
                DataFileName = dataFile2.Name,
                DataFileSize = dataFile2.Length,
                MetaFileId = Guid.NewGuid(),
                MetaFileName = metaFile2.Name,
                MetaFileSize = metaFile2.Length,
                ReplacingFileId = withReplacement ? Guid.NewGuid() : null,
            }
        ];
    }

    public DataSetFileBuilder WhereFileNameIs(string fileName)
    {
        _fileName = fileName;
        return this;
    }

    public DataSetFileBuilder WhereFileSizeIsZero()
    {
        _fileSizeIsZero = true;
        return this;
    }
}
