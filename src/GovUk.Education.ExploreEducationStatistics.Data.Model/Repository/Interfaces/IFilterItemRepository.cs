#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IFilterItemRepository : IRepository<FilterItem, Guid>
    {
        Task<Dictionary<Guid, int>> CountFilterItemsByFilter(IEnumerable<Guid> filterItemIds);

        /// <summary>
        /// Method to retrieve a set of <see cref="FilterItem" />s, their <see cref="FilterGroup" />s and
        /// <see cref="Filter" />s that are present on a given <see cref="IQueryable<Observation>" /> set of
        /// Observations.
        ///
        /// Note that this method is for use when there is specifically an <see cref="IQueryable<Observation>" />
        /// available that is not yet fetched into memory, as it is optimised for this scenario.
        /// 
        /// </summary>
        /// <param name="subjectId">The Subject ID that the Filter Items belong to.</param>
        /// <param name="observations">An Observation query that has not yet been fetched into memory.</param>
        /// <returns>a set of <see cref="FilterItem" />s, their <see cref="FilterGroup" />s and
        /// <see cref="Filter" />s</returns>
        IEnumerable<FilterItem> GetFilterItemsFromObservationQuery(
            Guid subjectId,
            IQueryable<Observation> observations);

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
        IEnumerable<FilterItem> GetFilterItemsFromObservationList(IList<Observation> observations);

        FilterItem? GetTotal(Filter filter);

        FilterItem? GetTotal(IEnumerable<FilterItem> filterItems);
    }
}
