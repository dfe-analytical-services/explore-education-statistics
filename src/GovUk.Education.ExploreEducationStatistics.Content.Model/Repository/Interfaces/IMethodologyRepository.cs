﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IMethodologyRepository
    {
        Task<Methodology> CreateMethodologyForPublication(Guid publicationId);

        Task<List<Methodology>> GetLatestByPublication(Guid publicationId);

        Task<Methodology> GetLatestPublishedByMethodologyParent(Guid methodologyParentId);

        Task<List<Methodology>> GetLatestPublishedByPublication(Guid publicationId);

        Task<bool> IsPubliclyAccessible(Guid methodologyId);
        
        Task PublicationTitleChanged(Guid publicationId, string originalSlug, string updatedTitle, string updatedSlug);
    }
}
