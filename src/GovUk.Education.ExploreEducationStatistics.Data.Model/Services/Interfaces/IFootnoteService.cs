using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFootnoteService
    {
        IEnumerable<Footnote> GetFootnotes(
            Guid subjectId,
            IQueryable<Observation> observations,
            IEnumerable<Guid> indicators
        );
    }
}