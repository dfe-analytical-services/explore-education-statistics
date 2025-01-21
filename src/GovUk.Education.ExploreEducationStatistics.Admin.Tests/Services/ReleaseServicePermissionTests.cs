#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseServicePermissionTests
{
    private static readonly Publication Publication = new()
    {
        Id = Guid.NewGuid()
    };

    [Fact]
    public async Task CreateRelease()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(Publication, CanCreateReleaseForSpecificPublication)
            .AssertForbidden(
                userService =>
                {
                    using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.Publications.Add(Publication);
                    contextDbContext.SaveChangesAsync();

                    var service = BuildReleaseService(
                        context: contextDbContext,
                        userService: userService.Object);

                    return service.CreateRelease(
                        new ReleaseCreateRequest
                        {
                            PublicationId = Publication.Id,
                        }
                    );
                }
            );
    }

    private ReleaseService BuildReleaseService(
        IUserService userService,
        ContentDbContext? context = null,
        IReleaseVersionService? releaseVersionService = null)
    {
        return new ReleaseService(
            context: context ?? Mock.Of<ContentDbContext>(),
            userService: userService,
            releaseVersionService: releaseVersionService ?? Mock.Of<IReleaseVersionService>(),
            guidGenerator: new SequentialGuidGenerator()
        );
    }
}
