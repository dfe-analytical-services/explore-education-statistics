using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
    using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

    public interface IPermalinkService
    {
        Task<PermalinkViewModel> GetAsync(Guid id);

        Task<PermalinkViewModel> CreateAsync(ObservationQueryContext tableQuery);
    }
}