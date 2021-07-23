using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class FilterRepository : AbstractRepository<Filter, Guid>, IFilterRepository
    {
        public FilterRepository(StatisticsDbContext context) : base(context)
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