#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class DataSetDtoBuilder
{
    private Guid? _releaseVersionId;
    private string? _title;
    private string? _dataFileName;
    private string? _metaFileName;
    private FileDto? _dataFile;
    private FileDto? _metaFile;
    private Content.Model.File? _replacingFile;

    public async Task<DataSetDto> Build()
    {
        _title ??= "Data set title";
        _dataFileName ??= "test-data.csv";
        _metaFileName ??= "test-data.meta.csv";
        _dataFile ??= null;
        _metaFile ??= null;
        _replacingFile ??= null;

        var dataMemoryStream = new MemoryStream();
        var metaMemoryStream = new MemoryStream();

        if (_dataFile is null)
        {
            using var dataFileStream = File.OpenRead(GetPathForFile(_dataFileName));
            await dataFileStream.CopyToAsync(dataMemoryStream);
            dataMemoryStream.SeekToBeginning();

            _dataFile = new()
            {
                FileName = _dataFileName,
                FileSize = dataMemoryStream.Length,
                FileStream = dataMemoryStream
            };
        }

        if (_metaFile is null)
        {
            using var metaFileStream = File.OpenRead(GetPathForFile(_metaFileName));
            await metaFileStream.CopyToAsync(metaMemoryStream);
            metaMemoryStream.SeekToBeginning();

            _metaFile = new()
            {
                FileName = _metaFileName,
                FileSize = metaMemoryStream.Length,
                FileStream = metaMemoryStream
            };
        }

        return
            new()
            {
                ReleaseVersionId = _releaseVersionId ?? Guid.NewGuid(),
                Title = _title,
                DataFile = _dataFile,
                MetaFile = _metaFile,
                ReplacingFile = _replacingFile,
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

    public DataSetDtoBuilder WhereReplacingFileIs(Content.Model.File file)
    {
        _replacingFile = file;
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
