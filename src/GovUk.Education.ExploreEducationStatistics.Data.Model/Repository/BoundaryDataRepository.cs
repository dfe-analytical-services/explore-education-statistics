#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;

public class BoundaryDataRepository : IBoundaryDataRepository
{
    private readonly DataServiceMemoryCache<BoundaryData> _cache;
    private readonly StatisticsDbContext _context;

    public BoundaryDataRepository(StatisticsDbContext context, DataServiceMemoryCache<BoundaryData> cache)
    {
        _context = context;
        _cache = cache;
    }

    public Dictionary<string, BoundaryData> FindByBoundaryLevelAndCodes(long boundaryLevelId, IEnumerable<string> codes)
    {
        var codesList = codes.ToList();
        var cached = TryCacheLookup(boundaryLevelId, codesList);
        var cachedCodes = cached.Select(boundaryData => boundaryData.Code);
        var codesNotCached = codesList.Except(cachedCodes);

        var dbResult = FindAndCache(boundaryLevelId, codesNotCached);

        return cached.Union(dbResult).ToDictionary(boundaryData => boundaryData.Code);
    }

    private IEnumerable<BoundaryData> FindAndCache(long boundaryLevelId, IEnumerable<string> codes)
    {
        var boundaryDataList = _context
            .BoundaryData.Where(boundaryData =>
                boundaryData.BoundaryLevel.Id == boundaryLevelId && codes.Contains(boundaryData.Code)
            )
            .ToList();

        AddAllToCache(boundaryLevelId, boundaryDataList);

        return boundaryDataList;
    }

    private List<BoundaryData> TryCacheLookup(long boundaryLevelId, IEnumerable<string> codes)
    {
        return codes.Select(code => TryCacheLookup(boundaryLevelId, code)).WhereNotNull().ToList();
    }

    private BoundaryData? TryCacheLookup(long boundaryLevelId, string code)
    {
        return _cache.GetOrDefault($"{boundaryLevelId}_{code}");
    }

    private void AddAllToCache(long boundaryLevelId, List<BoundaryData> boundaryDataList)
    {
        foreach (var boundaryData in boundaryDataList)
        {
            _cache.Set($"{boundaryLevelId}_{boundaryData.Code}", boundaryData, TimeSpan.FromMinutes(30));
        }
    }
}
