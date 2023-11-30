using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSet : ICreatedUpdatedTimestamps<DateTime, DateTime>
{
    public Guid Id { get; init; }
    public string Title { get; set; } = string.Empty!;
    public Guid PublicationId { get; set; }
    public DataSetStatus Status { get; set; }
    public Guid SupersedingDataSetId { get; set; }
    public DataSet SupersedingDataSet { get; set; } = null!;
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}
