using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FilterService : AbstractRepository<Filter, Guid>, IFilterService
    {
        public FilterService(StatisticsDbContext context) : base(context)
        {
        }

        public IEnumerable<Filter> GetFiltersIncludingItems(Guid subjectId)
        {
            return FindMany(filter => filter.SubjectId == subjectId)
                .Include(filter => filter.FilterGroups)
                .ThenInclude(group => group.FilterItems);
        }
    }
}