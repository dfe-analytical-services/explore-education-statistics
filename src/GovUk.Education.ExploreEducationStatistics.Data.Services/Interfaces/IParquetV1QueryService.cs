#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

/// <summary>
/// Answers table tool queries from the Parquet copy of a data set's CSV file rather than from the Observation
/// tables in the statistics database. The Parquet file holds the raw CSV values, so results are mapped back onto
/// the Location, Filter Item and Indicator rows of the statistics database that the table tool refers to by ID.
/// </summary>
public interface IParquetV1QueryService
{
    Task<IList<(int Year, TimeIdentifier TimeIdentifier)>> ListTimePeriods(
        File dataFile,
        IEnumerable<Guid> locationIds,
        CancellationToken cancellationToken = default
    );

    Task<IList<FilterItem>> ListFilterItems(
        File dataFile,
        FullTableQuery query,
        CancellationToken cancellationToken = default
    );

    Task<IList<Observation>> ListObservations(
        File dataFile,
        FullTableQuery query,
        CancellationToken cancellationToken = default
    );
}
