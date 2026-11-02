#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;

public class ReleaseSubjectRepository : IReleaseSubjectRepository
{
    private readonly StatisticsDbContext _statisticsDbContext;

    public ReleaseSubjectRepository(StatisticsDbContext statisticsDbContext)
    {
        _statisticsDbContext = statisticsDbContext;
    }

    public async Task<IReadOnlyList<ReleaseSubject>> FindAll(
        Guid releaseVersionId,
        Func<IQueryable<ReleaseSubject>, IQueryable<ReleaseSubject>>? queryExtender = null
    )
    {
        var query = _statisticsDbContext.ReleaseSubject.Where(rs => rs.ReleaseVersionId == releaseVersionId);

        if (queryExtender is not null)
        {
            query = queryExtender.Invoke(query);
        }

        return await query.ToListAsync();
    }
}
