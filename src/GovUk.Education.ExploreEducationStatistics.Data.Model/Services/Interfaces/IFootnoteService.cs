using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFootnoteService
    {
        Dictionary<Footnote, IEnumerable<long>> GetFootnotes(long subjectId, IEnumerable<string> indicators);
    }
}