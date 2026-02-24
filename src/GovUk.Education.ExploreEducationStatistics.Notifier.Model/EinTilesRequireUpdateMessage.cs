namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public record EinTilesRequireUpdateMessage
{
    public required List<EinPageRequiresUpdate> Pages { get; init; }
}

public record EinPageRequiresUpdate
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required List<EinTileRequiresUpdate> Tiles { get; init; }
}

public record EinTileRequiresUpdate
{
    public required string? Title { get; init; }
    public required string? ContentSectionTitle { get; init; }
    public required Guid? DataSetFileId { get; init; }
}
