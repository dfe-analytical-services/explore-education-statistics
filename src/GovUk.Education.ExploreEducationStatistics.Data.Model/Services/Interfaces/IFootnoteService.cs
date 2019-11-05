using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFootnoteService
    {
        Footnote CreateFootnote(string content,
            IEnumerable<long> filterIds,
            IEnumerable<long> filterGroupIds,
            IEnumerable<long> filterItemIds,
            IEnumerable<long> indicatorIds,
            IEnumerable<long> subjectIds);

        void DeleteFootnote(long id);

        Footnote GetFootnote(long id);

        IEnumerable<Footnote> GetFootnotes(Guid releaseId);

        IEnumerable<Footnote> GetFootnotes(
            long subjectId,
            IQueryable<Observation> observations,
            IEnumerable<long> indicators
        );

        Footnote UpdateFootnote(long id,
            string content,
            IEnumerable<long> filterIds,
            IEnumerable<long> filterGroupIds,
            IEnumerable<long> filterItemIds,
            IEnumerable<long> indicatorIds,
            IEnumerable<long> subjectIds);
    }
}