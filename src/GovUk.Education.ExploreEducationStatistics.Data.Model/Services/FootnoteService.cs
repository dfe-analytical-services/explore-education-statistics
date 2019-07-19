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

        public IEnumerable<Footnote> GetFootnotes(long subjectId, IEnumerable<long> indicators)
        {
            return (from f in _context.Footnote
                    join s in _context.SubjectFootnote on f.Id equals s.FootnoteId
                    join i in _context.IndicatorFootnote on f.Id equals i.FootnoteId
                    where s.SubjectId == subjectId && indicators.Contains(i.IndicatorId)
                    select f)
                .Distinct()
                .OrderBy(footnote => footnote.Id)
                .ToList();
        }
    }
}