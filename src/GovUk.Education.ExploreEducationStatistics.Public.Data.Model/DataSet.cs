using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSet : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; init; }

    public required string Title { get; set; }

    public required Guid PublicationId { get; set; }

    public required DataSetStatus Status { get; set; }

    public Guid? SupersedingDataSetId { get; set; }

    public DataSet? SupersedingDataSet { get; set; }

    public List<DataSetVersion> Versions { get; set; } = new();

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }
}
