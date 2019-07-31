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

        public IEnumerable<Footnote> GetFootnotes(IEnumerable<Observation> observations,
            IEnumerable<long> indicators)
        {
            var filterItems = observations.SelectMany(observation => observation.FilterItems)
                .Select(item => item.FilterItem.Id).Distinct();

            var indicatorListParam = CreateIdListType("indicatorList", indicators);
            var filterItemListParam = CreateIdListType("filterItemList", filterItems);

            return _context.Footnote.AsNoTracking().FromSql(
                "EXEC dbo.FilteredFootnotes " +
                "@indicatorList," +
                "@filterItemList",
                indicatorListParam,
                filterItemListParam);
        }
    }
}