#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

[Obsolete("No longer used. Use the content ReleaseVersion instead. Kept until the table is dropped.")]
public class ReleaseVersion
{
    public Guid Id { get; set; }
    public Guid PublicationId { get; set; }
}
