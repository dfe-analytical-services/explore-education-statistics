using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFootnoteService
    {
        Footnote CreateFootnote(string content,
            IEnumerable<Guid> filterIds,
            IEnumerable<Guid> filterGroupIds,
            IEnumerable<Guid> filterItemIds,
            IEnumerable<Guid> indicatorIds,
            IEnumerable<Guid> subjectIds);

        void DeleteFootnote(Guid id);

        bool Exists(Guid id);

        Footnote GetFootnote(Guid id);

        IEnumerable<Footnote> GetFootnotes(Guid releaseId);

        IEnumerable<Footnote> GetFootnotes(
            Guid subjectId,
            IQueryable<Observation> observations,
            IEnumerable<Guid> indicators
        );

        Footnote UpdateFootnote(Guid id,
            string content,
            IEnumerable<Guid> filterIds,
            IEnumerable<Guid> filterGroupIds,
            IEnumerable<Guid> filterItemIds,
            IEnumerable<Guid> indicatorIds,
            IEnumerable<Guid> subjectIds);
    }
}