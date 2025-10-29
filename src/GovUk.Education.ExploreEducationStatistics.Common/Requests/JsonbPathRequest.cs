namespace GovUk.Education.ExploreEducationStatistics.Common.Requests;

/// <summary>
/// This is used to specify a JSONB path within a given table and a given row of that table.
/// </summary>
public record JsonbPathRequest<TRowId>
{
    public required string TableName { get; init; }

    public required string IdColumnName { get; init; }

    public required string JsonbColumnName { get; init; }

    public required TRowId RowId { get; init; }

    public required string[] PathSegments { get; init; }
}
