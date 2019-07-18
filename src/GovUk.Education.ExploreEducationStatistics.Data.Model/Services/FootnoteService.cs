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

        public IEnumerable<Footnote> GetFootnotes(long subjectId)
        {
            return _context.Set<SubjectFootnote>()
                .AsNoTracking()
                .Where(footnote => footnote.SubjectId == subjectId)
                .Select(footnote => footnote.Footnote);
        }
    }
}