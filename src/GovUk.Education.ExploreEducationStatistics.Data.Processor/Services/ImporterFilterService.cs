using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImporterFilterService
    {
        private readonly ImporterFilterCache _importerFilterCache;

        public ImporterFilterService(ImporterFilterCache importerFilterCache)
        {
            _importerFilterCache = importerFilterCache;
        }

        public FilterItem Find(string filterItemLabel, string filterGroupLabel, Filter filter, StatisticsDbContext context)
        {
            var filterGroup = LookupFilterGroup(filter, filterGroupLabel, context);
            return LookupFilterItem(filterGroup, filterItemLabel, context);
        }

        public FilterItem LookupFilterItem(FilterGroup filterGroup, string label, StatisticsDbContext context)
        {
            return _importerFilterCache.GetOrCacheFilterItem(
                filterGroup, 
                label, 
                context, 
                () => context
                    .FilterItem
                    .AsNoTracking()
                    .First(fi => fi.FilterGroupId == filterGroup.Id && fi.Label == label));
        }

        public FilterGroup LookupFilterGroup(Filter filter, string label, StatisticsDbContext context)
        {
            return _importerFilterCache.GetOrCacheFilterGroup(
                filter, 
                label, 
                context, 
                () => context
                    .FilterGroup
                    .AsNoTracking()
                    .First(fg => fg.FilterId == filter.Id && fg.Label == label));
        }
    }
}
