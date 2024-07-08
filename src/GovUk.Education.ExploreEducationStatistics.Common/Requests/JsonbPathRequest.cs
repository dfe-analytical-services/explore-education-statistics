namespace GovUk.Education.ExploreEducationStatistics.Common.Requests;

/// <summary>
/// This is used to specify a JSONB path within a given table and a given row of that table.
/// </summary>
public record JsonbPathRequest<TRowId>
{
    public string TableName { get; init; }

    public string IdColumnName { get; init; }

    public string JsonbColumnName { get; init; }

    public TRowId RowId { get; init; }

    public string[] PathSegments { get; init; }
}
