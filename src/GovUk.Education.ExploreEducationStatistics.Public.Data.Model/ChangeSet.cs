using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public abstract class ChangeSet<TChangeState> : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public Guid DataSetVersionId { get; set; }

    public List<Change<TChangeState>> Changes { get; set; } = new();

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }
}

public class ChangeSetFilters : ChangeSet<FilterChangeState>;

public class ChangeSetFilterOptions : ChangeSet<FilterOptionChangeState>;

public class ChangeSetIndicators : ChangeSet<IndicatorChangeState>;

public class ChangeSetLocations : ChangeSet<LocationChangeState>;

public class ChangeSetTimePeriods : ChangeSet<TimePeriodChangeState>;
