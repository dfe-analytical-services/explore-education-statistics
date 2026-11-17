using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.DataSets;

public class DataSetSearchableDocumentDtoBuilder
{
    private Guid _fileId = Guid.NewGuid();
    private Guid _dataSetFileId = Guid.NewGuid();
    private Guid _subjectId = Guid.NewGuid();
    private string _filename = "data.csv";
    private string _fileExtension = "csv";
    private string _fileSize = "10 Kb";
    private string _title = "Data set title";
    private string _content = "Data set summary";
    private Guid _themeId = Guid.NewGuid();
    private string _themeTitle = "Theme title";
    private Guid _publicationId = Guid.NewGuid();
    private string _publicationTitle = "Publication title";
    private string _publicationSlug = "publication-slug";
    private Guid _releaseId = Guid.NewGuid();
    private Guid _releaseVersionId = Guid.NewGuid();
    private string _releaseTitle = "Academic year 2024/25";
    private string _releaseSlug = "2024-25";
    private ReleaseType _releaseType = ReleaseType.OfficialStatistics;
    private bool _latestData = true;
    private bool _isSuperseded;
    private DateTimeOffset _published = new(2025, 09, 01, 08, 30, 00, TimeSpan.Zero);
    private DateTimeOffset _lastUpdated = new(2025, 09, 01, 08, 30, 00, TimeSpan.Zero);
    private DataSetSearchableDocumentApiDto? _api;
    private int _numDataFileRows = 100;
    private DataSetSearchableDocumentGeographicLevelDto[] _geographicLevels =
    [
        new()
        {
            Code = "NAT",
            Label = "National",
            CsvOnly = false,
        },
    ];
    private DataSetSearchableDocumentTimePeriodRangeDto _timePeriodRange = new() { From = "2020/21", To = "2024/25" };
    private string[] _filters = ["Filter"];
    private string[] _indicators = ["Indicator"];
    private SearchableDocumentPublishingOrganisationDto[] _publishingOrganisations =
    [
        new() { Id = Guid.NewGuid(), Title = "Department for Education" },
    ];

    public DataSetSearchableDocumentDto Build() =>
        new()
        {
            FileId = _fileId,
            DataSetFileId = _dataSetFileId,
            SubjectId = _subjectId,
            Filename = _filename,
            FileExtension = _fileExtension,
            FileSize = _fileSize,
            Title = _title,
            Content = _content,
            ThemeId = _themeId,
            ThemeTitle = _themeTitle,
            PublicationId = _publicationId,
            PublicationTitle = _publicationTitle,
            PublicationSlug = _publicationSlug,
            ReleaseId = _releaseId,
            ReleaseVersionId = _releaseVersionId,
            ReleaseTitle = _releaseTitle,
            ReleaseSlug = _releaseSlug,
            ReleaseType = _releaseType,
            LatestData = _latestData,
            IsSuperseded = _isSuperseded,
            Published = _published,
            LastUpdated = _lastUpdated,
            Api = _api,
            NumDataFileRows = _numDataFileRows,
            GeographicLevels = _geographicLevels,
            TimePeriodRange = _timePeriodRange,
            Filters = _filters,
            Indicators = _indicators,
            PublishingOrganisations = _publishingOrganisations,
        };

    public DataSetSearchableDocumentDtoBuilder WithFileId(Guid fileId)
    {
        _fileId = fileId;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithDataSetFileId(Guid dataSetFileId)
    {
        _dataSetFileId = dataSetFileId;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithSubjectId(Guid subjectId)
    {
        _subjectId = subjectId;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithFilename(string filename)
    {
        _filename = filename;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithFileExtension(string fileExtension)
    {
        _fileExtension = fileExtension;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithFileSize(string fileSize)
    {
        _fileSize = fileSize;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithThemeId(Guid themeId)
    {
        _themeId = themeId;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithThemeTitle(string themeTitle)
    {
        _themeTitle = themeTitle;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithPublicationId(Guid publicationId)
    {
        _publicationId = publicationId;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithPublicationTitle(string publicationTitle)
    {
        _publicationTitle = publicationTitle;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithPublicationSlug(string publicationSlug)
    {
        _publicationSlug = publicationSlug;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithReleaseId(Guid releaseId)
    {
        _releaseId = releaseId;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithReleaseVersionId(Guid releaseVersionId)
    {
        _releaseVersionId = releaseVersionId;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithReleaseTitle(string releaseTitle)
    {
        _releaseTitle = releaseTitle;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithReleaseSlug(string releaseSlug)
    {
        _releaseSlug = releaseSlug;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithReleaseType(ReleaseType releaseType)
    {
        _releaseType = releaseType;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithLatestData(bool latestData)
    {
        _latestData = latestData;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithIsSuperseded(bool isSuperseded)
    {
        _isSuperseded = isSuperseded;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithPublished(DateTimeOffset published)
    {
        _published = published;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithLastUpdated(DateTimeOffset lastUpdated)
    {
        _lastUpdated = lastUpdated;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithApi(DataSetSearchableDocumentApiDto? api)
    {
        _api = api;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithNumDataFileRows(int numDataFileRows)
    {
        _numDataFileRows = numDataFileRows;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithGeographicLevels(
        params DataSetSearchableDocumentGeographicLevelDto[] geographicLevels
    )
    {
        _geographicLevels = geographicLevels;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithTimePeriodRange(
        DataSetSearchableDocumentTimePeriodRangeDto timePeriodRange
    )
    {
        _timePeriodRange = timePeriodRange;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithFilters(params string[] filters)
    {
        _filters = filters;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithIndicators(params string[] indicators)
    {
        _indicators = indicators;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithPublishingOrganisations(
        params SearchableDocumentPublishingOrganisationDto[] publishingOrganisations
    )
    {
        _publishingOrganisations = publishingOrganisations;
        return this;
    }
}
