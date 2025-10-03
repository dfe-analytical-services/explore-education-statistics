using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using InterpolatedSql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;

public interface IParquetDataRepository
{
    Task<long> CountRows(
        DataSetVersion dataSetVersion,
        IInterpolatedSql where,
        CancellationToken cancellationToken = default
    );

    Task<IList<IDictionary<string, object?>>> ListRows(
        DataSetVersion dataSetVersion,
        IEnumerable<string> columns,
        IInterpolatedSql where,
        IEnumerable<Sort>? sorts = null,
        int page = 1,
        int pageSize = 1000,
        CancellationToken cancellationToken = default
    );

    Task<ISet<string>> ListColumns(DataSetVersion dataSetVersion, CancellationToken cancellationToken = default);
}
