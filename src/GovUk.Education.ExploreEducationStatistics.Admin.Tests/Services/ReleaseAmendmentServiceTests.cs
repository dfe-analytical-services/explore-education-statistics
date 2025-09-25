#nullable enable
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
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Moq;
using Semver;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using ReleaseSubject = GovUk.Education.ExploreEducationStatistics.Data.Model.ReleaseSubject;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseAmendmentServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task CreateReleaseAmendment()
    {
        var originalCreatedBy = new User { Id = Guid.NewGuid() };

        var amendmentCreator = new User { Id = _userId };

        var dataBlockParents = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(() => _fixture
                .DefaultDataBlockVersion()
                .WithVersion(0))
            .GenerateList(3);

        var dataBlock1Parent = dataBlockParents[0];
        var dataBlock2Parent = dataBlockParents[1];
        var dataBlock3Parent = dataBlockParents[2];

        ReleaseVersion originalReleaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture
                .DefaultRelease()
                .WithYear(2035)
                .WithTimePeriodCoverage(TimeIdentifier.March)
                .WithPublication(_fixture
                    .DefaultPublication()))
            .WithCreated(
                created: DateTime.UtcNow.AddDays(-2),
                createdById: originalCreatedBy.Id)
            .WithPublishScheduled(DateTime.Now.AddDays(1))
            .WithNextReleaseDate(new PartialDate
            {
                Day = "1",
                Month = "1",
                Year = "2040"
            })
            .WithPublished(DateTime.UtcNow.AddDays(-1))
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .WithPreviousVersionId(Guid.NewGuid())
            .WithType(ReleaseType.OfficialStatistics)
            .WithPublishingOrganisations(_fixture.DefaultOrganisation()
                .Generate(2))
            .WithReleaseStatuses(_fixture
                .DefaultReleaseStatus()
                .Generate(1))
            .WithVersion(2)
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
            .WithUpdates([
                // 'On' is deliberately set in local time with DateTime.Now to match how ReleaseNoteService creates Updates.
                // TODO EES-6490 Convert 'On' from DateTime to DateTimeOffset
                new Update
                {
                    Id = Guid.NewGuid(),
                    On = DateTime.Now.AddDays(-4),
                    Reason = "Reason 1",
                    Created = DateTime.UtcNow.AddDays(-6),
                    CreatedById = Guid.NewGuid(),
                },
                new Update
                {
                    Id = Guid.NewGuid(),
                    On = DateTime.Now.AddDays(-5),
                    Reason = "Reason 2",
                    Created = DateTime.UtcNow.AddDays(-2),
                    CreatedById = Guid.NewGuid(),
                }
            ])
            .WithKeyStatistics(ListOf<KeyStatistic>(
                new KeyStatisticText
                {
                    Title = "key stat text",
                },
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
                            }),
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
                .ForIndex(1,
                    s => s
                        .SetContentBlocks(ListOf<ContentBlock>(_fixture
                            .DefaultHtmlBlock()
                            .WithBody("<div></div>"))))
                .ForIndex(2, s => s
                    .SetType(ContentSectionType.RelatedDashboards)
                    .SetContentBlocks(_fixture
                        .DefaultHtmlBlock()
                        .WithComments(ListOf(
                            new Comment
                            {
                                Id = Guid.NewGuid(),
                                Content = "RelatedDashboards comment"
                            }))
                        .Generate(1)))
                .GenerateList())
            .WithDataBlockVersions(dataBlockParents
                .Select(dataBlockParent => dataBlockParent.LatestPublishedVersion!))
            .WithKeyStatistics(ListOf<KeyStatistic>(
                new KeyStatisticText
                {
                    Title = "key stat text",
                },
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
                }));

        var approverReleaseRole = new UserReleaseRole
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Role = ReleaseRole.Approver,
            ReleaseVersion = originalReleaseVersion,
            ReleaseVersionId = originalReleaseVersion.Id
        };

        var contributorReleaseRole = new UserReleaseRole
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Role = ReleaseRole.Contributor,
            ReleaseVersion = originalReleaseVersion,
            ReleaseVersionId = originalReleaseVersion.Id
        };

        var prereleaseReleaseRole = new UserReleaseRole
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Role = ReleaseRole.PrereleaseViewer,
            ReleaseVersion = originalReleaseVersion,
            ReleaseVersionId = originalReleaseVersion.Id,
        };

        var userReleaseRoles = new List<UserReleaseRole>
        {
            approverReleaseRole,
            contributorReleaseRole,
            prereleaseReleaseRole
        };

        var dataFile1 = new File
        {
            Id = Guid.NewGuid(),
            Filename = "Filename 1",
            SubjectId = Guid.NewGuid(),
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
                ReleaseVersion = originalReleaseVersion,
                ReleaseVersionId = originalReleaseVersion.Id,
                File = dataFile1,
                FileId = dataFile1.Id,
                FilterSequence =
                [
                    new FilterSequenceEntry(
                        Guid.NewGuid(),
                        [new(Guid.NewGuid(), [Guid.NewGuid()])]
                    )
                ],
                IndicatorSequence =
                [
                    new IndicatorGroupSequenceEntry(
                        Guid.NewGuid(),
                        [Guid.NewGuid()]
                    )
                ],
                Published = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                PublicApiDataSetId = Guid.NewGuid(),
                PublicApiDataSetVersion = SemVersion.Parse("1.0.0", SemVersionStyles.Any),
            },
            new()
            {
                Id = Guid.NewGuid(),
                ReleaseVersion = originalReleaseVersion,
                ReleaseVersionId = originalReleaseVersion.Id,
                File = dataFile2,
                FileId = dataFile2.Id,
                Published = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            }
        };

        Subject subject = _fixture
            .DefaultSubject();

        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersionId = originalReleaseVersion.Id,
            SubjectId = subject.Id,
            Created = DateTime.UtcNow.AddDays(-2),
            Updated = DateTime.UtcNow.AddDays(-1),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(originalReleaseVersion);
            contentDbContext.Users.AddRange(originalCreatedBy, amendmentCreator);
            contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.Add(new Data.Model.ReleaseVersion
            {
                Id = originalReleaseVersion.Id,
                PublicationId = originalReleaseVersion.PublicationId,
            });

            statisticsDbContext.Subject.AddRange(subject);
            statisticsDbContext.ReleaseSubject.AddRange(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        Guid? amendmentId;
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var releaseAmendmentService = BuildService(
                contentDbContext,
                statisticsDbContext);

            // Method under test
            var result = await releaseAmendmentService.CreateReleaseAmendment(originalReleaseVersion.Id);
            var viewModel = result.AssertRight();

            Assert.NotEqual(Guid.Empty, viewModel.Id);
            Assert.NotEqual(originalReleaseVersion.Id, viewModel.Id);
            amendmentId = viewModel.Id;
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var amendment = RetrieveAmendment(contentDbContext, amendmentId.Value);

            // Check the values that we expect to have been copied over successfully from the original Release.
            amendment.AssertDeepEqualTo(
                originalReleaseVersion,
                ignoreProperties:
                [
                    r => r.Id,
                    r => r.Amendment,
                    r => r.Publication,
                    r => r.Release,
                    r => r.PreviousVersion,
                    r => r.PreviousVersionId,
                    r => r.Version,
                    r => r.Published,
                    r => r.PublishScheduled,
                    r => r.Live,
                    r => r.ApprovalStatus,
                    r => r.Created,
                    r => r.CreatedBy,
                    r => r.CreatedById,
                    r => r.NotifiedOn,
                    r => r.NotifySubscribers,
                    r => r.UpdatePublishedDate,
                    r => r.LatestInternalReleaseNote,
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
                    r => r.FeaturedTables
                ]);

            // Check fields that should be set to new values for an amendment, rather than copied from the original
            // Release.
            Assert.NotEqual(Guid.Empty, amendment.Id);
            Assert.True(amendment.Amendment);
            Assert.Equal(originalReleaseVersion.Id, amendment.PreviousVersion?.Id);
            Assert.Equal(originalReleaseVersion.Id, amendment.PreviousVersionId);
            Assert.Equal(originalReleaseVersion.Version + 1, amendment.Version);
            Assert.Null(amendment.Published);
            Assert.Null(amendment.PublishScheduled);
            Assert.False(amendment.Live);
            Assert.Equal(ReleaseApprovalStatus.Draft, amendment.ApprovalStatus);
            Assert.Equal(amendmentCreator.Id, amendment.CreatedById);
            Assert.Null(amendment.NotifiedOn);
            Assert.False(amendment.NotifySubscribers);
            Assert.False(amendment.UpdatePublishedDate);
            Assert.Null(amendment.LatestInternalReleaseNote);

            Assert.NotEqual(originalReleaseVersion.Created, amendment.Created);
            amendment.Created.AssertUtcNow();

            // Check Related Information links have been copied over.
            Assert.Equal(2, amendment.RelatedInformation.Count);
            amendment.RelatedInformation.ForEach(amendedLink =>
            {
                var index = amendment.RelatedInformation.IndexOf(amendedLink);
                var originalLink = originalReleaseVersion.RelatedInformation[index];
                AssertAmendedLinkCorrect(amendedLink, originalLink);
            });

            // Check Content Updates have been copied over.
            Assert.Equal(2, amendment.Updates.Count);
            amendment.Updates.ForEach(amendedUpdate =>
            {
                var index = amendment.Updates.IndexOf(amendedUpdate);
                var originalUpdate = originalReleaseVersion.Updates[index];
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
                originalReleaseVersion.KeyStatistics[0] as KeyStatisticText)!.Title, amendmentKeyStatText.Title);
            Assert.NotEqual(originalReleaseVersion.KeyStatistics[0].Id, amendmentKeyStatText.Id);

            var amendmentKeyStatDataBlock = Assert.IsType<KeyStatisticDataBlock>(amendment
                .KeyStatistics.Find(ks => ks.GetType() == typeof(KeyStatisticDataBlock)));
            Assert.Equal(dataBlock3Parent.LatestDraftVersion!.Name, amendmentKeyStatDataBlock.DataBlock.Name);
            Assert.NotEqual(originalReleaseVersion.KeyStatistics[1].Id, amendmentKeyStatDataBlock.Id);
            Assert.NotEqual(dataBlock3Parent.LatestDraftVersion.Id, amendmentKeyStatDataBlock.DataBlockId);
            Assert.Equal(amendmentContentBlock3.Id, amendmentKeyStatDataBlock.DataBlockId);

            // Check generic Release Content has been copied over (both ContentSections and ContentBlocks).
            Assert.Equal(2, amendment.GenericContent.Count());
            amendment.GenericContent.ForEach(amendedContentSection =>
            {
                var index = amendment.Content.IndexOf(amendedContentSection);
                var originalContentSection = originalReleaseVersion.Content[index];
                AssertAmendedContentSectionCorrect(amendment, amendedContentSection, originalContentSection);
            });

            // Check Headlines have been copied over OK.
            AssertAmendedContentSectionCorrect(
                amendment,
                amendment.HeadlinesSection,
                originalReleaseVersion.HeadlinesSection);

            // Check Key Statistics ContentSection has been copied over OK.
            // TODO - not sure if having a Key Statistics ContentSection serves any purpose now that
            // KeyStatisticsText and KeyStatisticsDataBlock are no longer ContentBlocks themselves.
            AssertAmendedContentSectionCorrect(
                amendment,
                amendment.KeyStatisticsSecondarySection,
                originalReleaseVersion.KeyStatisticsSecondarySection);

            // Check Related Dashboards have been copied over OK.
            AssertAmendedContentSectionCorrect(
                amendment,
                amendment.RelatedDashboardsSection,
                originalReleaseVersion.RelatedDashboardsSection);

            // Check Summary section has been copied over OK.
            AssertAmendedContentSectionCorrect(
                amendment,
                amendment.SummarySection,
                originalReleaseVersion.SummarySection);

            // Check EmbedBlocks have been copied over OK.
            var amendmentEmbedBlockLink = await contentDbContext
                .ContentBlocks
                .OfType<EmbedBlockLink>()
                .Include(embedBlockLink => embedBlockLink.EmbedBlock)
                .SingleAsync(block => block.ReleaseVersionId == amendment.Id);

            var originalEmbedBlockLink =
                Assert.IsType<EmbedBlockLink>(originalReleaseVersion.Content[0].Content[2]);
            Assert.NotEqual(originalEmbedBlockLink.Id, amendmentEmbedBlockLink.Id);
            Assert.NotEqual(originalEmbedBlockLink.EmbedBlockId, amendmentEmbedBlockLink.EmbedBlockId);
            Assert.Equal(originalEmbedBlockLink.EmbedBlock.Title, amendmentEmbedBlockLink.EmbedBlock.Title);
            Assert.Equal(originalEmbedBlockLink.EmbedBlock.Url, amendmentEmbedBlockLink.EmbedBlock.Url);

            var amendmentReleaseRoles = contentDbContext
                .UserReleaseRoles
                .Where(r => r.ReleaseVersionId == amendment.Id)
                .ToList();

            // Expect one less UserReleaseRole on the Amendment, as the Pre-release role shouldn't be copied over
            Assert.Equal(userReleaseRoles.Count - 1, amendmentReleaseRoles.Count);
            var approverAmendmentRole = amendmentReleaseRoles.Single(r => r.Role == ReleaseRole.Approver);
            AssertAmendedReleaseRoleCorrect(approverReleaseRole, approverAmendmentRole, amendment);

            var contributorAmendmentRole = amendmentReleaseRoles.Single(r => r.Role == ReleaseRole.Contributor);
            Assert.NotEqual(contributorReleaseRole.Id, contributorAmendmentRole.Id);
            AssertAmendedReleaseRoleCorrect(contributorReleaseRole, contributorAmendmentRole, amendment);

            var amendmentDataFiles = contentDbContext
                .ReleaseFiles
                .Include(f => f.File)
                .Where(f => f.ReleaseVersionId == amendment.Id)
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
                var originalTable = originalReleaseVersion.FeaturedTables[index];
                amendedTable.AssertDeepEqualTo(
                    originalTable,
                    ignoreProperties:
                    [
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
                        ft => ft.ReleaseVersion,
                        ft => ft.ReleaseVersionId,
                        ft => ft.Created,
                        ft => ft.CreatedById,
                        ft => ft.Updated
                    ]);

                Assert.NotEqual(Guid.Empty, amendedTable.Id);
                Assert.NotEqual(Guid.Empty, amendedTable.DataBlockParentId);

                // Expect the DataBlock referred to in the amended FeaturedTable to be the
                // DataBlock from the LatestDraftVersion of the associated DataBlockParent rather
                // than the LatestPublishedVersion.
                Assert.Equal(
                    amendedTable.DataBlockParent.LatestDraftVersion!.ContentBlock,
                    amendedTable.DataBlock);

                Assert.Equal(amendment, amendedTable.ReleaseVersion);
                Assert.Equal(amendment.Id, amendedTable.ReleaseVersionId);

                Assert.NotEqual(originalTable.Created, amendedTable.Created);
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
            var statsReleaseVersionAmendment =
                statisticsDbContext.ReleaseVersion.SingleOrDefault(rv => rv.Id == amendmentId);
            Assert.NotNull(statsReleaseVersionAmendment);
            Assert.Equal(originalReleaseVersion.PublicationId, statsReleaseVersionAmendment.PublicationId);

            // Check that Subjects have been linked to the new amendment OK.
            var releaseSubjectLinks = statisticsDbContext
                .ReleaseSubject
                .AsQueryable()
                .Where(r => r.ReleaseVersionId == amendmentId)
                .ToList();

            var releaseSubjectAmendment = Assert.Single(releaseSubjectLinks);

            releaseSubjectAmendment.AssertDeepEqualTo(
                releaseSubject,
                ignoreProperties:
                [
                    rs => rs.Created,
                    rs => rs.Updated,
                    // SubjectId will be the same despite a different instance of Subject itself.
                    rs => rs.Subject,
                    rs => rs.ReleaseVersion,
                    rs => rs.ReleaseVersionId
                ]);

            Assert.NotEqual(releaseSubject.Created, releaseSubjectAmendment.Created);
            releaseSubjectAmendment.Created.AssertUtcNow();

            Assert.Null(releaseSubjectAmendment.Updated);

            Assert.Equal(amendmentId, releaseSubjectAmendment.ReleaseVersionId);
        }
    }

    private static ReleaseVersion RetrieveAmendment(ContentDbContext contentDbContext, Guid amendmentId)
    {
        return contentDbContext
            .ReleaseVersions
            .Include(releaseVersion => releaseVersion.PreviousVersion)
            .Include(releaseVersion => releaseVersion.Publication)
            .Include(releaseVersion => releaseVersion.Release)
            .Include(releaseVersion => releaseVersion.PublishingOrganisations)
            .Include(releaseVersion => releaseVersion.Content)
            .ThenInclude(section => section.Content)
            .ThenInclude(section => section.Comments)
            .Include(releaseVersion => releaseVersion.Content)
            .ThenInclude(section => section.Content)
            .ThenInclude(section => (section as EmbedBlockLink)!.EmbedBlock)
            .Include(releaseVersion => releaseVersion.DataBlockVersions)
            .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlockParent)
            .ThenInclude(dataBlockParent => dataBlockParent.LatestDraftVersion)
            .Include(releaseVersion => releaseVersion.DataBlockVersions)
            .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlockParent)
            .ThenInclude(dataBlockParent => dataBlockParent.LatestPublishedVersion)
            .Include(releaseVersion => releaseVersion.DataBlockVersions)
            .Include(releaseVersion => releaseVersion.Updates)
            .Include(releaseVersion => releaseVersion.KeyStatistics)
            .ThenInclude(keyStat => (keyStat as KeyStatisticDataBlock)!.DataBlock)
            .Include(releaseVersion => releaseVersion.FeaturedTables)
            .First(rv => rv.Id == amendmentId);
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

        var htmlBlock2Body = @"
                    <p>
                        Content block 2
                        <comment-start name=""comment-1""></comment-start>
                            Content 1
                        <comment-end name=""comment-1""></comment-end>

                        Content 2
                    </p>".TrimIndent();

        var expectedHtmlBlock2Body = @"
                    <p>
                        Content block 2
                        
                            Content 1
                        

                        Content 2
                    </p>".TrimIndent();

        var htmlBlock3Body = @"
                    <p>
                        Content block 3
                        <comment-start name=""comment-1""></comment-start>
                            Content 1
                        <comment-end name=""comment-1""></comment-end>
                    </p>".TrimIndent();

        var expectedHtmlBlock3Body = @"
                    <p>
                        Content block 3
                        
                            Content 1
                        
                    </p>".TrimIndent();

        ReleaseVersion originalReleaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture
                .DefaultRelease()
                .WithPublication(_fixture
                    .DefaultPublication()))
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
                .GenerateList());

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(originalReleaseVersion);
            contentDbContext.Users.Add(new User { Id = _userId });
            await contentDbContext.SaveChangesAsync();
        }

        Guid? amendmentId;
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var releaseAmendmentService = BuildService(
                contentDbContext,
                statisticsDbContext: InMemoryStatisticsDbContext());

            // Method under test
            var result = await releaseAmendmentService.CreateReleaseAmendment(originalReleaseVersion.Id);
            var amendment = result.AssertRight();

            Assert.NotEqual(originalReleaseVersion.Id, amendment.Id);
            amendmentId = amendment.Id;
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var amendment = RetrieveAmendment(contentDbContext, amendmentId.Value);

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
        ReleaseVersion originalReleaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture
                .DefaultRelease()
                .WithPublication(_fixture
                    .DefaultPublication()))
            .WithCreated(createdById: _userId)
            .WithContent(_fixture
                .DefaultContentSection()
                .WithContentBlocks(_fixture
                    .DefaultHtmlBlock()
                    .WithBody(null!)
                    .GenerateList(1))
                .GenerateList(1));

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(originalReleaseVersion);
            contentDbContext.Users.Add(new User { Id = _userId });
            await contentDbContext.SaveChangesAsync();
        }

        Guid? amendmentId;
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var releaseAmendmentService = BuildService(
                contentDbContext,
                statisticsDbContext: InMemoryStatisticsDbContext());

            // Method under test
            var result = await releaseAmendmentService.CreateReleaseAmendment(originalReleaseVersion.Id);

            var amendment = result.AssertRight();

            Assert.NotEqual(originalReleaseVersion.Id, amendment.Id);
            amendmentId = amendment.Id;
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var amendment = RetrieveAmendment(contentDbContext, amendmentId.Value);
            Assert.NotNull(amendment);
        }
    }

    [Fact]
    public async Task CreatesRelatedDashboardsSectionIfNotOnOriginal()
    {
        ReleaseVersion originalReleaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture
                .DefaultRelease()
                .WithPublication(_fixture
                    .DefaultPublication()))
            .WithCreated(createdById: _userId);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(originalReleaseVersion);
            contentDbContext.Users.Add(new User { Id = _userId });
            await contentDbContext.SaveChangesAsync();
        }

        Guid? amendmentId;
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var releaseAmendmentService = BuildService(
                contentDbContext,
                statisticsDbContext: InMemoryStatisticsDbContext());

            // Method under test
            var result = await releaseAmendmentService.CreateReleaseAmendment(originalReleaseVersion.Id);
            var amendment = result.AssertRight();

            Assert.NotEqual(originalReleaseVersion.Id, amendment.Id);
            amendmentId = amendment.Id;
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var amendment = RetrieveAmendment(contentDbContext, amendmentId.Value);

            var relatedDashboardsSection = Assert.Single(amendment.Content);
            Assert.Equal(ContentSectionType.RelatedDashboards, relatedDashboardsSection.Type);
        }
    }

    [Fact]
    public async Task CopyFootnotes()
    {
        ReleaseVersion originalReleaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture
                .DefaultRelease()
                .WithPublication(_fixture
                    .DefaultPublication()))
            .WithCreated(createdById: _userId)
            .WithContent(_fixture
                .DefaultContentSection()
                .WithContentBlocks(_fixture
                    .DefaultHtmlBlock()
                    .WithBody(null!)
                    .GenerateList(1))
                .GenerateList(1));

        Data.Model.ReleaseVersion originalStatsReleaseVersion = _fixture
            .DefaultStatsReleaseVersion()
            .WithId(originalReleaseVersion.Id);

        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(originalStatsReleaseVersion)
            .WithSubject(_fixture
                .DefaultSubject()
                .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(1))
                .WithIndicatorGroups(_fixture.DefaultIndicatorGroup()
                    .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                    .Generate(1)));

        var releaseFootnotes = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersion(originalStatsReleaseVersion)
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
            contentDbContext.ReleaseVersions.AddRange(originalReleaseVersion);
            contentDbContext.Users.AddRange(new User { Id = _userId });

            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseVersion.AddRange(originalStatsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(releaseSubject);
            statisticsDbContext.ReleaseFootnote.AddRange(releaseFootnotes);

            await statisticsDbContext.SaveChangesAsync();
        }

        Guid? amendmentId;
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var releaseAmendmentService = BuildService(
                contentDbContext,
                statisticsDbContext);

            // Method under test
            var result = await releaseAmendmentService.CreateReleaseAmendment(originalStatsReleaseVersion.Id);
            var viewModel = result.AssertRight();

            Assert.NotEqual(Guid.Empty, viewModel.Id);
            Assert.NotEqual(originalStatsReleaseVersion.Id, viewModel.Id);
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
                .Where(f => f.Releases.FirstOrDefault(r => r.ReleaseVersionId == amendmentId) != null)
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

    private DataBlock GetMatchingDataBlock(List<DataBlockVersion> amendmentDataBlockVersions,
        DataBlockParent dataBlockToFind)
    {
        return amendmentDataBlockVersions
            .Where(dataBlockVersion => dataBlockVersion.Name == dataBlockToFind.LatestDraftVersion!.Name)
            .Select(dataBlockParent => dataBlockParent.ContentBlock)
            .Single();
    }

    private void AssertAmendedLinkCorrect(Link amendedLink, Link originalLink)
    {
        amendedLink.AssertDeepEqualTo(
            originalLink,
            ignoreProperties: [l => l.Id]);
    }

    private void AssertAmendedUpdateCorrect(Update amendedUpdate, Update originalUpdate, ReleaseVersion amendment)
    {
        amendedUpdate.AssertDeepEqualTo(
            originalUpdate,
            ignoreProperties:
            [
                u => u.Id,
                u => u.ReleaseVersion,
                u => u.ReleaseVersionId,
                u => u.Created,
                u => u.CreatedById
            ]);

        Assert.Equal(amendment, amendedUpdate.ReleaseVersion);
        Assert.Equal(amendment.Id, amendedUpdate.ReleaseVersionId);

        Assert.NotEqual(originalUpdate.Created, amendedUpdate.Created);
        amendedUpdate.Created.AssertUtcNow();

        Assert.Equal(_userId, amendedUpdate.CreatedById);
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private void AssertAmendedContentSectionCorrect(
        ReleaseVersion amendment,
        ContentSection amendedSection,
        ContentSection originalSection)
    {
        Assert.Equal(amendment, amendedSection.ReleaseVersion);
        Assert.Equal(amendment.Id, amendedSection.ReleaseVersionId);
        Assert.True(amendedSection.Id != Guid.Empty);
        Assert.NotEqual(originalSection.Id, amendedSection.Id);

        Assert.NotEqual(originalSection.Id, amendedSection.Id);
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

    [UsedImplicitly] 
    private static void AssertAmendedReleaseRoleCorrect(
        UserReleaseRole originalReleaseRole,
        UserReleaseRole amendedReleaseRole,
        ReleaseVersion amendment)
    {
        Assert.NotEqual(originalReleaseRole.Id, amendedReleaseRole.Id);
        Assert.Equal(amendment, amendedReleaseRole.ReleaseVersion);
        Assert.Equal(amendment.Id, amendedReleaseRole.ReleaseVersionId);
        Assert.Equal(originalReleaseRole.UserId, amendedReleaseRole.UserId);
        Assert.Equal(originalReleaseRole.Role, amendedReleaseRole.Role);
        amendedReleaseRole.Created.AssertUtcNow();
        Assert.Equal(originalReleaseRole.CreatedById, amendedReleaseRole.CreatedById);
    }

    [UsedImplicitly] 
    private static void AssertAmendedReleaseFileCorrect(
        ReleaseFile originalFile,
        ReleaseFile amendmentDataFile,
        ReleaseVersion amendment)
    {
        // Assert it's a new link table entry between the Release amendment and the data file reference
        Assert.NotEqual(originalFile.Id, amendmentDataFile.Id);
        Assert.Equal(amendment, amendmentDataFile.ReleaseVersion);
        Assert.Equal(amendment.Id, amendmentDataFile.ReleaseVersionId);
        Assert.Equal(originalFile.Name, amendmentDataFile.Name);
        Assert.Equal(originalFile.Order, amendmentDataFile.Order);
        Assert.Equal(originalFile.Published!.Value, amendmentDataFile.Published!.Value, TimeSpan.FromMinutes(1));
        originalFile.FilterSequence.AssertDeepEqualTo(amendmentDataFile.FilterSequence);
        originalFile.IndicatorSequence.AssertDeepEqualTo(amendmentDataFile.IndicatorSequence);

        Assert.Equal(originalFile.PublicApiDataSetId, amendmentDataFile.PublicApiDataSetId);
        Assert.Equal(originalFile.PublicApiDataSetVersion, amendmentDataFile.PublicApiDataSetVersion);

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
