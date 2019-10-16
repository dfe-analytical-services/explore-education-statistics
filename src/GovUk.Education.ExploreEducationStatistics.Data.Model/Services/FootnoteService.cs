using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FootnoteService : AbstractRepository<Footnote, long>, IFootnoteService
    {
        public FootnoteService(StatisticsDbContext context, ILogger<FootnoteService> logger) : base(context, logger)
        {
        }

        public IEnumerable<Footnote> GetFootnotes(long subjectId,
            IQueryable<Observation> observations,
            IEnumerable<long> indicators)
        {
            var filterItems = observations.SelectMany(observation => observation.FilterItems)
                .Select(item => item.FilterItem.Id).Distinct();

            var subjectIdParam = new SqlParameter("subjectId", subjectId);
            var indicatorListParam = CreateIdListType("indicatorList", indicators);
            var filterItemListParam = CreateIdListType("filterItemList", filterItems);

            return _context.Footnote.FromSqlRaw(
                "EXEC dbo.FilteredFootnotes " +
                "@subjectId," +
                "@indicatorList," +
                "@filterItemList",
                subjectIdParam,
                indicatorListParam,
                filterItemListParam);
        }
    }
}