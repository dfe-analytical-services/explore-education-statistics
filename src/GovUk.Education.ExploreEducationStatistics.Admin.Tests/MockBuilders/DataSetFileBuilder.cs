#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

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

        _fileName ??= fileType switch
        {
            FileType.BulkDataZipIndex => "dataset_names.csv",
            FileType.Metadata => "test-data.meta.csv",
            _ => "test-data.csv",
        };

        var memoryStream = new MemoryStream();

        if (!_fileSizeIsZero)
        {
            memoryStream = await FileStreamUtils.CreateMemoryStreamFromLocalResource(_fileName);
        }

        return new()
        {
            FileName = _fileName,
            FileSize = memoryStream.Length,
            FileStreamProvider = () => memoryStream,
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
