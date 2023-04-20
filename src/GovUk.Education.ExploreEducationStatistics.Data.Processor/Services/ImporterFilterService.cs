using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImporterFilterService
    {
        private readonly Dictionary<string, FilterItem> _filterItems = new();
        
        public ImporterFilterService(ImporterFilterCache importerFilterCache)
        {
        }

        public void Fill(List<Filter> filters)
        {
            filters.SelectMany(f => f.FilterGroups).SelectMany(fg => fg.FilterItems).ForEach(fi => 
                _filterItems.Add($"{fi.FilterGroup.Filter.Label}_{fi.FilterGroup.Label}_{fi.Label}".ToLower(), fi));
        }

        public FilterItem Find(string filterItemLabel, string filterGroupLabel, string filterLabel)
        {
            return _filterItems[$"{filterLabel}_{filterGroupLabel}_{filterItemLabel}".ToLower()];
        }
    }
}
