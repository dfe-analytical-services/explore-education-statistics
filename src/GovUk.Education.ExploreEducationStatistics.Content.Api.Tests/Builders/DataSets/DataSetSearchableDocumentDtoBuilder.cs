using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.DataSets;

public class DataSetSearchableDocumentDtoBuilder
{
    private Guid _fileId = Guid.NewGuid();
    private string _title = "Data set title";

    public DataSetSearchableDocumentDto Build() =>
        new()
        {
            FileId = _fileId,
            DataSetFileId = Guid.NewGuid(),
            SubjectId = Guid.NewGuid(),
            Filename = "data.csv",
            FileExtension = "csv",
            FileSize = "10 Kb",
            Title = _title,
            Content = "Data set summary",
            ThemeId = Guid.NewGuid(),
            ThemeTitle = "Theme title",
            PublicationId = Guid.NewGuid(),
            PublicationTitle = "Publication title",
            PublicationSlug = "publication-slug",
            ReleaseId = Guid.NewGuid(),
            ReleaseVersionId = Guid.NewGuid(),
            ReleaseTitle = "Academic year 2024/25",
            ReleaseSlug = "2024-25",
            ReleaseType = ReleaseType.OfficialStatistics.ToString(),
            LatestData = true,
            IsSuperseded = false,
            Published = new DateTimeOffset(2025, 09, 01, 08, 30, 00, TimeSpan.Zero),
            LastUpdated = new DateTimeOffset(2025, 09, 01, 08, 30, 00, TimeSpan.Zero),
            Api = null,
            NumDataFileRows = 100,
            GeographicLevels =
            [
                new()
                {
                    Code = "NAT",
                    Label = "National",
                    CsvOnly = false,
                },
            ],
            TimePeriodRange = new() { From = "2020/21", To = "2024/25" },
            Filters = ["Filter"],
            Indicators = ["Indicator"],
            PublishingOrganisations =
            [
                new SearchableDocumentPublishingOrganisationDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Department for Education",
                },
            ],
        };

    public DataSetSearchableDocumentDtoBuilder WithFileId(Guid fileId)
    {
        _fileId = fileId;
        return this;
    }

    public DataSetSearchableDocumentDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }
}
