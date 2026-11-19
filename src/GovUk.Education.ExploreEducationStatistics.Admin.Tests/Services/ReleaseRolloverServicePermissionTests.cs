#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseRolloverServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task CreateReleaseRollover()
    {
        Publication publication = _dataFixture.DefaultPublication();

        Release sourceRelease = _dataFixture.DefaultRelease().WithPublication(publication);

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Publication>(
                p => p.Id == publication.Id,
                CanCreateReleaseForSpecificPublication
            )
            .AssertForbidden(async userService =>
            {
                await using var contentDbContext = InMemoryApplicationDbContext();
                contentDbContext.Publications.Add(publication);
                contentDbContext.Releases.Add(sourceRelease);
                await contentDbContext.SaveChangesAsync();

                var service = BuildService(context: contentDbContext, userService: userService.Object);

                return await service.CreateReleaseRollover(
                    new ReleaseRolloverCreateRequest
                    {
                        PublicationId = publication.Id,
                        RolloverSourceReleaseId = sourceRelease.Id,
                        Mode = RolloverMode.WithData,
                        Year = 2024,
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Type = ReleaseType.OfficialStatistics,
                        PublishingOrganisations = [],
                    }
                );
            });
    }

    private static ReleaseRolloverService BuildService(
        IUserService userService,
        ContentDbContext? context = null,
        IReleaseVersionService? releaseVersionService = null,
        IReleaseSlugValidator? releaseSlugValidator = null
    )
    {
        return new ReleaseRolloverService(
            appOptions: new AppOptions { EnableReleaseRollover = true }.ToOptionsWrapper(),
            context: context ?? Mock.Of<ContentDbContext>(),
            userService: userService,
            releaseVersionService: releaseVersionService ?? Mock.Of<IReleaseVersionService>(Strict),
            guidGenerator: new SequentialGuidGenerator(),
            organisationsValidator: new OrganisationsValidatorMockBuilder().Build(),
            releaseSlugValidator: releaseSlugValidator ?? new ReleaseSlugValidatorMockBuilder().Build()
        );
    }
}
