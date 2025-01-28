#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task CreateRelease()
    {
        Publication publication = _dataFixture.DefaultPublication();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Publication>(p => p.Id == publication.Id,
                CanCreateReleaseForSpecificPublication)
            .AssertForbidden(
                async userService =>
                {
                    await using var contextDbContext = InMemoryApplicationDbContext();
                    contextDbContext.Publications.Add(publication);
                    await contextDbContext.SaveChangesAsync();

                    var service = BuildService(
                        context: contextDbContext,
                        userService: userService.Object);

                    return await service.CreateRelease(
                        new ReleaseCreateRequest
                        {
                            PublicationId = publication.Id,
                        }
                    );
                }
            );
    }

    private static ReleaseService BuildService(
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
