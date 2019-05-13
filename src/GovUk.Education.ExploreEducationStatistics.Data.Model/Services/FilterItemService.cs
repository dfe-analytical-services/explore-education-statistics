using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FilterItemService : AbstractDataService<FilterItem, long>, IFilterItemService
    {
        public FilterItemService(ApplicationDbContext context,
            ILogger<FilterItemService> logger) : base(context, logger)
        {
        }

        public IEnumerable<FilterItem> GetFilterItems(long subjectId,
            IEnumerable<int> years = null,
            IEnumerable<string> countries = null,
            IEnumerable<string> regions = null,
            IEnumerable<string> localAuthorities = null,
            IEnumerable<string> localAuthorityDistricts = null)
        {
            var predicate = PredicateBuilder.True<Observation>()
                .And(observation => observation.SubjectId == subjectId);

            if (years != null && years.Any())
            {
                predicate = predicate.And(observation =>
                    years.Contains(observation.Year));
            }

            if (countries != null && countries.Any())
            {
                predicate = predicate.And(observation =>
                    countries.Contains(observation.Location.Country.Code));
            }

            if (regions != null && regions.Any())
            {
                predicate = predicate.And(observation =>
                    regions.Contains(observation.Location.Region.Code));
            }

            if (localAuthorities != null && localAuthorities.Any())
            {
                predicate = predicate.And(observation =>
                    localAuthorities.Contains(observation.Location.LocalAuthority.Code));
            }

            if (localAuthorityDistricts != null && localAuthorityDistricts.Any())
            {
                predicate = predicate.And(observation =>
                    localAuthorityDistricts.Contains(observation.Location.LocalAuthorityDistrict.Code));
            }
            
            var filterItemIds = (from ofi in _context.Set<ObservationFilterItem>()
                join
                    o in _context.Observation.Where(predicate) on ofi
                        .ObservationId equals o.Id
                select ofi.FilterItemId).Distinct().ToList();

            return DbSet()
                .Where(item => filterItemIds.Contains(item.Id))
                .Include(item => item.FilterGroup.Filter);
        }
    }
}