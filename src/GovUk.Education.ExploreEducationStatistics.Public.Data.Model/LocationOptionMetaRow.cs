using LinqToDB.Mapping;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

/// <summary>
/// This is a LINQ to DB mapping for <see cref="LocationOptionMeta"/>,
/// providing access to the lower level columns without having to deal
/// with the normal inheritance model which can prevent access.
/// </summary>
public class LocationOptionMetaRow
{
    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column(CanBeNull = false)]
    public required string Type { get; set; }

    [Column(CanBeNull = false)]
    public required string Label { get; set; }

    public string? Code { get; set; }

    public string? OldCode { get; set; }

    public string? Urn { get; set; }

    public string? LaEstab { get; set; }

    public string? Ukprn { get; set; }
}