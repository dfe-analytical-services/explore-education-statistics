using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFootnoteService
    {
        IEnumerable<Footnote> GetFootnotes(IEnumerable<Observation> observations, IEnumerable<long> indicators);
    }
}