using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetMeta : ICreatedUpdatedTimestamps<DateTime, DateTime>
{
    public Guid Id { get; set; }
    public Guid DataSetVersionId { get; set; }
    public DataSetMetaType Type { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}
