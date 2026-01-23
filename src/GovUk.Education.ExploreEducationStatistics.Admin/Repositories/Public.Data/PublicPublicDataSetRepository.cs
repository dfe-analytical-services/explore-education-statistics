using GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data;

public class PublicPublicDataSetRepository(PublicDataDbContext publicDataDbContext) : IPublicDataSetRepository
{
    public async Task<DataSet> GetDataSet(Guid dataSetId)
    {
        var apiDataSet = await publicDataDbContext
            .DataSets.Include(ds => ds.LatestLiveVersion)
            .SingleAsync(ds => ds.Id == dataSetId);
        return apiDataSet;
    }

    public async Task<IndicatorMeta> GetIndicatorMeta(Guid dataSetVersionId, string indicatorPublicId)
    {
        var indicatorMeta = await publicDataDbContext.IndicatorMetas.SingleOrDefaultAsync(im =>
            im.DataSetVersionId == dataSetVersionId && im.PublicId == indicatorPublicId
        );
        return indicatorMeta;
    }
}
