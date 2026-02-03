#nullable enable
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data.Interfaces;

public interface IPublicDataSetRepository
{
    Task<DataSet> GetDataSet(Guid dataSetId, CancellationToken cancellationToken = default);

    Task<IndicatorMeta?> GetIndicatorMeta(
        Guid dataSetVersionId,
        string indicatorPublicId,
        CancellationToken cancellationToken = default
    );
}
