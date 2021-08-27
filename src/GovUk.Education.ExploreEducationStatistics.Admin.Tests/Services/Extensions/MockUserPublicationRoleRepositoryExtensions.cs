#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions
{
    public static class MockUserPublicationRoleRepositoryExtensions
    {
        public static IReturnsResult<IUserPublicationRoleRepository> SetupPublicationOwnerRoleExpectations(
            this Mock<IUserPublicationRoleRepository> service,
            Guid userId,
            Publication publication,
            bool hasRole)
        {
            return service.Setup(s => s.IsUserPublicationOwner(userId, publication.Id))
                .ReturnsAsync(hasRole);
        }
    }
}
