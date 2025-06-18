#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
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
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit.Abstractions;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class ReleaseServiceTests
{
    private readonly DataFixture _dataFixture = new();
    private static readonly User User = new() { Id = Guid.NewGuid() };

    public class CreateReleaseTests : ReleaseServiceTests
    {
        [Fact]
        public async Task ReleaseTypeExperimentalStatistics_ReturnsValidationActionResult()
        {
            var releaseCreateRequest = new ReleaseCreateRequest
            {
                Type = ReleaseType.ExperimentalStatistics,
            };

            var releaseService = BuildService(Mock.Of<ContentDbContext>());

            var result = await releaseService.CreateRelease(releaseCreateRequest);

            result.AssertBadRequest(ReleaseTypeInvalid);
        }

        [Fact]
        public async Task NoTemplate()
        {
            Publication publication = _dataFixture.DefaultPublication();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
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
                var releaseService = BuildService(
                    contentDbContext: context, 
                    releaseVersionService: releaseVersionServiceMock.Object);

                var result = await releaseService.CreateRelease(
                    new ReleaseCreateRequest
                    {
                        PublicationId = publication.Id,
                        Year = 2018,
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Type = ReleaseType.OfficialStatistics,
                        Label = "initial"
                    });

                var viewModel = result.AssertRight();
                Assert.Equal(releaseVersionViewModel, viewModel);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var actualReleaseVersion = await context.ReleaseVersions
                    .Include(rv => rv.Release)
                    .SingleAsync(rv => rv.Id == newReleaseVersionId);

                var actualRelease = actualReleaseVersion.Release;

                Assert.Equal(publication.Id, actualRelease.PublicationId);
                Assert.Equal(2018, actualRelease.Year);
                Assert.Equal(TimeIdentifier.AcademicYear, actualRelease.TimePeriodCoverage);
                Assert.Equal("2018-19-initial", actualRelease.Slug);

                Assert.Equal(ReleaseType.OfficialStatistics, actualReleaseVersion.Type);
                Assert.Equal(ReleaseApprovalStatus.Draft, actualReleaseVersion.ApprovalStatus);
                Assert.Equal(0, actualReleaseVersion.Version);

                Assert.Null(actualReleaseVersion.PreviousVersionId);
                Assert.Null(actualReleaseVersion.PublishScheduled);
                Assert.Null(actualReleaseVersion.Published);
                Assert.Null(actualReleaseVersion.NextReleaseDate);
                Assert.Null(actualReleaseVersion.NotifiedOn);
                Assert.False(actualReleaseVersion.NotifySubscribers);
                Assert.False(actualReleaseVersion.UpdatePublishedDate);
            }
        }

        [Fact]
        public async Task WithTemplate()
        {
            var dataBlock1 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Data Block 1",
                Order = 2,
                Comments =
                [
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Content = "Comment 1 Text"
                    },
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Content = "Comment 2 Text"
                    }
                ]
            };

            var dataBlock2 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Data Block 2"
            };

            var templateReleaseVersion = new ReleaseVersion
            {
                Content = ListOf(new ContentSection
                {
                    Id = Guid.NewGuid(),
                    Caption = "Template caption index 0",
                    Heading = "Template heading index 0",
                    Type = ContentSectionType.Generic,
                    Order = 1,
                    Content =
                    [
                        new HtmlBlock
                        {
                            Id = Guid.NewGuid(),
                            Body = "<div></div>",
                            Order = 1,
                            Comments =
                            [
                                new Comment
                                {
                                    Id = Guid.NewGuid(),
                                    Content = "Comment 1 Text"
                                },
                                new Comment
                                {
                                    Id = Guid.NewGuid(),
                                    Content = "Comment 2 Text"
                                }
                            ]
                        },
                        dataBlock1,
                        dataBlock2
                    ]
                })
            };

            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease()
                    .WithVersions([templateReleaseVersion])
                    .Generate(1));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
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
                var releaseService = BuildService(
                    contentDbContext: context, 
                    releaseVersionService: releaseVersionServiceMock.Object);

                var result = await releaseService.CreateRelease(
                    new ReleaseCreateRequest
                    {
                        PublicationId = publication.Id,
                        TemplateReleaseId = templateReleaseVersion.Id,
                        Year = 2018,
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Type = ReleaseType.OfficialStatistics
                    }
                );

                var viewModel = result.AssertRight();
                Assert.Equal(releaseVersionViewModel, viewModel);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                // Do an in depth check of the saved release version
                var newReleaseVersion = await context
                    .ReleaseVersions
                    .Include(releaseVersion => releaseVersion.Content)
                    .ThenInclude(section => section.Content)
                    .SingleAsync(rv => rv.Id == newReleaseVersionId);

                var contentSections = newReleaseVersion.GenericContent.ToList();
                Assert.Single(contentSections);
                Assert.Equal("Template caption index 0", contentSections[0].Caption);
                Assert.Equal("Template heading index 0", contentSections[0].Heading);
                Assert.Single(contentSections);
                Assert.Equal(1, contentSections[0].Order);

                // Content should not be copied when created from a template.
                Assert.Empty(contentSections[0].Content);
                Assert.Empty(contentSections[0].Content.AsReadOnly());
                Assert.Equal(ContentSectionType.ReleaseSummary, newReleaseVersion.SummarySection.Type);
                Assert.Equal(ContentSectionType.Headlines, newReleaseVersion.HeadlinesSection.Type);
                Assert.Equal(ContentSectionType.KeyStatisticsSecondary,
                    newReleaseVersion.KeyStatisticsSecondarySection.Type);

                // Data Blocks should not be copied when created from a template.
                Assert.Equal(2, context.DataBlocks.Count());
                Assert.Empty(context
                    .DataBlocks
                    .Where(dataBlock => dataBlock.ReleaseVersionId == newReleaseVersionId));
            }
        }

        [Fact]
        public async Task GivenReleaseExists_ReleaseSlugIsValidated()
        {
            Publication publication = _dataFixture.DefaultPublication();

            using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
            context.Publications.Add(publication);
            await context.SaveChangesAsync();

            var releaseVersionService = new ReleaseVersionServiceMockBuilder()
                .WhereGetReleaseVersionReturns();

            var releaseSlugValidator = new ReleaseSlugValidatorMockBuilder();

            var sut = BuildService(
                contentDbContext: context,
                releaseVersionService: releaseVersionService.Build(),
                releaseSlugValidator: releaseSlugValidator.Build());

            var year = 2020;
            var timePeriodCoverage = TimeIdentifier.AcademicYear;
            var label = "initial";
            var request = new ReleaseCreateRequest
            {
                PublicationId = publication.Id,
                Year = year,
                TimePeriodCoverage = timePeriodCoverage,
                Label = label,
                Type = ReleaseType.OfficialStatistics,
            };

            // ACT
            await sut.CreateRelease(request);

            // ASSERT
            var expectedReleaseSlug = NamingUtils.CreateReleaseSlug(
                year: year,
                timePeriodCoverage: timePeriodCoverage,
                label: label);

            releaseSlugValidator.Assert.ValidateNewSlugWasCalled(
                expectedNewReleaseSlug: expectedReleaseSlug,
                expectedPublicationId: publication.Id,
                expectedReleaseId: null);
        }
    }

    public class UpdateReleaseTests(ITestOutputHelper output) : ReleaseServiceTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GivenLiveRelease_WhenSlugUpdated_ThenEventRaised(bool isPublicationArchived)
        {
            // ARRANGE
            var expectedPublicationId = Guid.NewGuid();
            var expectedPublicationSlug = "this-is-the-publication-slug";
            var expectedReleaseId = Guid.NewGuid();
            var label = "this is the new label";
            var year = 2025;
            var timePeriod = TimeIdentifier.April;
            
            var expectedNewReleaseSlug = NamingUtils.CreateReleaseSlug(year, timePeriod, label);

            var supersedingPublication = _dataFixture
                .DefaultPublication()
                .WithLatestPublishedReleaseVersion(_dataFixture.DefaultReleaseVersion());

            var publication = _dataFixture.DefaultPublication()
                .WithId(expectedPublicationId)
                .WithSlug(expectedPublicationSlug)
                .If(isPublicationArchived)
                    .Then(p => p.WithSupersededBy(supersedingPublication))
                .Generate();

            var release = _dataFixture.DefaultRelease()
                .WithId(expectedReleaseId)
                .WithPublication(publication)
                .WithYear(year)
                .WithTimePeriodCoverage(timePeriod)
                .Generate();
            
            var releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(release)
                .WithPublished(new DateTime(2025, 04, 01, 09, 16, 00)) // Release Version is live (ie has been published)
                .Generate();

            var dbContextId = Guid.NewGuid().ToString();
            
            await using(var context = InMemoryApplicationDbContext(dbContextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(release);
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }
            
            var releaseVersionService = new ReleaseVersionServiceMockBuilder()
                                            .WhereGetReleaseVersionReturns(releaseVersion.Id);

            var releasePublishingStatusRepository = new ReleasePublishingStatusRepositoryMockBuilder()
                .SetNoReleaseVersionStatus(releaseVersion.Id);

            var adminEventRaiser = new AdminEventRaiserMockBuilder();

            await using var context2 = InMemoryApplicationDbContext(dbContextId);
            
            var sut = BuildService(
                                    context2,
                                    releaseVersionService: releaseVersionService.Build(),
                                    releaseCacheService: new ReleaseCacheServiceMockBuilder().Build(),
                                    publicationCacheService: new PublicationCacheServiceMockBuilder().Build(),
                                    redirectsCacheService: new RedirectsCacheServiceMockBuilder().Build(),
                                    releasePublishingStatusRepository: releasePublishingStatusRepository.Build(),
                                    adminEventRaiser: adminEventRaiser.Build());
            
            var request = new ReleaseUpdateRequest
            {
                Label = label
            };

            // ACT
            var result = await sut.UpdateRelease(release.Id, request);
            
            // ASSERT
            result.AssertRight();
            
            output.WriteLine($"Expected Release Id: {expectedReleaseId}");
            output.WriteLine($"Expected New Release Slug: {expectedNewReleaseSlug}");
            output.WriteLine($"Expected Publication Id: {expectedPublicationId}");
            output.WriteLine($"Expected Publication Slug: {expectedPublicationSlug}");
            
            adminEventRaiser.Assert.OnReleaseSlugChangedWasRaised(
                expectedReleaseId,
                expectedNewReleaseSlug,
                expectedPublicationId,
                expectedPublicationSlug,
                isPublicationArchived
                );
        }
        
        [Fact]
        public async Task GivenReleaseThatIsNotPublished_WhenSlugUpdated_ThenNoEventRaised()
        {
            // ARRANGE
            var expectedPublicationId = Guid.NewGuid();
            var expectedPublicationSlug = "this-is-the-publication-slug";
            var expectedReleaseId = Guid.NewGuid();
            var label = "this is the new label";
            var year = 2025;
            var timePeriod = TimeIdentifier.April;
            
            var publication = _dataFixture.DefaultPublication()
                .WithId(expectedPublicationId)
                .WithSlug(expectedPublicationSlug)
                .Generate();

            var release = _dataFixture.DefaultRelease()
                .WithId(expectedReleaseId)
                .WithPublication(publication)
                .WithYear(year)
                .WithTimePeriodCoverage(timePeriod)
                .Generate();

            var currentReleaseSlug = release.Slug;
            
            // Release Version is not published
            var releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(release)
                .Generate();

            await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
            context.Publications.Add(publication);
            context.Releases.Add(release);
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();

            var releaseVersionService = new ReleaseVersionServiceMockBuilder()
                                            .WhereGetReleaseVersionReturns(releaseVersion.Id);

            var releasePublishingStatusRepository = new ReleasePublishingStatusRepositoryMockBuilder()
                .SetNoReleaseVersionStatus(releaseVersion.Id);

            var adminEventRaiser = new AdminEventRaiserMockBuilder();
            
            var sut = BuildService(
                                    context,
                                    releaseVersionService: releaseVersionService.Build(),
                                    releaseCacheService: new ReleaseCacheServiceMockBuilder().Build(),
                                    publicationCacheService: new PublicationCacheServiceMockBuilder().Build(),
                                    redirectsCacheService: new RedirectsCacheServiceMockBuilder().Build(),
                                    releasePublishingStatusRepository: releasePublishingStatusRepository.Build(),
                                    adminEventRaiser: adminEventRaiser.Build());
            
            var request = new ReleaseUpdateRequest
            {
                Label = label
            };

            // ACT
            var result = await sut.UpdateRelease(release.Id, request);
            
            // ASSERT
            result.AssertRight();

            var newReleaseSlug = NamingUtils.CreateReleaseSlug(year, timePeriod, label);
            Assert.NotEqual(currentReleaseSlug, newReleaseSlug);
            
            adminEventRaiser.Assert.OnReleaseSlugChangedWasNotRaised();
        }
        
        [Fact]
        public async Task GivenLiveRelease_WhenReleaseUpdatedButSlugUnchanged_ThenNoEventRaised()
        {
            // ARRANGE
            var expectedPublicationId = Guid.NewGuid();
            var expectedPublicationSlug = "this-is-the-publication-slug";
            var expectedReleaseId = Guid.NewGuid();
            var currentLabel = "this is the current label";
            var year = 2025;
            var timePeriod = TimeIdentifier.April;
            var currentReleaseSlug = NamingUtils.CreateReleaseSlug(year, timePeriod, currentLabel);
            
            var publication = _dataFixture.DefaultPublication()
                .WithId(expectedPublicationId)
                .WithSlug(expectedPublicationSlug)
                .Generate();

            var release = _dataFixture.DefaultRelease()
                .WithId(expectedReleaseId)
                .WithPublication(publication)
                .WithYear(year)
                .WithTimePeriodCoverage(timePeriod)
                .WithLabel(currentLabel)
                .WithSlug(currentReleaseSlug)
                .Generate();
            
            var releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(release)
                .WithPublished(new DateTime(2025, 04, 01, 09, 16, 00)) // Release Version is live (ie has been published)
                .Generate();

            await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
            context.Publications.Add(publication);
            context.Releases.Add(release);
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();

            var releaseVersionService = new ReleaseVersionServiceMockBuilder()
                                            .WhereGetReleaseVersionReturns(releaseVersion.Id);

            var releasePublishingStatusRepository = new ReleasePublishingStatusRepositoryMockBuilder()
                .SetNoReleaseVersionStatus(releaseVersion.Id);

            var adminEventRaiser = new AdminEventRaiserMockBuilder();

            var sut = BuildService(
                                    context,
                                    releaseVersionService: releaseVersionService.Build(),
                                    releaseCacheService: new ReleaseCacheServiceMockBuilder().Build(),
                                    publicationCacheService: new PublicationCacheServiceMockBuilder().Build(),
                                    redirectsCacheService: new RedirectsCacheServiceMockBuilder().Build(),
                                    releasePublishingStatusRepository: releasePublishingStatusRepository.Build(),
                                    adminEventRaiser: adminEventRaiser.Build());
            
            var request = new ReleaseUpdateRequest
            {
                Label = currentLabel
            };

            // ACT
            var result = await sut.UpdateRelease(release.Id, request);
            
            // ASSERT
            result.AssertRight();
            adminEventRaiser.Assert.OnReleaseSlugChangedWasNotRaised();
        }

        [Fact]
        public async Task GivenReleaseExists_NewReleaseSlugIsValidated()
        {
            Release release = _dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication());

            await using var context = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
            context.Releases.Add(release);
            await context.SaveChangesAsync();

            var releaseSlugValidator = new ReleaseSlugValidatorMockBuilder();

            var sut = BuildService(
                context,
                releaseSlugValidator: releaseSlugValidator.Build());

            var newLabel = "initial";

            var request = new ReleaseUpdateRequest
            {
                Label = newLabel
            };

            // ACT
            await sut.UpdateRelease(release.Id, request);

            // ASSERT
            var expectedNewReleaseSlug = NamingUtils.CreateReleaseSlug(
                year: release.Year,
                timePeriodCoverage: release.TimePeriodCoverage,
                label: newLabel);

            releaseSlugValidator.Assert.ValidateNewSlugWasCalled(
                expectedNewReleaseSlug: expectedNewReleaseSlug,
                expectedPublicationId: release.PublicationId,
                expectedReleaseId: release.Id);
        }
    }

    private static ReleaseService BuildService(
        ContentDbContext contentDbContext,
        IReleaseVersionService? releaseVersionService = null,
        IReleaseCacheService? releaseCacheService = null,
        IPublicationCacheService? publicationCacheService = null,
        IReleasePublishingStatusRepository? releasePublishingStatusRepository = null,
        IRedirectsCacheService? redirectsCacheService = null,
        IAdminEventRaiser? adminEventRaiser = null,
        IReleaseSlugValidator? releaseSlugValidator = null)
    {
        var userService = AlwaysTrueUserService();

        userService
            .Setup(s => s.GetUserId())
            .Returns(User.Id);

        return new ReleaseService(
            context: contentDbContext,
            userService: userService.Object,
            releaseVersionService: releaseVersionService ?? Mock.Of<IReleaseVersionService>(Strict),
            releaseCacheService: releaseCacheService ?? Mock.Of<IReleaseCacheService>(Strict),
            publicationCacheService: publicationCacheService ?? Mock.Of<IPublicationCacheService>(Strict),
            releasePublishingStatusRepository: releasePublishingStatusRepository ?? Mock.Of<IReleasePublishingStatusRepository>(Strict),
            redirectsCacheService: redirectsCacheService ?? Mock.Of<IRedirectsCacheService>(Strict),
            adminEventRaiser: adminEventRaiser ?? new AdminEventRaiserMockBuilder().Build(),
            guidGenerator: new SequentialGuidGenerator(),
            releaseSlugValidator: releaseSlugValidator ?? new ReleaseSlugValidatorMockBuilder().Build()
        );
    }
}
