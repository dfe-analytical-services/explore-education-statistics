namespace GovUk.Education.ExploreEducationStatistics.Common.Requests;

public record JsonbPathRequest<TRowId>
{
    public string TableName { get; init; }
    
    public string IdColumnName { get; init; }
    
    public string JsonbColumnName { get; init; }
    
    public TRowId RowId { get; init; }
    
    public string[] PathSegments { get; init; }
}
