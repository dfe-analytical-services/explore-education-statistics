#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
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

            var releaseService = BuildReleaseService(Mock.Of<ContentDbContext>());

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
            var releaseVersionServiceMock = new Mock<IReleaseVersionService>();
            releaseVersionServiceMock
                .Setup(rvs => rvs.GetRelease(It.IsAny<Guid>()))
                .ReturnsAsync(releaseVersionViewModel)
                .Callback((Guid releaseVersionId) => newReleaseVersionId = releaseVersionId);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(
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
            var releaseVersionServiceMock = new Mock<IReleaseVersionService>();
            releaseVersionServiceMock
                .Setup(rvs => rvs.GetRelease(It.IsAny<Guid>()))
                .ReturnsAsync(releaseVersionViewModel)
                .Callback((Guid releaseVersionId) => newReleaseVersionId = releaseVersionId);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(
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
    }

    private static ReleaseService BuildReleaseService(
        ContentDbContext contentDbContext,
        IReleaseVersionService? releaseVersionService = null,
        IReleaseCacheService? releaseCacheService = null)
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
            guidGenerator: new SequentialGuidGenerator()
        );
    }
}
