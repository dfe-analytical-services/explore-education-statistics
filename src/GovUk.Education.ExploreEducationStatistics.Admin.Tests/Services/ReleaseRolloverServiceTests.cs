#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class ReleaseRolloverServiceTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly OrganisationsValidatorMockBuilder _organisationsValidator = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public class CreateReleaseRolloverTests : ReleaseRolloverServiceTests
    {
        [Theory]
        [InlineData(RolloverMode.WithData)]
        [InlineData(RolloverMode.StructureOnly)]
        public async Task GivenEligibleSourceRelease_CreatesRollover(RolloverMode mode)
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                await context.SaveChangesAsync();
            }

            Guid? newReleaseVersionId = null;

            var releaseVersionViewModel = new ReleaseVersionViewModel();
            var releaseVersionServiceMock = new Mock<IReleaseVersionService>(Strict);
            releaseVersionServiceMock
                .Setup(rvs => rvs.GetRelease(It.IsAny<Guid>()))
                .ReturnsAsync(releaseVersionViewModel)
                .Callback((Guid releaseVersionId) => newReleaseVersionId = releaseVersionId);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = BuildService(
                    contentDbContext: context,
                    releaseVersionService: releaseVersionServiceMock.Object
                );

                var result = await sut.CreateReleaseRollover(
                    ValidRequest(publication.Id, sourceRelease.Id) with
                    {
                        Mode = mode,
                    }
                );

                var viewModel = result.AssertRight();
                Assert.Equal(releaseVersionViewModel, viewModel);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var actualReleaseVersion = await context
                    .ReleaseVersions.Include(rv => rv.Release)
                    .Include(rv => rv.PublishingOrganisations)
                    .SingleAsync(rv => rv.Id == newReleaseVersionId);

                var actualRelease = actualReleaseVersion.Release;

                Assert.Equal(publication.Id, actualRelease.PublicationId);

                // The gotcha this guards against: the request's RolloverSourceReleaseId must end up
                // stamped as-is on the new Release - it must be a Release id, not a ReleaseVersion id.
                Assert.Equal(sourceRelease.Id, actualRelease.RolloverSourceReleaseId);
                Assert.NotEqual(sourceReleaseVersion.Id, actualRelease.RolloverSourceReleaseId);

                Assert.Equal(2024, actualRelease.Year);
                Assert.Equal(TimeIdentifier.AcademicYear, actualRelease.TimePeriodCoverage);

                Assert.Equal(ReleaseType.OfficialStatistics, actualReleaseVersion.Type);
                Assert.Equal(ReleaseApprovalStatus.Draft, actualReleaseVersion.ApprovalStatus);
                Assert.Equal(0, actualReleaseVersion.Version);
                Assert.True(actualReleaseVersion.Rollover);
                Assert.Empty(actualReleaseVersion.PublishingOrganisations);
            }
        }

        [Fact]
        public async Task GivenSourceReleaseDoesNotExist_NotFound()
        {
            Publication publication = _dataFixture.DefaultPublication();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using var context2 = InMemoryApplicationDbContext(contextId);
            var sut = BuildService(contentDbContext: context2);

            var result = await sut.CreateReleaseRollover(ValidRequest(publication.Id, Guid.NewGuid()));

            result.AssertNotFound();
        }

        [Fact]
        public async Task GivenRolloverSourceReleaseIdIsAReleaseVersionId_NotFound()
        {
            // Guards against conflating a ReleaseVersion id with its parent Release id - passing the
            // wrong kind of id must fail closed (404), not silently succeed against the wrong table.
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using var context2 = InMemoryApplicationDbContext(contextId);
            var sut = BuildService(contentDbContext: context2);

            var result = await sut.CreateReleaseRollover(ValidRequest(publication.Id, sourceReleaseVersion.Id));

            result.AssertNotFound();
        }

        [Fact]
        public async Task GivenSourceReleaseBelongsToDifferentPublication_BadRequest()
        {
            Publication publication = _dataFixture.DefaultPublication();
            Publication otherPublication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(otherPublication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.AddRange(publication, otherPublication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using var context2 = InMemoryApplicationDbContext(contextId);
            var sut = BuildService(contentDbContext: context2);

            var result = await sut.CreateReleaseRollover(ValidRequest(publication.Id, sourceRelease.Id));

            result.AssertBadRequest(RolloverSourceReleaseMustBeInSamePublication);
        }

        [Fact]
        public async Task GivenSourceReleaseHasNoPublishedVersion_BadRequest()
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithPublished(null)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using var context2 = InMemoryApplicationDbContext(contextId);
            var sut = BuildService(contentDbContext: context2);

            var result = await sut.CreateReleaseRollover(ValidRequest(publication.Id, sourceRelease.Id));

            result.AssertBadRequest(RolloverSourceReleaseMustBePublished);
        }

        [Fact]
        public async Task GivenSourceReleaseIsEligible_ReleaseSlugIsValidated()
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                await context.SaveChangesAsync();
            }

            var releaseVersionService = new ReleaseVersionServiceMockBuilder().WhereGetReleaseVersionReturns();
            var releaseSlugValidator = new ReleaseSlugValidatorMockBuilder();

            await using var context2 = InMemoryApplicationDbContext(contextId);
            var sut = BuildService(
                contentDbContext: context2,
                releaseVersionService: releaseVersionService.Build(),
                releaseSlugValidator: releaseSlugValidator.Build()
            );

            var year = 2024;
            var timePeriodCoverage = TimeIdentifier.AcademicYear;
            var label = "initial";

            await sut.CreateReleaseRollover(
                ValidRequest(publication.Id, sourceRelease.Id) with
                {
                    Year = year,
                    TimePeriodCoverage = timePeriodCoverage,
                    Label = label,
                }
            );

            var expectedReleaseSlug = NamingUtils.CreateReleaseSlug(
                year: year,
                timePeriodCoverage: timePeriodCoverage,
                label: label
            );

            releaseSlugValidator.Assert.ValidateNewSlugWasCalled(
                expectedNewReleaseSlug: expectedReleaseSlug,
                expectedPublicationId: publication.Id,
                expectedReleaseId: null
            );
        }

        [Fact]
        public async Task GivenReleaseRolloverDisabled_ReturnsForbid()
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using var context2 = InMemoryApplicationDbContext(contextId);
            var sut = BuildService(contentDbContext: context2, enableReleaseRollover: false);

            var result = await sut.CreateReleaseRollover(ValidRequest(publication.Id, sourceRelease.Id));

            result.AssertForbidden();
        }
    }

    private ReleaseRolloverService BuildService(
        ContentDbContext contentDbContext,
        IReleaseVersionService? releaseVersionService = null,
        IReleaseSlugValidator? releaseSlugValidator = null,
        bool enableReleaseRollover = true
    )
    {
        var userService = AlwaysTrueUserService();

        userService.Setup(s => s.GetUserId()).Returns(UserId);

        return new ReleaseRolloverService(
            appOptions: new AppOptions { EnableReleaseRollover = enableReleaseRollover }.ToOptionsWrapper(),
            context: contentDbContext,
            userService: userService.Object,
            releaseVersionService: releaseVersionService ?? Mock.Of<IReleaseVersionService>(Strict),
            guidGenerator: new SequentialGuidGenerator(),
            organisationsValidator: _organisationsValidator.Build(),
            releaseSlugValidator: releaseSlugValidator ?? new ReleaseSlugValidatorMockBuilder().Build()
        );
    }

    private static ReleaseRolloverCreateRequest ValidRequest(Guid publicationId, Guid rolloverSourceReleaseId) =>
        new()
        {
            PublicationId = publicationId,
            RolloverSourceReleaseId = rolloverSourceReleaseId,
            Mode = RolloverMode.WithData,
            Year = 2024,
            TimePeriodCoverage = TimeIdentifier.AcademicYear,
            Label = "initial",
            Type = ReleaseType.OfficialStatistics,
            PublishingOrganisations = [],
        };
}
