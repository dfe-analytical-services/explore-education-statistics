#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IMethodologyVersionRepository
    {
        Task<MethodologyVersion> CreateMethodologyForPublication(Guid publicationId, Guid createdByUserId);

        Task<MethodologyVersion> GetLatestVersion(Guid methodologyId);

        Task<List<MethodologyVersion>> GetLatestVersionByPublication(Guid publicationId);

        Task<MethodologyVersion?> GetLatestPublishedVersion(Guid methodologyId);

        Task<List<MethodologyVersion>> GetLatestPublishedVersionByPublication(Guid publicationId);

        Task<MethodologyVersion?> GetLatestPublishedVersionBySlug(string slug);

        Task<bool> IsPubliclyAccessible(Guid methodologyVersionId);

        Task<bool> IsPubliclyAccessible(MethodologyVersion methodologyVersion);

        Task PublicationTitleOrSlugChanged(Guid publicationId, string updatedTitle, string updatedSlug);
    }
}
