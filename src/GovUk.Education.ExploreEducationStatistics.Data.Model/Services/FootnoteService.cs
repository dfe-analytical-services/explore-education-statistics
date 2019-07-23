using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FootnoteService : AbstractRepository<Footnote, long>, IFootnoteService
    {
        public FootnoteService(ApplicationDbContext context, ILogger<FootnoteService> logger) : base(context, logger)
        {
        }

        public Dictionary<Footnote, IEnumerable<long>> GetFootnotes(long subjectId, IEnumerable<string> indicators)
        {
//            return _context.Footnote
//                .Join(_context.IndicatorFootnote, f => f.Id, i => i.FootnoteId, (f, i) => new
//                {
//                    Footnote = f, i.IndicatorId
//                })
//                .Where(t => indicators.Contains(t.IndicatorId))
//                .GroupBy(tuple => tuple.Footnote)
//                .ToDictionary(
//                    grouping => grouping.Key,
//                    grouping => grouping.Select(tuple => tuple.IndicatorId));
// TODO DFE-1087
return new Dictionary<Footnote, IEnumerable<long>>();
        }
    }
}