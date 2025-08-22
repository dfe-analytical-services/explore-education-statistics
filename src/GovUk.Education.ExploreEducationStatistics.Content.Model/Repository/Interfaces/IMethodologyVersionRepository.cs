﻿#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

public interface IMethodologyVersionRepository
{
    Task<MethodologyVersion> CreateMethodologyForPublication(Guid publicationId, Guid createdByUserId);

    Task<MethodologyVersion> GetLatestVersion(Guid methodologyId);

    Task<List<MethodologyVersion>> GetLatestVersionByPublication(Guid publicationId);

    Task<MethodologyVersion?> GetLatestPublishedVersionBySlug(string slug);

    Task<MethodologyVersion?> GetLatestPublishedVersion(Guid methodologyId);

    Task<List<MethodologyVersion>> GetLatestPublishedVersionByPublication(Guid publicationId);

    Task<bool> IsLatestPublishedVersion(MethodologyVersion methodologyVersion);

    Task<bool> IsToBePublished(MethodologyVersion methodologyVersion);
}
