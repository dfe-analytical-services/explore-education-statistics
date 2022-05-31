#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IFilterRepository
    {
        Task<List<Filter>> GetFiltersIncludingItems(Guid subjectId);
    }
}
