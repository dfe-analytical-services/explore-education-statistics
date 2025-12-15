#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.KeyStatisticsMigration.Dtos;

/// <summary>
/// TODO EES-6747 Remove after the Key Statistics migration is complete.
/// </summary>
public record KeyStatisticsMigrationReportDto
{
    public required bool DryRun { get; init; }

    public int KeyStatisticsCount { get; init; }

    public int UpdatedKeyStatisticsCount => MigrationResults.Count;

    public required List<KeyStatisticsMigrationReportKeyStatisticUpdateDto> MigrationResults { get; init; }

    public required List<KeyStatisticsMigrationReportPublicationDto> Publications { get; init; }
}

public record KeyStatisticsMigrationReportPublicationDto
{
    public required Guid PublicationId { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public required string Theme { get; init; }

    public required List<KeyStatisticsMigrationReportReleaseDto> Releases { get; init; }
}

public record KeyStatisticsMigrationReportReleaseDto
{
    public required Guid ReleaseId { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public required List<KeyStatisticsMigrationReportReleaseVersionDto> ReleaseVersions { get; init; }
}

public record KeyStatisticsMigrationReportReleaseVersionDto
{
    public required Guid ReleaseVersionId { get; init; }

    public bool IsDraft => !Published.HasValue;

    public required List<KeyStatisticsMigrationReportKeyStatisticDto> KeyStatistics { get; init; }

    public required DateTimeOffset? Published { get; init; }

    public required int Version { get; init; }
}

public record KeyStatisticsMigrationReportKeyStatisticDto
{
    public required Guid KeyStatisticId { get; init; }

    public required string? GuidanceTextOriginal { get; init; }

    public required string? GuidanceTextPlain { get; init; }

    public required int Order { get; init; }

    public bool HasGuidanceTextChanged => GuidanceTextOriginal != GuidanceTextPlain;
}

public record KeyStatisticsMigrationReportKeyStatisticUpdateDto
{
    public required Guid KeyStatisticId { get; init; }

    public required string GuidanceTextOriginal { get; init; }

    public required string GuidanceTextPlain { get; init; }
}
