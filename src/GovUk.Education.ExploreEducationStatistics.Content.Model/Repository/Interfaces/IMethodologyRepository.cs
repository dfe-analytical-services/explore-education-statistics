#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IMethodologyRepository
    {
        Task<Methodology> CreateMethodologyForPublication(Guid publicationId, Guid createdByUserId);

        Task<List<Methodology>> GetLatestByPublication(Guid publicationId);

        Task<Methodology?> GetLatestPublishedByMethodologyParent(Guid methodologyParentId);

        Task<List<Methodology>> GetLatestPublishedByPublication(Guid publicationId);

        Task<Publication> GetOwningPublicationByMethodologyParent(Guid methodologyParentId);

        Task<bool> IsPubliclyAccessible(Guid methodologyId);

        Task PublicationTitleChanged(Guid publicationId, string originalSlug, string updatedTitle, string updatedSlug);
    }
}
