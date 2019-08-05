using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IPermalinkService
    {
        Task<PermalinkViewModel> GetAsync(Guid id);

        Task<PermalinkViewModel> CreateAsync(ObservationQueryContext tableQuery);
    }
}