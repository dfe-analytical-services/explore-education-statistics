namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class Release
{
    public required string Title { get; set; }

    public required string Slug { get; set; }

    public required Guid DataSetFileId { get; set; }

    public required Guid ReleaseFileId { get; set; }
}
