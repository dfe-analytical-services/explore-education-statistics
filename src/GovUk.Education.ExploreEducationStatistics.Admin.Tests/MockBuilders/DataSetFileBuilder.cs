#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using System;
using System.IO;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class DataSetFileBuilder
{
    private string? _fileName;
    private bool _fileSizeIsZero;

    public async Task<DataSetFileDto> Build(FileType fileType)
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
