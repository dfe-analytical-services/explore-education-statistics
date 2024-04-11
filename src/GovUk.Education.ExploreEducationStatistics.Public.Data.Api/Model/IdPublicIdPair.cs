namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;

public record IdPublicIdPair
{
    public required int Id { get; init; }

    public required string PublicId { get; init; }
}
