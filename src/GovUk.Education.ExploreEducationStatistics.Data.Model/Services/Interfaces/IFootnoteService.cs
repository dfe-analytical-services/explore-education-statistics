using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFootnoteService
    {
        Dictionary<Footnote, IEnumerable<long>> GetFootnotes(IEnumerable<Observation> observations,
            IEnumerable<long> indicators);
    }
}