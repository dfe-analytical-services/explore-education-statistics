#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class DataSetDtoBuilder
{
    private Guid? _releaseVersionId;
    private string? _title;
    private string? _dataFileName;
    private string? _metaFileName;
    private FileDto? _dataFile;
    private FileDto? _metaFile;

    public async Task<DataSetDto> Build()
    {
        _title ??= "Data set title";
        _dataFileName ??= "test-data.csv";
        _metaFileName ??= "test-data.meta.csv";

        if (_dataFile is null)
        {
            var dataMemoryStream = await FileStreamUtils.CreateMemoryStreamFromLocalResource(
                _dataFileName
            );

            _dataFile = new()
            {
                FileName = _dataFileName,
                FileSize = dataMemoryStream.Length,
                FileStreamProvider = () => dataMemoryStream,
            };
        }

        if (_metaFile is null)
        {
            var metaMemoryStream = await FileStreamUtils.CreateMemoryStreamFromLocalResource(
                _metaFileName
            );

            _metaFile = new()
            {
                FileName = _metaFileName,
                FileSize = metaMemoryStream.Length,
                FileStreamProvider = () => metaMemoryStream,
            };
        }

        return new()
        {
            ReleaseVersionId = _releaseVersionId ?? Guid.NewGuid(),
            Title = _title,
            DataFile = _dataFile,
            MetaFile = _metaFile,
        };
    }

    public DataSetDtoBuilder WhereReleaseVersionIdIsEmpty()
    {
        _releaseVersionId = Guid.Empty;
        return this;
    }

    public DataSetDtoBuilder WhereTitleIs(string title)
    {
        _title = title;
        return this;
    }

    public DataSetDtoBuilder WhereDataFileIs(FileDto file)
    {
        _dataFile = file;
        return this;
    }

    public DataSetDtoBuilder WhereMetaFileIs(FileDto file)
    {
        _metaFile = file;
        return this;
    }
}
