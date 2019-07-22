using System.Collections.Generic;
using System.Linq;
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

        public Dictionary<Footnote, IEnumerable<long>> GetFootnotes(IEnumerable<long> indicators)
        {
            return (from f in _context.Footnote
                    join i in _context.IndicatorFootnote on f.Id equals i.FootnoteId
                    where indicators.Contains(i.IndicatorId)
                    select new {f, i})
                .GroupBy(tuple => tuple.f)
                .ToDictionary(tuples => tuples.Key, tuples => tuples.Select(tuple => tuple.i.IndicatorId));
        }
    }
}