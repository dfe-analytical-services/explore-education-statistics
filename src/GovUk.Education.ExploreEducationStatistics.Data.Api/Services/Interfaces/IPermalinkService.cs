namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
    using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

    public interface IPermalinkService
    {
        Task<Permalink> GetAsync(Guid id);

        Task<Permalink> CreateAsync(ObservationQueryContext tableQuery);
    }
}