#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions.AssertExtensions;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using ReleaseSubject = GovUk.Education.ExploreEducationStatistics.Data.Model.ReleaseSubject;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseAmendmentServiceTests
    {
        private readonly Guid _userId = Guid.NewGuid();
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task CreateReleaseAmendment()
        {
            var originalCreatedBy = new User
            {
                Id = Guid.NewGuid()
            };

            var amendmentCreator = new User
            {
                Id = _userId
            };

            var dataBlockParents = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(() => _fixture
                    .DefaultDataBlockVersion()
                    .WithVersion(0)
                    .Generate())
                .GenerateList(3);

            var dataBlock1Parent = dataBlockParents[0];
            var dataBlock2Parent = dataBlockParents[1];
            var dataBlock3Parent = dataBlockParents[2];

            var originalRelease = _fixture
                .DefaultRelease()
                .WithCreated(
                    created: DateTime.UtcNow.AddDays(-2),
                    createdById: originalCreatedBy.Id)
                .WithPublishScheduled(DateTime.Now.AddDays(1))
                .WithNextReleaseDate(new PartialDate {Day = "1", Month = "1", Year = "2040"})
                .WithPublished(DateTime.UtcNow.AddDays(-1))
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPreviousVersionId(Guid.NewGuid())
                .WithYear(2035)
                .WithType(ReleaseType.OfficialStatistics)
                .WithPublication(_fixture
                    .DefaultPublication()
                    .Generate())
                .WithReleaseStatuses(ListOf(
                    new ReleaseStatus
                    {
                        InternalReleaseNote = "Release note",
                        Created = DateTime.UtcNow
                    }))
                .WithVersion(2)
                .WithTimePeriodCoverage(TimeIdentifier.March)
                .WithNotifiedOn(DateTime.UtcNow.AddDays(-4))
                .WithNotifySubscribers(true)
                .WithUpdatePublishedDate(true)
                .WithPreReleaseAccessList("Some Pre-release details")
                .WithRelatedInformation(ListOf<Link>(
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Description = "Link 1",
                        Url = "URL 1"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Description = "Link 2",
                        Url = "URL 2"
                    }))
                .WithUpdates(ListOf<Update>(new ()
                    {
                        Id = Guid.NewGuid(),
                        On = DateTime.UtcNow.AddDays(-4),
                        Reason = "Reason 1",
                        Created = DateTime.UtcNow.AddDays(-6),
                        CreatedById = Guid.NewGuid(),
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        On = DateTime.UtcNow.AddDays(-5),
                        Reason = "Reason 2",
                        Created = DateTime.UtcNow.AddDays(-2),
                        CreatedById = Guid.NewGuid(),
                    }))
                .WithKeyStatistics(ListOf<KeyStatistic>(
                    new KeyStatisticText { Title = "key stat text", },
                    new KeyStatisticDataBlock
                    {
                        DataBlock = dataBlock3Parent.LatestPublishedVersion!.ContentBlock,
                    }))
                .WithContent(_fixture
                    .DefaultContentSection()
                    .ForIndex(0, s => s
                        .SetContentBlocks(ListOf<ContentBlock>(_fixture
                            .DefaultHtmlBlock()
                            .WithBody("<div></div>")
                            .WithComments(new List<Comment>
                                {
                                    new()
                                    {
                                        Id = Guid.NewGuid(),
                                        Content = "Comment 1 Text"
                                    },
                                    new()
                                    {
                                        Id = Guid.NewGuid(),
                                        Content = "Comment 2 Text"
                                    }
                                })
                            .Generate(),
                            dataBlock1Parent.LatestPublishedVersion!.ContentBlock,
                            new EmbedBlockLink
                            {
                                Id = Guid.NewGuid(),
                                Order = 3,
                                EmbedBlock = new EmbedBlock
                                {
                                    Title = "Test EmbedBlockTitle",
                                    Url = "https://www.test.com/embedBlock",
                                },
                                Comments = new List<Comment>
                                {
                                    new()
                                    {
                                        Id = Guid.NewGuid(),
                                        Content = "Embed block comment"
                                    },
                                },
                            })))
                    .ForIndex(1, s => s
                        .SetContentBlocks(ListOf<ContentBlock>(
                            new MarkDownBlock
                            {
                                Id = Guid.NewGuid(),
                                Body = "Text",
                                Comments = new List<Comment>
                                {
                                    new()
                                    {
                                        Id = Guid.NewGuid(),
                                        Content = "Inset Comment 1 Text"
                                    }
                                }
                            })))
                    .ForIndex(2, s => s
                        .SetType(ContentSectionType.RelatedDashboards)
                        .SetContentBlocks(_fixture
                            .DefaultHtmlBlock()
                            .WithComments(ListOf(
                                new Comment()
                                {
                                    Id = Guid.NewGuid(),
                                    Content = "RelatedDashboards comment"
                                }))
                            .Generate(1)))
                    .GenerateList())
                .WithDataBlockVersions(dataBlockParents
                    .Select(dataBlockParent => dataBlockParent.LatestPublishedVersion!))
                .WithKeyStatistics(ListOf<KeyStatistic>(
                    new KeyStatisticText { Title = "key stat text", },
                    new KeyStatisticDataBlock
                    {
                        DataBlock = dataBlock3Parent.LatestPublishedVersion!.ContentBlock,
                    }))
                .WithReleaseSummaryContent(_fixture
                    .DefaultHtmlBlock()
                    .Generate(2))
                .WithHeadlinesContent(_fixture
                    .DefaultHtmlBlock()
                    .Generate(2))
                .WithRelatedDashboardContent(_fixture
                    .DefaultHtmlBlock()
                    .Generate(2))
                .WithFeaturedTables(ListOf<FeaturedTable>(
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Featured table 1",
                        Description = "Featured table 1 description",
                        Order = 1,
                        DataBlock = dataBlock1Parent.LatestPublishedVersion!.ContentBlock,
                        DataBlockId = dataBlock1Parent.LatestPublishedVersion!.Id,
                        DataBlockParent = dataBlock1Parent,
                        DataBlockParentId = dataBlock1Parent.Id,
                        Created = new DateTime(2023, 01, 01),
                        Updated = new DateTime(2023, 01, 02),
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Featured table 2",
                        Description = "Featured table 2 description",
                        Order = 2,
                        DataBlock = dataBlock2Parent.LatestPublishedVersion!.ContentBlock,
                        DataBlockId = dataBlock2Parent.LatestPublishedVersion!.Id,
                        DataBlockParent = dataBlock2Parent,
                        DataBlockParentId = dataBlock2Parent.Id,
                        Created = new DateTime(2023, 01, 01),
                        Updated = new DateTime(2023, 01, 02),
                    }))
                .Generate();

            var approverReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.Approver,
                Release = originalRelease,
                ReleaseId = originalRelease.Id
            };

            var contributorReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.Contributor,
                Release = originalRelease,
                ReleaseId = originalRelease.Id
            };

            var deletedReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.Lead,
                Release = originalRelease,
                ReleaseId = originalRelease.Id,
                Deleted = DateTime.UtcNow,
                DeletedById = Guid.NewGuid(),
            };

            var prereleaseReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.PrereleaseViewer,
                Release = originalRelease,
                ReleaseId = originalRelease.Id,
                Deleted = DateTime.UtcNow,
                DeletedById = Guid.NewGuid(),
            };

            var userReleaseRoles = new List<UserReleaseRole>
            {
                approverReleaseRole,
                contributorReleaseRole,
                deletedReleaseRole,
                prereleaseReleaseRole
            };

            var dataFile1 = new File
            {
                Id = Guid.NewGuid(),
                Filename = "Filename 1",
                SubjectId = Guid.NewGuid()
            };

            var dataFile2 = new File
            {
                Id = Guid.NewGuid(),
                Filename = "Filename 2",
                SubjectId = Guid.NewGuid()
            };

            var releaseFiles = new List<ReleaseFile>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Release = originalRelease,
                    ReleaseId = originalRelease.Id,
                    File = dataFile1,
                    FileId = dataFile1.Id
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Release = originalRelease,
                    ReleaseId = originalRelease.Id,
                    File = dataFile2,
                    FileId = dataFile2.Id
                }
            };

            var subject = _fixture
                .DefaultSubject()
                .Generate();

            var releaseSubject = new ReleaseSubject
            {
                ReleaseId = originalRelease.Id,
                SubjectId = subject.Id,
                Created = DateTime.UtcNow.AddDays(-2),
                Updated = DateTime.UtcNow.AddDays(-1),
                FilterSequence = new List<FilterSequenceEntry>
                {
                    new(
                        Guid.NewGuid(),
                        new List<FilterGroupSequenceEntry>
                        {
                            new(Guid.NewGuid(), new List<Guid> { Guid.NewGuid() })
                        }
                    )
                },
                IndicatorSequence = new List<IndicatorGroupSequenceEntry>
                {
                    new(
                        Guid.NewGuid(),
                        new List<Guid> { Guid.NewGuid() }
                    )
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(originalRelease);
                await contentDbContext.AddRangeAsync(originalCreatedBy, amendmentCreator);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRoles);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Release.Add(new Data.Model.Release
                {
                    Id = originalRelease.Id,
                    PublicationId = originalRelease.PublicationId,
                });

                statisticsDbContext.Subject.AddRange(subject);
                statisticsDbContext.ReleaseSubject.AddRange(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var amendmentId = Guid.Empty;
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var releaseAmendmentService = BuildService(
                    contentDbContext,
                    statisticsDbContext);

                // Method under test
                var result = await releaseAmendmentService.CreateReleaseAmendment(originalRelease.Id);
                var viewModel = result.AssertRight();

                Assert.NotEqual(Guid.Empty, viewModel.Id);
                Assert.NotEqual(originalRelease.Id, viewModel.Id);
                amendmentId = viewModel.Id;
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var amendment = RetrieveAmendment(contentDbContext, amendmentId);

                // Check the values that we expect to have been copied over successfully from the original Release.
                amendment.AssertDeepEqualTo(originalRelease, Except<Release>(
                    r => r.Id,
                    r => r.Amendment,
                    r => r.Publication,
                    r => r.PreviousVersion!,
                    r => r.PreviousVersionId!,
                    r => r.Version,
                    r => r.Published!,
                    r => r.PublishScheduled!,
                    r => r.Live,
                    r => r.ApprovalStatus,
                    r => r.Created,
                    r => r.CreatedBy,
                    r => r.CreatedById,
                    r => r.NotifiedOn!,
                    r => r.NotifySubscribers,
                    r => r.UpdatePublishedDate,
                    r => r.LatestInternalReleaseNote!,
                    r => r.RelatedInformation,
                    r => r.Updates,
                    r => r.Content,
                    r => r.ReleaseStatuses,
                    r => r.DataBlockVersions,
                    r => r.KeyStatistics,
                    r => r.GenericContent,
                    r => r.HeadlinesSection,
                    r => r.KeyStatisticsSecondarySection,
                    r => r.RelatedDashboardsSection,
                    r => r.SummarySection,
                    r => r.FeaturedTables));

                // Check fields that should be set to new values for an amendment, rather than copied from the original
                // Release.
                Assert.NotEqual(Guid.Empty, amendment.Id);
                Assert.True(amendment.Amendment);
                Assert.Equal(originalRelease.PublicationId, amendment.PublicationId);
                Assert.Equal(originalRelease.Id, amendment.PreviousVersion?.Id);
                Assert.Equal(originalRelease.Id, amendment.PreviousVersionId);
                Assert.Equal(originalRelease.Version + 1, amendment.Version);
                Assert.Null(amendment.Published);
                Assert.Null(amendment.PublishScheduled);
                Assert.False(amendment.Live);
                Assert.Equal(ReleaseApprovalStatus.Draft, amendment.ApprovalStatus);
                amendment.Created.AssertUtcNow(withinMillis: 1500);
                Assert.Equal(amendmentCreator.Id, amendment.CreatedById);
                Assert.Null(amendment.NotifiedOn);
                Assert.False(amendment.NotifySubscribers);
                Assert.False(amendment.UpdatePublishedDate);
                Assert.Null(amendment.LatestInternalReleaseNote);

                // Check Related Information links have been copied over.
                Assert.Equal(2, amendment.RelatedInformation.Count);
                amendment.RelatedInformation.ForEach(amendedLink =>
                {
                    var index = amendment.RelatedInformation.IndexOf(amendedLink);
                    var originalLink = originalRelease.RelatedInformation[index];
                    AssertAmendedLinkCorrect(amendedLink, originalLink);
                });

                // Check Content Updates have been copied over.
                Assert.Equal(2, amendment.Updates.Count);
                amendment.Updates.ForEach(amendedUpdate =>
                {
                    var index = amendment.Updates.IndexOf(amendedUpdate);
                    var originalUpdate = originalRelease.Updates[index];
                    AssertAmendedUpdateCorrect(amendedUpdate, originalUpdate, amendment);
                });

                // Assert that the original DataBlocks are cloned into the Release Amendment.
                var amendmentDataBlockVersions = amendment.DataBlockVersions.ToList();
                Assert.Equal(3, amendmentDataBlockVersions.Count);

                // Assert that each DataBlock has a new version created by the amendment process.
                // The new DataBlockVersion should now be the LatestDraftVersion of its parent but not
                // the LatestPublishedVersion (which should not have changed).
                amendmentDataBlockVersions.ForEach(dataBlockVersion =>
                {
                    var dataBlockVersionsForParent = contentDbContext
                        .DataBlockVersions
                        .Where(version => version.DataBlockParentId == dataBlockVersion.DataBlockParentId)
                        .ToList();

                    Assert.Equal(2, dataBlockVersionsForParent.Count);
                    Assert.Equal(0, dataBlockVersionsForParent[0].Version);
                    Assert.Equal(1, dataBlockVersionsForParent[1].Version);

                    Assert.Equal(dataBlockVersion, dataBlockVersion.DataBlockParent.LatestDraftVersion);
                    Assert.NotEqual(dataBlockVersion, dataBlockVersion.DataBlockParent.LatestPublishedVersion);
                });

                var amendmentContentBlock1 = GetMatchingDataBlock(amendmentDataBlockVersions, dataBlock1Parent);
                var amendmentContentBlock2 = GetMatchingDataBlock(amendmentDataBlockVersions, dataBlock2Parent);
                var amendmentContentBlock3 = GetMatchingDataBlock(amendmentDataBlockVersions, dataBlock3Parent);

                var amendmentContentBlock1InContent = amendment.Content[0].Content[0];

                // Check that the DataBlock that is included in this Release amendment's Content is successfully
                // identified as the exact same DataBlock that is attached to the Release amendment through the
                // additional "Release.ContentBlocks" relationship (which is used to determine which Data Blocks
                // belong to which Release when a Data Block has not yet been - or is removed from - the Release's
                // Content
                Assert.NotEqual(dataBlock1Parent.LatestDraftVersion!.Id, amendmentContentBlock1.Id);
                Assert.Equal(dataBlock1Parent.LatestDraftVersion.Name, amendmentContentBlock1.Name);
                Assert.Equal(amendmentContentBlock1, amendmentContentBlock1InContent);

                // and check that the Data Block that is not yet included in any content is copied across OK still
                Assert.NotEqual(dataBlock2Parent.LatestDraftVersion!.Id, amendmentContentBlock2.Id);

                // and check DataBlock previously associated with key stat is copied correctly
                Assert.NotEqual(dataBlock3Parent.LatestDraftVersion!.Id, amendmentContentBlock3.Id);

                // Check Key Statistics have been copied over.
                Assert.Equal(2, amendment.KeyStatistics.Count);
                var amendmentKeyStatText = Assert.IsType<KeyStatisticText>(amendment
                    .KeyStatistics.Find(ks => ks.GetType() == typeof(KeyStatisticText)));
                Assert.Equal((
                    originalRelease.KeyStatistics[0] as KeyStatisticText)!.Title, amendmentKeyStatText.Title);
                Assert.NotEqual(originalRelease.KeyStatistics[0].Id, amendmentKeyStatText.Id);

                var amendmentKeyStatDataBlock = Assert.IsType<KeyStatisticDataBlock>(amendment
                    .KeyStatistics.Find(ks => ks.GetType() == typeof(KeyStatisticDataBlock)));
                Assert.Equal(dataBlock3Parent.LatestDraftVersion!.Name, amendmentKeyStatDataBlock.DataBlock.Name);
                Assert.NotEqual(originalRelease.KeyStatistics[1].Id, amendmentKeyStatDataBlock.Id);
                Assert.NotEqual(dataBlock3Parent.LatestDraftVersion.Id, amendmentKeyStatDataBlock.DataBlockId);
                Assert.Equal(amendmentContentBlock3.Id, amendmentKeyStatDataBlock.DataBlockId);

                // Check generic Release Content has been copied over (both ContentSections and ContentBlocks).
                Assert.Equal(2, amendment.GenericContent.Count());
                amendment.GenericContent.ForEach(amendedContentSection =>
                {
                    var index = amendment.Content.IndexOf(amendedContentSection);
                    var originalContentSection = originalRelease.Content[index];
                    AssertAmendedContentSectionCorrect(amendment, amendedContentSection, originalContentSection);
                });

                // Check Headlines have been copied over OK.
                AssertAmendedContentSectionCorrect(
                    amendment,
                    amendment.HeadlinesSection,
                    originalRelease.HeadlinesSection);

                // Check Key Statistics ContentSection has been copied over OK.
                // TODO - not sure if having a Key Statistics ContentSection serves any purpose now that
                // KeyStatisticsText and KeyStatisticsDataBlock are no longer ContentBlocks themselves.
                AssertAmendedContentSectionCorrect(
                    amendment,
                    amendment.KeyStatisticsSecondarySection,
                    originalRelease.KeyStatisticsSecondarySection);

                // Check Related Dashboards have been copied over OK.
                AssertAmendedContentSectionCorrect(
                    amendment,
                    amendment.RelatedDashboardsSection,
                    originalRelease.RelatedDashboardsSection);

                // Check Summary section has been copied over OK.
                AssertAmendedContentSectionCorrect(
                    amendment,
                    amendment.SummarySection,
                    originalRelease.SummarySection);

                // Check EmbedBlocks have been copied over OK.
                var amendmentEmbedBlockLink = await contentDbContext
                    .ContentBlocks
                    .Where(block => block.ReleaseId == amendment.Id)
                    .OfType<EmbedBlockLink>()
                    .SingleAsync();

                var originalEmbedBlockLink = Assert.IsType<EmbedBlockLink>(originalRelease.Content[0].Content[2]);
                Assert.NotEqual(originalEmbedBlockLink.Id, amendmentEmbedBlockLink.Id);
                Assert.NotEqual(originalEmbedBlockLink.EmbedBlockId, amendmentEmbedBlockLink.EmbedBlockId);
                Assert.Equal(originalEmbedBlockLink.EmbedBlock.Title, amendmentEmbedBlockLink.EmbedBlock.Title);
                Assert.Equal(originalEmbedBlockLink.EmbedBlock.Url, amendmentEmbedBlockLink.EmbedBlock.Url);

                var amendmentReleaseRoles = contentDbContext
                    .UserReleaseRoles
                    .AsQueryable()
                    .IgnoreQueryFilters() // See if deletedAmendmentRole is also copied
                    .Where(r => r.ReleaseId == amendment.Id)
                    .ToList();

                // Expect one less UserReleaseRole on the Amendment, as the Pre-release role shouldn't be copied over
                Assert.Equal(userReleaseRoles.Count - 1, amendmentReleaseRoles.Count);
                var approverAmendmentRole = amendmentReleaseRoles.First(r => r.Role == ReleaseRole.Approver);
                AssertAmendedReleaseRoleCorrect(approverReleaseRole, approverAmendmentRole, amendment);

                var contributorAmendmentRole = amendmentReleaseRoles.First(r => r.Role == ReleaseRole.Contributor);
                Assert.NotEqual(contributorReleaseRole.Id, contributorAmendmentRole.Id);
                AssertAmendedReleaseRoleCorrect(contributorReleaseRole, contributorAmendmentRole, amendment);

                var deletedAmendmentRole = amendmentReleaseRoles.First(r => r.Deleted != null);
                Assert.NotEqual(deletedReleaseRole.Id, deletedAmendmentRole.Id);
                AssertAmendedReleaseRoleCorrect(deletedReleaseRole, deletedAmendmentRole, amendment);

                var amendmentDataFiles = contentDbContext
                    .ReleaseFiles
                    .Include(f => f.File)
                    .Where(f => f.ReleaseId == amendment.Id)
                    .ToList();

                // Check Release Files have been copied over OK.
                Assert.Equal(releaseFiles.Count, amendmentDataFiles.Count);

                var amendmentDataFile = amendmentDataFiles[0];
                var originalFile = releaseFiles.First(f =>
                    f.File.Filename == amendmentDataFile.File.Filename);
                AssertAmendedReleaseFileCorrect(originalFile, amendmentDataFile, amendment);

                var amendmentDataFile2 = amendmentDataFiles[1];
                var originalFile2 = releaseFiles.First(f =>
                    f.File.Filename == amendmentDataFile2.File.Filename);
                AssertAmendedReleaseFileCorrect(originalFile2, amendmentDataFile2, amendment);

                // Check Featured Tables have been copied over OK.
                Assert.Equal(2, amendment.FeaturedTables.Count);
                amendment.FeaturedTables.ForEach((amendedTable, index) =>
                {
                    var originalTable = originalRelease.FeaturedTables[index];
                    amendedTable.AssertDeepEqualTo(originalTable, Except<FeaturedTable>(
                        ft => ft.Id,
                        ft => ft.DataBlock,
                        ft => ft.DataBlockId,
                        // Note that we're ignoring DataBlockParent here only, not DataBlockParentId.
                        // This is because the LatestPublishedVersion and LatestDraftVersion hanging from
                        // the representation of DataBlockParent on the amendment is more up-to-date than
                        // that of the original Featured Table's setup state since going through the amendment
                        // process. We expect both versions of the FeaturedTable to have the same
                        // DataBlockParentId though.
                        ft => ft.DataBlockParent,
                        ft => ft.Release,
                        ft => ft.ReleaseId,
                        ft => ft.Created,
                        ft => ft.CreatedById!,
                        ft => ft.Updated!));

                    Assert.NotEqual(Guid.Empty, amendedTable.Id);
                    Assert.NotEqual(Guid.Empty, amendedTable.DataBlockParentId);

                    // Expect the DataBlock referred to in the amended FeaturedTable to be the
                    // DataBlock from the LatestDraftVersion of the associated DataBlockParent rather
                    // than the LatestPublishedVersion.
                    Assert.Equal(
                        amendedTable.DataBlockParent.LatestDraftVersion!.ContentBlock,
                        amendedTable.DataBlock);

                    Assert.Equal(amendment, amendedTable.Release);
                    Assert.Equal(amendment.Id, amendedTable.ReleaseId);
                    amendedTable.Created.AssertUtcNow();
                    Assert.Equal(_userId, amendedTable.CreatedById);
                    Assert.Null(amendedTable.Updated);
                    Assert.Null(amendedTable.UpdatedById);
                });
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // Check the Statistics Release has been amended OK.  It should have the same Id as the new Content
                // Release amendment.
                var statsReleaseAmendment = statisticsDbContext.Release.SingleOrDefault(r => r.Id == amendmentId);
                Assert.NotNull(statsReleaseAmendment);
                Assert.Equal(originalRelease.PublicationId, statsReleaseAmendment.PublicationId);

                // Check that Subjects have been linked to the new amendment OK.
                var releaseSubjectLinks = statisticsDbContext
                    .ReleaseSubject
                    .AsQueryable()
                    .Where(r => r.ReleaseId == amendmentId)
                    .ToList();

                var releaseSubjectAmendment = Assert.Single(releaseSubjectLinks);

                releaseSubjectAmendment.AssertDeepEqualTo(releaseSubject, Except<ReleaseSubject>(
                    rs => rs.Created!,
                    rs => rs.Updated!,
                    // SubjectId will be the same despite a different instance of Subject itself.
                    rs => rs.Subject,
                    rs => rs.Release,
                    rs => rs.ReleaseId));

                releaseSubjectAmendment.Created.AssertUtcNow(withinMillis: 1500);
                Assert.Null(releaseSubjectAmendment.Updated);
                Assert.Equal(amendmentId, releaseSubjectAmendment.ReleaseId);
            }
        }

        private static Release RetrieveAmendment(ContentDbContext contentDbContext, Guid amendmentId)
        {
            return contentDbContext
                .Releases
                .Include(release => release.PreviousVersion)
                .Include(release => release.Publication)
                .Include(release => release.Content)
                .ThenInclude(section => section.Content)
                .ThenInclude(section => section.Comments)
                .Include(release => release.Content)
                .ThenInclude(section => section.Content)
                .ThenInclude(section => (section as EmbedBlockLink)!.EmbedBlock)
                .Include(release => release.DataBlockVersions)
                .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlockParent)
                .ThenInclude(dataBlockParent => dataBlockParent.LatestDraftVersion)
                .Include(release => release.DataBlockVersions)
                .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlockParent)
                .ThenInclude(dataBlockParent => dataBlockParent.LatestPublishedVersion)
                .Include(release => release.DataBlockVersions)
                .Include(release => release.Updates)
                .Include(release => release.KeyStatistics)
                .ThenInclude(keyStat => (keyStat as KeyStatisticDataBlock)!.DataBlock)
                .Include(release => release.FeaturedTables)
                .First(r => r.Id == amendmentId);
        }

        [Fact]
        public async Task FiltersCommentsFromContent()
        {
            var htmlBlock1Body = @"
                <p>
                    Content 1 <comment-start name=""comment-1""></comment-start>goes here<comment-end name=""comment-1""></comment-end>
                </p>
                <ul>
                    <li><comment-start name=""comment-2""/>Content 2<comment-end name=""comment-2""/></li>
                    <li><commentplaceholder-start name=""comment-3""/>Content 3<commentplaceholder-end name=""comment-3""/></li>
                    <li><resolvedcomment-start name=""comment-4""/>Content 4<resolvedcomment-end name=""comment-4""/></li>
                </ul>".TrimIndent();

            var expectedHtmlBlock1Body = @"
                <p>
                    Content 1 goes here
                </p>
                <ul>
                    <li>Content 2</li>
                    <li>Content 3</li>
                    <li>Content 4</li>
                </ul>".TrimIndent();

            var htmlBlock2Body = $@"
                    <p>
                        Content block 2
                        <comment-start name=""comment-1""></comment-start>
                            Content 1
                        <comment-end name=""comment-1""></comment-end>

                        Content 2
                    </p>".TrimIndent();

            var expectedHtmlBlock2Body = $@"
                    <p>
                        Content block 2
                        
                            Content 1
                        

                        Content 2
                    </p>".TrimIndent();

            var htmlBlock3Body = $@"
                    <p>
                        Content block 3
                        <comment-start name=""comment-1""></comment-start>
                            Content 1
                        <comment-end name=""comment-1""></comment-end>
                    </p>".TrimIndent();

            var expectedHtmlBlock3Body = $@"
                    <p>
                        Content block 3
                        
                            Content 1
                        
                    </p>".TrimIndent();

            var originalRelease = _fixture
                .DefaultRelease()
                .WithPublication(_fixture
                    .DefaultPublication()
                    .Generate())
                .WithCreated(createdById: _userId)
                .WithContent(_fixture
                    .DefaultContentSection()
                    .ForIndex(0, section => section.SetContentBlocks(_fixture
                        .DefaultHtmlBlock()
                        .ForIndex(0, block => block.SetBody(htmlBlock1Body))
                        .ForIndex(1, block => block.SetBody(htmlBlock2Body))
                        .GenerateList()))
                    // Test that we are amending content across multiple sections.
                    .ForIndex(1, s => s.SetContentBlocks(_fixture
                        .DefaultHtmlBlock()
                        .WithBody(htmlBlock3Body)
                        .Generate(1)))
                    .ForIndex(2, s => s.SetType(ContentSectionType.RelatedDashboards))
                    .GenerateList())
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(originalRelease);
                await contentDbContext.AddAsync(new User
                {
                    Id = _userId
                });
                await contentDbContext.SaveChangesAsync();
            }

            var amendmentId = Guid.Empty;
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseAmendmentService = BuildService(
                    contentDbContext,
                    statisticsDbContext: InMemoryStatisticsDbContext());

                // Method under test
                var result = await releaseAmendmentService.CreateReleaseAmendment(originalRelease.Id);
                var amendment = result.AssertRight();

                Assert.NotEqual(originalRelease.Id, amendment.Id);
                amendmentId = amendment.Id;
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var amendment = RetrieveAmendment(contentDbContext, amendmentId);

                Assert.Equal(3, amendment.Content.Count);
                Assert.Equal(ContentSectionType.Generic, amendment.Content[0].Type);
                Assert.Equal(ContentSectionType.Generic, amendment.Content[1].Type);
                Assert.Equal(ContentSectionType.RelatedDashboards, amendment.Content[2].Type);

                var amendmentContentSection1 = amendment.Content[0];
                var amendmentContentSection2 = amendment.Content[1];

                Assert.Equal(2, amendmentContentSection1.Content.Count);
                var amendmentContentBlock1 = Assert.IsType<HtmlBlock>(amendmentContentSection1.Content[0]);
                var amendmentContentBlock2 = Assert.IsType<HtmlBlock>(amendmentContentSection1.Content[1]);
                var amendmentContentBlock3 = Assert.IsType<HtmlBlock>(Assert.Single(amendmentContentSection2.Content));

                Assert.Equal(expectedHtmlBlock1Body, amendmentContentBlock1.Body);
                Assert.Equal(expectedHtmlBlock2Body, amendmentContentBlock2.Body);
                Assert.Equal(expectedHtmlBlock3Body, amendmentContentBlock3.Body);
            }
        }

        [Fact]
        public async Task NullHtmlBlockBody()
        {
            var originalRelease = _fixture
                .DefaultRelease()
                .WithPublication(_fixture
                    .DefaultPublication()
                    .Generate())
                .WithCreated(createdById: _userId)
                .WithContent(_fixture
                    .DefaultContentSection()
                    .WithContentBlocks(_fixture
                        .DefaultHtmlBlock()
                        .WithBody(null!)
                        .GenerateList(1))
                    .GenerateList(1))
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(originalRelease);
                await contentDbContext.AddAsync(new User
                {
                    Id = _userId
                });
                await contentDbContext.SaveChangesAsync();
            }

            var amendmentId = Guid.Empty;
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseAmendmentService = BuildService(
                    contentDbContext,
                    statisticsDbContext: InMemoryStatisticsDbContext());

                // Method under test
                var result = await releaseAmendmentService.CreateReleaseAmendment(originalRelease.Id);

                var amendment = result.AssertRight();

                Assert.NotEqual(originalRelease.Id, amendment.Id);
                amendmentId = amendment.Id;
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var amendment = RetrieveAmendment(contentDbContext, amendmentId);
                Assert.NotNull(amendment);
            }
        }

        [Fact]
        public async Task CreatesRelatedDashboardsSectionIfNotOnOriginal()
        {
            var originalRelease = _fixture
                .DefaultRelease()
                .WithPublication(_fixture
                    .DefaultPublication()
                    .Generate())
                .WithCreated(createdById: _userId)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(originalRelease);
                await contentDbContext.AddAsync(new User
                {
                    Id = _userId
                });
                await contentDbContext.SaveChangesAsync();
            }

            var amendmentId = Guid.Empty;
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseAmendmentService = BuildService(
                    contentDbContext,
                    statisticsDbContext: InMemoryStatisticsDbContext());

                // Method under test
                var result = await releaseAmendmentService.CreateReleaseAmendment(originalRelease.Id);
                var amendment = result.AssertRight();

                Assert.NotEqual(originalRelease.Id, amendment.Id);
                amendmentId = amendment.Id;
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var amendment = RetrieveAmendment(contentDbContext, amendmentId);

                var relatedDashboardsSection = Assert.Single(amendment.Content);
                Assert.Equal(ContentSectionType.RelatedDashboards, relatedDashboardsSection.Type);
            }
        }

        [Fact]
        public async Task CopyFootnotes()
        {
            var originalRelease = _fixture
                .DefaultRelease()
                .WithPublication(_fixture
                    .DefaultPublication()
                    .Generate())
                .WithCreated(createdById: _userId)
                .WithContent(_fixture
                    .DefaultContentSection()
                    .WithContentBlocks(_fixture
                        .DefaultHtmlBlock()
                        .WithBody(null!)
                        .GenerateList(1))
                    .GenerateList(1))
                .Generate();

            var originalStatsRelease = _fixture
                .DefaultStatsRelease()
                .WithId(originalRelease.Id)
                .Generate();

            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(originalStatsRelease)
                .WithSubject(_fixture
                    .DefaultSubject()
                    .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(1))
                    .WithIndicatorGroups(_fixture.DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .Generate(1)))
                .Generate();

            var releaseFootnotes = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(originalStatsRelease)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetSubjects(ListOf(releaseSubject.Subject))
                        .SetFilters(releaseSubject.Subject.Filters)
                        .SetFilterGroups(releaseSubject.Subject.Filters[0].FilterGroups)
                        .SetFilterItems(releaseSubject.Subject.Filters[0].FilterGroups[0].FilterItems)
                        .SetIndicators(releaseSubject.Subject.IndicatorGroups[0].Indicators))
                    .ForIndex(1, s => s
                        .SetSubjects(ListOf(releaseSubject.Subject)))
                    .GenerateList())
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Releases.AddRangeAsync(originalRelease);
                await contentDbContext.Users.AddRangeAsync(new User
                {
                    Id = _userId
                });

                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.Release.AddRangeAsync(originalStatsRelease);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
                await statisticsDbContext.ReleaseFootnote.AddRangeAsync(releaseFootnotes);

                await statisticsDbContext.SaveChangesAsync();
            }

            var amendmentId = Guid.Empty;
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var releaseAmendmentService = BuildService(
                    contentDbContext,
                    statisticsDbContext);

                // Method under test
                var result = await releaseAmendmentService.CreateReleaseAmendment(originalStatsRelease.Id);
                var viewModel = result.AssertRight();

                Assert.NotEqual(Guid.Empty, viewModel.Id);
                Assert.NotEqual(originalStatsRelease.Id, viewModel.Id);
                amendmentId = viewModel.Id;
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var newFootnotesFromDb = statisticsDbContext
                    .Footnote
                    .Include(f => f.Filters)
                    .Include(f => f.FilterGroups)
                    .Include(f => f.FilterItems)
                    .Include(f => f.Releases)
                    .Include(f => f.Subjects)
                    .Include(f => f.Indicators)
                    .Where(f => f.Releases.FirstOrDefault(r => r.ReleaseId == amendmentId) != null)
                    .OrderBy(f => f.Content)
                    .ToList();

                Assert.Equal(2, newFootnotesFromDb.Count);
                AssertFootnoteDetailsCopiedCorrectly(releaseFootnotes[0].Footnote, newFootnotesFromDb[0]);
                AssertFootnoteDetailsCopiedCorrectly(releaseFootnotes[1].Footnote, newFootnotesFromDb[1]);
            }
        }

        private void AssertFootnoteDetailsCopiedCorrectly(Footnote originalFootnote, Footnote newFootnote)
        {
            Assert.Equal(originalFootnote.Content, newFootnote.Content);
            Assert.Equal(originalFootnote.Order, newFootnote.Order);

            Assert.Equal(originalFootnote
                    .Filters
                    .SelectNullSafe(f => f.FilterId),
                newFootnote
                    .Filters
                    .SelectNullSafe(f => f.FilterId));

            Assert.Equal(
                originalFootnote
                    .FilterGroups
                    .SelectNullSafe(f => f.FilterGroupId),
                newFootnote
                    .FilterGroups
                    .SelectNullSafe(f => f.FilterGroupId));

            Assert.Equal(
                originalFootnote
                    .FilterItems
                    .SelectNullSafe(f => f.FilterItemId),
                newFootnote
                    .FilterItems
                    .SelectNullSafe(f => f.FilterItemId));

            Assert.Equal(
                originalFootnote
                    .Subjects
                    .SelectNullSafe(f => f.SubjectId),
                newFootnote
                    .Subjects
                    .SelectNullSafe(f => f.SubjectId));

            Assert.Equal(
                originalFootnote
                    .Indicators
                    .SelectNullSafe(f => f.IndicatorId),
                newFootnote
                    .Indicators
                    .SelectNullSafe(f => f.IndicatorId));
        }

        private DataBlock GetMatchingDataBlock(List<DataBlockVersion> amendmentDataBlockVersions, DataBlockParent dataBlockToFind)
        {
            return amendmentDataBlockVersions
                .Where(dataBlockVersion => dataBlockVersion.Name == dataBlockToFind.LatestDraftVersion!.Name)
                .Select(dataBlockParent => dataBlockParent.ContentBlock)
                .Single();
        }

        private void AssertAmendedLinkCorrect(Link amendedLink, Link originalLink)
        {
            amendedLink.AssertDeepEqualTo(originalLink, Except<Link>(l => l.Id));
        }

        private void AssertAmendedUpdateCorrect(Update amendedUpdate, Update originalUpdate, Release amendment)
        {
            amendedUpdate.AssertDeepEqualTo(originalUpdate,
                Except<Update>(
                    u => u.Id,
                    u => u.Release,
                    u => u.ReleaseId,
                    u => u.Created!,
                    u => u.CreatedById!));

            Assert.Equal(amendment, amendedUpdate.Release);
            Assert.Equal(amendment.Id, amendedUpdate.ReleaseId);
            amendedUpdate.Created.AssertUtcNow(withinMillis: 1500);
            Assert.Equal(_userId, amendedUpdate.CreatedById);
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void AssertAmendedContentSectionCorrect(
            Release amendment,
            ContentSection amendedSection,
            ContentSection originalSection)
        {
            Assert.Equal(amendment, amendedSection.Release);
            Assert.Equal(amendment.Id, amendedSection.ReleaseId);
            Assert.True(amendedSection.Id != Guid.Empty);
            Assert.NotEqual(originalSection.Id, amendedSection.Id);

            Assert.NotEqual(originalSection.Id, amendedSection.Id);
            Assert.Equal(originalSection.Caption, amendedSection.Caption);
            Assert.Equal(originalSection.Heading, amendedSection.Heading);
            Assert.Equal(originalSection.Order, amendedSection.Order);
            Assert.Equal(originalSection.Type, amendedSection.Type);
            Assert.Equal(originalSection.Content.Count, amendedSection.Content.Count);

            amendedSection.Content.ForEach(amendedBlock =>
            {
                var originalBlock = originalSection.Content.Find(b => b.Order == amendedBlock.Order);
                AssertAmendedContentBlockCorrect(originalBlock, amendedBlock, amendedSection);
            });
        }

        private void AssertAmendedContentBlockCorrect(ContentBlock? originalBlock, ContentBlock amendedBlock,
            ContentSection amendedSection)
        {
            Assert.NotEqual(originalBlock?.Id, amendedBlock.Id);
            Assert.Equal(originalBlock?.Order, amendedBlock.Order);
            Assert.Equal(amendedSection, amendedBlock.ContentSection);
            Assert.Equal(amendedSection.Id, amendedBlock.ContentSectionId);
            Assert.NotEmpty(originalBlock!.Comments);
            Assert.Empty(amendedBlock.Comments);
        }

        private static void AssertAmendedReleaseRoleCorrect(
            UserReleaseRole originalReleaseRole,
            UserReleaseRole amendedReleaseRole,
            Release amendment)
        {
            Assert.NotEqual(originalReleaseRole.Id, amendedReleaseRole.Id);
            Assert.Equal(amendment, amendedReleaseRole.Release);
            Assert.Equal(amendment.Id, amendedReleaseRole.ReleaseId);
            Assert.Equal(originalReleaseRole.UserId, amendedReleaseRole.UserId);
            Assert.Equal(originalReleaseRole.Role, amendedReleaseRole.Role);
            amendedReleaseRole.Created.AssertUtcNow(withinMillis: 1500);
            Assert.Equal(originalReleaseRole.CreatedById, amendedReleaseRole.CreatedById);
            Assert.Equal(originalReleaseRole.Deleted, amendedReleaseRole.Deleted);
            Assert.Equal(originalReleaseRole.DeletedById, amendedReleaseRole.DeletedById);
        }

        private void AssertAmendedReleaseFileCorrect(ReleaseFile originalFile, ReleaseFile amendmentDataFile,
            Release amendment)
        {
            // Assert it's a new link table entry between the Release amendment and the data file reference
            Assert.NotEqual(originalFile.Id, amendmentDataFile.Id);
            Assert.Equal(amendment, amendmentDataFile.Release);
            Assert.Equal(amendment.Id, amendmentDataFile.ReleaseId);
            Assert.Equal(originalFile.Name, amendmentDataFile.Name);
            Assert.Equal(originalFile.Order, amendmentDataFile.Order);

            // And assert that the file referenced is the SAME file reference as linked from the original Release's
            // link table entry
            Assert.Equal(originalFile.File.Id, amendmentDataFile.File.Id);
        }

        private Mock<IUserService> UserServiceMock()
        {
            var userService = AlwaysTrueUserService();

            userService
                .Setup(s => s.GetUserId())
                .Returns(_userId);

            return userService;
        }

        private ReleaseAmendmentService BuildService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IUserService? userService = null)
        {
            return new ReleaseAmendmentService(
                contentDbContext,
                userService ?? UserServiceMock().Object,
                new FootnoteRepository(statisticsDbContext),
                statisticsDbContext);
        }
    }
}
