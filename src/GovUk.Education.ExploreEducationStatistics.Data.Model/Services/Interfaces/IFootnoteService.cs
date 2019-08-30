using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFootnoteService
    {
        IEnumerable<Footnote> GetFootnotes(
            long subjectId,
            IQueryable<Observation> observations,
            IEnumerable<long> indicators
        );
    }
}