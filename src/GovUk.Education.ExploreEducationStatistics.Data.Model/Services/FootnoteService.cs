using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FootnoteService : AbstractRepository<Footnote, long>, IFootnoteService
    {
        public FootnoteService(ApplicationDbContext context, ILogger<FootnoteService> logger) : base(context, logger)
        {
        }

        public Dictionary<Footnote, IEnumerable<long>> GetFootnotes(IEnumerable<Observation> observations,
            IEnumerable<long> indicators)
        {
            var filterItems = observations.SelectMany(observation => observation.FilterItems)
                .Select(item => item.FilterItem.Id).Distinct();

            var indicatorListParam = CreateIdListType("indicatorList", indicators);
            var filterItemListParam = CreateIdListType("filterItemList", filterItems);

            var inner = _context.Query<IdWrapper>().AsNoTracking()
                .FromSql("EXEC dbo.FilteredFootnotes " +
                         "@indicatorList," +
                         "@filterItemList",
                    indicatorListParam,
                    filterItemListParam);

            var ids = inner.Select(obs => obs.Id).ToList();

            return Find(ids.ToArray())
                .ToDictionary(
                    footnote => footnote,
                    // TODO DFE-1127 What indicator ids are going to be returned here?
                    footnote => new List<long>().AsEnumerable());
        }
    }
}