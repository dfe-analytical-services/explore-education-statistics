#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<Filter> GetFiltersIncludingItems(Guid subjectId)
        {
            return FindMany(filter => filter.SubjectId == subjectId)
                .Include(filter => filter.FilterGroups)
                .ThenInclude(group => group.FilterItems)
                .ToList();
        }
    }
}
