using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSet : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; init; }

    public string Title { get; set; } = string.Empty;

    public Guid PublicationId { get; set; }

    public DataSetStatus Status { get; set; }

    public Guid? SupersedingDataSetId { get; set; }

    public DataSet? SupersedingDataSet { get; set; }

    public List<DataSetVersion> Versions { get; set; } = new();

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }
}
