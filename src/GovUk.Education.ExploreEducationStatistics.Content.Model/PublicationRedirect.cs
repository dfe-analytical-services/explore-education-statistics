#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class PublicationRedirect : ICreatedTimestamp<DateTime>
{
    public string Slug { get; set; } = null!;

    public Guid PublicationId { get; set; }

    public Publication Publication { get; set; } = null!;

    public DateTime Created { get; set; }
}
