using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;

public class ReleaseDataContentDtoBuilder
{
    private Guid _releaseId = Guid.NewGuid();
    private Guid _releaseVersionId = Guid.NewGuid();
    private string? _dataDashboards = "Data dashboards";
    private string? _dataGuidance = "Data guidance";
    private ReleaseDataContentDataSetDto[] _dataSets = [new ReleaseDataContentDataSetDtoBuilder().Build()];
    private ReleaseDataContentFeaturedTableDto[] _featuredTables =
    [
        new ReleaseDataContentFeaturedTableDtoBuilder().Build(),
    ];
    private ReleaseDataContentSupportingFileDto[] _supportingFiles =
    [
        new ReleaseDataContentSupportingFileDtoBuilder().Build(),
    ];

    public ReleaseDataContentDto Build() =>
        new()
        {
            ReleaseId = _releaseId,
            ReleaseVersionId = _releaseVersionId,
            DataDashboards = _dataDashboards,
            DataGuidance = _dataGuidance,
            DataSets = _dataSets,
            FeaturedTables = _featuredTables,
            SupportingFiles = _supportingFiles,
        };

    public ReleaseDataContentDtoBuilder WithReleaseId(Guid releaseId)
    {
        _releaseId = releaseId;
        return this;
    }

    public ReleaseDataContentDtoBuilder WithReleaseVersionId(Guid releaseVersionId)
    {
        _releaseVersionId = releaseVersionId;
        return this;
    }

    public ReleaseDataContentDtoBuilder WithDataDashboards(string? dataDashboards)
    {
        _dataDashboards = dataDashboards;
        return this;
    }

    public ReleaseDataContentDtoBuilder WithDataGuidance(string? dataGuidance)
    {
        _dataGuidance = dataGuidance;
        return this;
    }

    public ReleaseDataContentDtoBuilder WithDataSets(ReleaseDataContentDataSetDto[] dataSets)
    {
        _dataSets = dataSets;
        return this;
    }

    public ReleaseDataContentDtoBuilder WithFeaturedTables(ReleaseDataContentFeaturedTableDto[] featuredTables)
    {
        _featuredTables = featuredTables;
        return this;
    }

    public ReleaseDataContentDtoBuilder WithSupportingFiles(ReleaseDataContentSupportingFileDto[] supportingFiles)
    {
        _supportingFiles = supportingFiles;
        return this;
    }
}

public class ReleaseDataContentDataSetDtoBuilder
{
    private Guid _dataSetFileId = Guid.NewGuid();
    private Guid _fileId = Guid.NewGuid();
    private Guid _subjectId = Guid.NewGuid();
    private ReleaseDataContentDataSetMetaDto _meta = new ReleaseDataContentDataSetMetaDtoBuilder().Build();
    private string _summary = "Summary";
    private string _title = "Title";

    public ReleaseDataContentDataSetDto Build() =>
        new()
        {
            DataSetFileId = _dataSetFileId,
            FileId = _fileId,
            SubjectId = _subjectId,
            Meta = _meta,
            Summary = _summary,
            Title = _title,
        };

    public ReleaseDataContentDataSetDtoBuilder WithDataSetFileId(Guid dataSetFileId)
    {
        _dataSetFileId = dataSetFileId;
        return this;
    }

    public ReleaseDataContentDataSetDtoBuilder WithFileId(Guid fileId)
    {
        _fileId = fileId;
        return this;
    }

    public ReleaseDataContentDataSetDtoBuilder WithSubjectId(Guid subjectId)
    {
        _subjectId = subjectId;
        return this;
    }

    public ReleaseDataContentDataSetDtoBuilder WithMeta(ReleaseDataContentDataSetMetaDto meta)
    {
        _meta = meta;
        return this;
    }

    public ReleaseDataContentDataSetDtoBuilder WithSummary(string summary)
    {
        _summary = summary;
        return this;
    }

    public ReleaseDataContentDataSetDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }
}

public class ReleaseDataContentDataSetMetaDtoBuilder
{
    private string[] _filters = ["Filter 1"];
    private string[] _geographicLevels = ["Geographic level 1"];
    private string[] _indicators = ["Indicator 1"];
    private int _numDataFileRows = 100;
    private ReleaseDataContentDataSetMetaTimePeriodRangeDto _timePeriodRange =
        new ReleaseDataContentDataSetMetaTimePeriodRangeDtoBuilder().Build();

    public ReleaseDataContentDataSetMetaDto Build() =>
        new()
        {
            Filters = _filters,
            GeographicLevels = _geographicLevels,
            Indicators = _indicators,
            NumDataFileRows = _numDataFileRows,
            TimePeriodRange = _timePeriodRange,
        };

    public ReleaseDataContentDataSetMetaDtoBuilder WithFilters(string[] filters)
    {
        _filters = filters;
        return this;
    }

    public ReleaseDataContentDataSetMetaDtoBuilder WithGeographicLevels(string[] geographicLevels)
    {
        _geographicLevels = geographicLevels;
        return this;
    }

    public ReleaseDataContentDataSetMetaDtoBuilder WithIndicators(string[] indicators)
    {
        _indicators = indicators;
        return this;
    }

    public ReleaseDataContentDataSetMetaDtoBuilder WithNumDataFileRows(int numDataFileRows)
    {
        _numDataFileRows = numDataFileRows;
        return this;
    }

    public ReleaseDataContentDataSetMetaDtoBuilder WithTimePeriodRange(
        ReleaseDataContentDataSetMetaTimePeriodRangeDto timePeriodRange
    )
    {
        _timePeriodRange = timePeriodRange;
        return this;
    }
}

public class ReleaseDataContentDataSetMetaTimePeriodRangeDtoBuilder
{
    private string _start = "Start";
    private string _end = "End";

    public ReleaseDataContentDataSetMetaTimePeriodRangeDto Build() => new() { Start = _start, End = _end };

    public ReleaseDataContentDataSetMetaTimePeriodRangeDtoBuilder WithStart(string start)
    {
        _start = start;
        return this;
    }

    public ReleaseDataContentDataSetMetaTimePeriodRangeDtoBuilder WithEnd(string end)
    {
        _end = end;
        return this;
    }
}

public class ReleaseDataContentFeaturedTableDtoBuilder
{
    private Guid _featuredTableId = Guid.NewGuid();
    private Guid _dataBlockId = Guid.NewGuid();
    private Guid _dataBlockParentId = Guid.NewGuid();
    private string _summary = "Summary";
    private string _title = "Title";

    public ReleaseDataContentFeaturedTableDto Build() =>
        new()
        {
            FeaturedTableId = _featuredTableId,
            DataBlockId = _dataBlockId,
            DataBlockParentId = _dataBlockParentId,
            Summary = _summary,
            Title = _title,
        };

    public ReleaseDataContentFeaturedTableDtoBuilder WithFeaturedTableId(Guid featuredTableId)
    {
        _featuredTableId = featuredTableId;
        return this;
    }

    public ReleaseDataContentFeaturedTableDtoBuilder WithDataBlockId(Guid dataBlockId)
    {
        _dataBlockId = dataBlockId;
        return this;
    }

    public ReleaseDataContentFeaturedTableDtoBuilder WithDataBlockParentId(Guid dataBlockParentId)
    {
        _dataBlockParentId = dataBlockParentId;
        return this;
    }

    public ReleaseDataContentFeaturedTableDtoBuilder WithSummary(string summary)
    {
        _summary = summary;
        return this;
    }

    public ReleaseDataContentFeaturedTableDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }
}

public class ReleaseDataContentSupportingFileDtoBuilder
{
    private Guid _fileId = Guid.NewGuid();
    private string _extension = "Extension";
    private string _filename = "Filename";
    private string _size = "Size";
    private string _summary = "Summary";
    private string _title = "Title";

    public ReleaseDataContentSupportingFileDto Build() =>
        new()
        {
            FileId = _fileId,
            Extension = _extension,
            Filename = _filename,
            Size = _size,
            Summary = _summary,
            Title = _title,
        };

    public ReleaseDataContentSupportingFileDtoBuilder WithFileId(Guid fileId)
    {
        _fileId = fileId;
        return this;
    }

    public ReleaseDataContentSupportingFileDtoBuilder WithExtension(string extension)
    {
        _extension = extension;
        return this;
    }

    public ReleaseDataContentSupportingFileDtoBuilder WithFilename(string filename)
    {
        _filename = filename;
        return this;
    }

    public ReleaseDataContentSupportingFileDtoBuilder WithSize(string size)
    {
        _size = size;
        return this;
    }

    public ReleaseDataContentSupportingFileDtoBuilder WithSummary(string summary)
    {
        _summary = summary;
        return this;
    }

    public ReleaseDataContentSupportingFileDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }
}
