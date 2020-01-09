using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
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

        IEnumerable<Footnote> GetFootnotes(Guid releaseId);

        Footnote UpdateFootnote(Guid id,
            string content,
            IEnumerable<Guid> filterIds,
            IEnumerable<Guid> filterGroupIds,
            IEnumerable<Guid> filterItemIds,
            IEnumerable<Guid> indicatorIds,
            IEnumerable<Guid> subjectIds);
    }
}