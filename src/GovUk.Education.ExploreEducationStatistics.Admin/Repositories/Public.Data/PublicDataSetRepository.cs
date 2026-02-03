#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data;

public class PublicDataSetRepository(PublicDataDbContext publicDataDbContext) : IPublicDataSetRepository
{
    public async Task<DataSet> GetDataSet(Guid dataSetId, CancellationToken cancellationToken = default) =>
        await publicDataDbContext
            .DataSets.Include(ds => ds.LatestLiveVersion)
            .SingleAsync(ds => ds.Id == dataSetId, cancellationToken);

    public async Task<IndicatorMeta?> GetIndicatorMeta(
        Guid dataSetVersionId,
        string indicatorPublicId,
        CancellationToken cancellationToken = default
    ) =>
        await publicDataDbContext.IndicatorMetas.SingleOrDefaultAsync(
            im => im.DataSetVersionId == dataSetVersionId && im.PublicId == indicatorPublicId,
            cancellationToken
        );
}
