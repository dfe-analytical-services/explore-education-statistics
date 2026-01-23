using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data.Interfaces;

public interface IPublicDataSetRepository
{
    Task<DataSet> GetDataSet(Guid dataSetId);

    Task<IndicatorMeta> GetIndicatorMeta(Guid dataSetVersionId, string indicatorPublicId);
}
