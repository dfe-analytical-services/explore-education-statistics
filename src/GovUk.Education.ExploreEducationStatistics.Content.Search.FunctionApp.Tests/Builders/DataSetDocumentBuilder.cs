using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class DataSetDocumentBuilder
{
    private Guid? _fileId;
    private Guid? _releaseId;
    private Guid? _publicationId;
    private string? _title;
    private DataSetDocumentApi? _api;

    public DataSetDocument Build() =>
        new()
        {
            FileId = _fileId ?? new Guid("3d6cf2b9-6b0a-4b0e-9d6f-0f5b7c1f6a01"),
            DataSetFileId = new Guid("5f1f4d0e-2c57-4a4e-8a52-7f0d5b9f3c02"),
            SubjectId = new Guid("9a7b2c1d-4e3f-4a5b-8c6d-1e2f3a4b5c03"),
            Filename = "data-file.csv",
            FileExtension = "csv",
            FileSize = "10 Kb",
            Title = _title ?? "Data Set Title",
            Content = "This is a data set summary.",
            ThemeId = new Guid("498e449b-d093-43dc-959c-bc3a2c23fdfb"),
            ThemeTitle = "Theme Title",
            PublicationId = _publicationId ?? new Guid("8db69a42-ef47-441b-bed5-5e91d012194f"),
            PublicationTitle = "Publication Title",
            PublicationSlug = "publication-slug",
            ReleaseId = _releaseId ?? new Guid("08228000-71de-4a1a-9adf-35f3d2bd061a"),
            ReleaseVersionId = new Guid("a20f1068-4298-495d-8d80-c662ca72731d"),
            ReleaseTitle = "Academic year 2024/25",
            ReleaseSlug = "2024-25",
            ReleaseType = "OfficialStatistics",
            LatestData = true,
            IsSuperseded = false,
            Published = new DateTimeOffset(2025, 02, 21, 09, 30, 00, TimeSpan.Zero),
            LastUpdated = new DateTimeOffset(2025, 03, 14, 10, 15, 00, TimeSpan.Zero),
            Api = _api,
            NumDataFileRows = 1234,
            GeographicLevelDetails =
            [
                new DataSetDocumentGeographicLevel
                {
                    Code = "LA",
                    Label = "Local authority",
                    CsvOnly = false,
                },
                new DataSetDocumentGeographicLevel
                {
                    Code = "NAT",
                    Label = "National",
                    CsvOnly = true,
                },
            ],
            TimePeriodRange = new DataSetDocumentTimePeriodRange { From = "2020/21", To = "2024/25" },
            Filters = ["Filter 1", "Filter 2"],
            Indicators = ["Indicator 1", "Indicator 2"],
            PublishingOrganisationIds =
            [
                new Guid("7cbcfe03-9f7e-478a-8512-a1a5e0ca793b"),
                new Guid("7c3252c6-6e94-4a34-8762-ff52aae5c0c0"),
            ],
            PublishingOrganisationTitles = ["Department for Education", "Ofsted"],
        };

    public DataSetDocumentBuilder WithFileId(Guid fileId)
    {
        _fileId = fileId;
        return this;
    }

    public DataSetDocumentBuilder WithReleaseId(Guid releaseId)
    {
        _releaseId = releaseId;
        return this;
    }

    public DataSetDocumentBuilder WithPublicationId(Guid publicationId)
    {
        _publicationId = publicationId;
        return this;
    }

    public DataSetDocumentBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public DataSetDocumentBuilder WithApi(DataSetDocumentApi api)
    {
        _api = api;
        return this;
    }
}
