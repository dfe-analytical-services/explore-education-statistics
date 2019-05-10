using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class IndicatorGroupService : AbstractDataService<IndicatorGroup, long>, IIndicatorGroupService
    {
        public IndicatorGroupService(ApplicationDbContext context,
            ILogger<IndicatorGroupService> logger) : base(context, logger)
        {
        }

        public IEnumerable<IndicatorGroup> GetIndicatorGroups(long subjectId,
            IEnumerable<int> years = null,
            IEnumerable<string> countries = null,
            IEnumerable<string> regions = null,
            IEnumerable<string> localAuthorities = null,
            IEnumerable<string> localAuthorityDistricts = null)
        {
            // TODO DFE-609 fields are ignored
            
            return FindMany(group => group.SubjectId == subjectId,
                new List<Expression<Func<IndicatorGroup, object>>> {group => group.Indicators});
        }
    }
}