#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IFilterItemRepository : IRepository<FilterItem, Guid>
    {
        Task<Dictionary<Guid, int>> CountFilterItemsByFilter(IEnumerable<Guid> filterItemIds);

        /// <summary>
        /// Method to retrieve a set of <see cref="FilterItem" />s, their <see cref="FilterGroup" />s and
        /// <see cref="Filter" />s that are present on a set of previously-matched
        /// <see cref="IQueryable<MatchedObservation>" /> Observations.
        ///
        /// Note that this method is for use when a set of matched Observations have had their ids stored in the
        /// #MatchedObservation temporary table.
        /// 
        /// </summary>
        /// <param name="subjectId">The Subject ID that the Filter Items belong to.</param>
        /// <param name="matchedObservations">Ids of matched Observations stored in the #MatchedObservation temporary
        /// table.</param>
        /// <returns>a set of <see cref="FilterItem" />s, their <see cref="FilterGroup" />s and
        /// <see cref="Filter" />s</returns>
        Task<IEnumerable<FilterItem>> GetFilterItemsFromMatchedObservationIds(
            Guid subjectId,
            IQueryable<MatchedObservation> matchedObservations);

        /// <summary>
        /// Method to retrieve a set of <see cref="FilterItem" />s, their <see cref="FilterGroup" />s and
        /// <see cref="Filter" />s that are present on a given <see cref="IQueryable<Observation>" /> set of
        /// Observations.
        ///
        /// Note that this method is for use when there is a set of Observations that has been fetched into memory
        /// already, as it is optimised for this scenario.
        /// /// </summary>
        /// <param name="observations">A set of Observations that have been already fetched into memory.</param>
        /// <returns>a set of <see cref="FilterItem" />s, their <see cref="FilterGroup" />s and
        /// <see cref="Filter" />s</returns>
        IList<FilterItem> GetFilterItemsFromObservationList(IList<Observation> observations);

        FilterItem? GetTotal(Filter filter);

        FilterItem? GetTotal(IEnumerable<FilterItem> filterItems);
    }
}
