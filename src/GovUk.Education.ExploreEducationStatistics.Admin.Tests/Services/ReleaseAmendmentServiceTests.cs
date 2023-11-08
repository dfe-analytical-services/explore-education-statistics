#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseAmendmentServiceTests
    {
        private readonly Guid _userId = Guid.NewGuid();

        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task CreateReleaseAmendment()
        {
            var releaseId = Guid.NewGuid();
            var publicationId = Guid.NewGuid();
            var publishedDate = DateTime.UtcNow.AddDays(-1);
            var createdDate = DateTime.UtcNow.AddDays(-2);
            var previousVersionReleaseId = Guid.NewGuid();
            var createdById = Guid.NewGuid();
            var createdBy = new User
            {
                Id = createdById
            };
            var latestInternalReleaseNote = "Release note";
            var releaseApprovalStatus = ReleaseApprovalStatus.Approved;
            var publishScheduled = DateTime.Now.AddDays(1);
            var nextReleaseDate = new PartialDate {Day = "1", Month = "1", Year = "2040"};
            var releaseName = "2035";
            var timePeriodCoverage = TimeIdentifier.March;

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

            var embedBlockLink = new EmbedBlockLink
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
                ReleaseId = releaseId
            };

            var originalRelease = new Release
            {
                Id = releaseId,
                Type = ReleaseType.OfficialStatistics,
                PublishScheduled = publishScheduled,
                NextReleaseDate = nextReleaseDate,
                ReleaseName = releaseName,
                TimePeriodCoverage = timePeriodCoverage,
                PublicationId = publicationId,
                Published = publishedDate,
                ApprovalStatus = releaseApprovalStatus,
                NotifiedOn = DateTime.UtcNow,
                NotifySubscribers = true,
                UpdatePublishedDate = true,
                Version = 2,
                PreviousVersionId = previousVersionReleaseId,
                Created = createdDate,
                CreatedBy = createdBy,
                CreatedById = createdById,
                PreReleaseAccessList = "Some Pre-release details",
                ReleaseStatuses = new List<ReleaseStatus>
                {
                    new()
                    {
                        ReleaseId = releaseId,
                        InternalReleaseNote = latestInternalReleaseNote,
                        Created = DateTime.UtcNow
                    }
                },
                RelatedInformation = new List<Link>
                {
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
                    }
                },
                Updates = new List<Update>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        On = DateTime.UtcNow.AddDays(-4),
                        Reason = "Reason 1",
                        ReleaseId = releaseId,
                        Created = createdDate.AddDays(1),
                        CreatedById = Guid.NewGuid(),
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        On = DateTime.UtcNow.AddDays(-5),
                        Reason = "Reason 2",
                        ReleaseId = releaseId,
                        Created = createdDate.AddDays(2),
                        CreatedById = Guid.NewGuid(),
                    }
                },
                KeyStatistics = new List<KeyStatistic>
                {
                    new KeyStatisticText { Title = "key stat text", },
                    new KeyStatisticDataBlock
                    {
                        DataBlock = dataBlock3Parent.LatestPublishedVersion!.ContentBlock,
                    },
                },
                Content = ListOf(
                    new ContentSection
                    {
                        Id = Guid.NewGuid(),
                        Caption = "Template caption index 0",
                        Heading = "Template heading index 0",
                        Type = ContentSectionType.Generic,
                        Order = 1,
                        Content = new List<ContentBlock>
                        {
                            new HtmlBlock
                            {
                                Id = Guid.NewGuid(),
                                Body = @"<div></div>",
                                Order = 1,
                                Comments = new List<Comment>
                                {
                                    new()
                                    {
                                        Id = Guid.NewGuid(),
                                        Content = "Comment 1 Text"
                                    },
                                    new Comment
                                    {
                                        Id = Guid.NewGuid(),
                                        Content = "Comment 2 Text"
                                    }
                                }
                            },
                            dataBlock1Parent.LatestPublishedVersion!.ContentBlock,
                            embedBlockLink,
                        }
                    },
                    new ContentSection
                    {
                        Id = Guid.NewGuid(),
                        Caption = "Template caption index 1",
                        Heading = "Template heading index 1",
                        Type = ContentSectionType.Generic,
                        Order = 2,
                        Content = new List<ContentBlock>
                        {
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
                            }
                        }
                    },
                    new ContentSection
                    {
                        Id = Guid.NewGuid(),
                        Caption = "Template caption index 2",
                        Heading = "Template heading index 2",
                        Type = ContentSectionType.Headlines,
                        Order = 1,
                        Content = new List<ContentBlock>
                        {
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
                            }
                        }
                    },
                    new ContentSection
                    {
                        Id = Guid.NewGuid(),
                        Type = ContentSectionType.RelatedDashboards,
                        Order = 0,
                        Content = new List<ContentBlock>
                        {
                            new HtmlBlock
                            {
                                Id = Guid.NewGuid(),
                                Body = "RelatedDashboards text",
                                Comments = new List<Comment>
                                {
                                    new()
                                    {
                                        Id = Guid.NewGuid(),
                                        Content = "RelatedDashboards comment"
                                    }
                                }
                            }
                        }
                    }),
                DataBlockVersions = dataBlockParents
                    .Select(dataBlockParent => dataBlockParent.LatestPublishedVersion!)
                    .ToList()
            };

            var approverReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.Approver,
                Release = originalRelease,
                ReleaseId = releaseId
            };

            var contributorReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.Contributor,
                Release = originalRelease,
                ReleaseId = releaseId
            };

            var deletedReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.Lead,
                Release = originalRelease,
                ReleaseId = releaseId,
                Deleted = DateTime.UtcNow,
                DeletedById = Guid.NewGuid(),
            };

            var prereleaseReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.PrereleaseViewer,
                Release = originalRelease,
                ReleaseId = releaseId,
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
                    ReleaseId = releaseId,
                    File = dataFile1,
                    FileId = dataFile1.Id
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Release = originalRelease,
                    ReleaseId = releaseId,
                    File = dataFile2,
                    FileId = dataFile2.Id
                }
            };

            var subject1 = new Subject
            {
                Id = Guid.NewGuid()
            };

            var subject2 = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Add(new Publication
                {
                    Id = publicationId,
                    Contact = new Contact(),
                    Releases = new List<Release>
                    {
                        originalRelease
                    }
                });
                contentDbContext.Add(createdBy);
                contentDbContext.Add(new User
                {
                    Id = _userId
                });
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                contentDbContext.ReleaseFiles.AddRange(releaseFiles);
                // contentDbContext.DataBlockParents.AddRange(dataBlock1Parent, dataBlock2Parent, dataBlock3Parent);
                // contentDbContext.ContentBlocks.AddRange(embedBlockLink);
                contentDbContext.SaveChanges();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Release.Add(new Data.Model.Release
                {
                    Id = originalRelease.Id,
                    PublicationId = originalRelease.PublicationId,
                });

                statisticsDbContext.Subject.AddRange(subject1, subject2);

                statisticsDbContext.ReleaseSubject.AddRange(
                    new ReleaseSubject
                    {
                        ReleaseId = releaseId,
                        SubjectId = subject1.Id
                    },
                    new ReleaseSubject
                    {
                        ReleaseId = releaseId,
                        SubjectId = subject2.Id
                    }
                );

                statisticsDbContext.SaveChanges();
            }

            var footnoteService = new Mock<IFootnoteService>(Strict);

            footnoteService
                .Setup(service => service.CopyFootnotes(releaseId, It.IsAny<Guid>()))
                .ReturnsAsync(new Either<ActionResult, List<Footnote>>(new List<Footnote>()));

            var releaseService = new Mock<IReleaseService>(Strict);

            var amendmentViewModel = new ReleaseViewModel
            {
                Slug = "amended-release"
            };

            var capturedAmendmentId = Guid.Empty;
            var amendmentIdMatch = new CaptureMatch<Guid>(param => capturedAmendmentId = param);
            releaseService
                .Setup(service => service.GetRelease(Capture.With(amendmentIdMatch)))
                .ReturnsAsync(amendmentViewModel);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var releaseAmendmentService = BuildService(
                    contentDbContext,
                    statisticsDbContext,
                    releaseService: releaseService.Object,
                    footnoteService: footnoteService.Object
                );

                // Method under test
                var result = await releaseAmendmentService.CreateReleaseAmendment(releaseId);

                result.AssertRight(amendmentViewModel);

                VerifyAllMocks(footnoteService);

                Assert.NotEqual(originalRelease.Id, capturedAmendmentId);
                Assert.NotEqual(Guid.Empty, capturedAmendmentId);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var amendment = contentDbContext
                    .Releases
                    .Include(release => release.PreviousVersion)
                    .Include(release => release.CreatedBy)
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
                    .First(r => r.Id == capturedAmendmentId);

                var amendmentContentBlocks = contentDbContext
                    .ContentBlocks
                    .Where(block => block.ReleaseId == amendment.Id)
                    .ToList();

                // check fields that should be set to new values for an amendment, rather than copied from its original
                // Release
                Assert.Null(amendment.NotifiedOn);
                Assert.False(amendment.NotifySubscribers);
                Assert.False(amendment.UpdatePublishedDate);
                Assert.Null(amendment.PublishScheduled);
                Assert.Null(amendment.Published);
                Assert.Equal(originalRelease.Version + 1, amendment.Version);
                Assert.Equal(ReleaseApprovalStatus.Draft, amendment.ApprovalStatus);
                Assert.Equal(originalRelease.Id, amendment.PreviousVersion?.Id);
                Assert.Equal(originalRelease.Id, amendment.PreviousVersionId);
                Assert.Equal(_userId, amendment.CreatedBy.Id);
                Assert.Equal(_userId, amendment.CreatedById);
                amendment.Created.AssertUtcNow(withinMillis: 1500);

                Assert.Equal(ReleaseType.OfficialStatistics, amendment.Type);
                Assert.Equal(nextReleaseDate, amendment.NextReleaseDate);
                Assert.Equal(releaseName, amendment.ReleaseName);
                Assert.Equal(timePeriodCoverage, amendment.TimePeriodCoverage);
                Assert.Equal(publicationId, amendment.PublicationId);

                Assert.Equal(originalRelease.RelatedInformation.Count, amendment.RelatedInformation.Count);
                amendment.RelatedInformation.ForEach(amended =>
                {
                    var index = amendment.RelatedInformation.IndexOf(amended);
                    var previous = originalRelease.RelatedInformation[index];
                    AssertAmendedLinkCorrect(amended, previous);
                });

                Assert.Equal(originalRelease.Updates.Count, amendment.Updates.Count);
                amendment.Updates.ForEach(amended =>
                {
                    var index = amendment.Updates.IndexOf(amended);
                    var previous = originalRelease.Updates[index];
                    AssertAmendedUpdateCorrect(amended, previous, amendment);
                });

                Assert.Equal(originalRelease.Content.Count, amendment.Content.Count);
                amendment.Content.ForEach(amended =>
                {
                    var index = amendment.Content.IndexOf(amended);
                    var previous = originalRelease.Content[index];
                    AssertAmendedContentSectionCorrect(amendment, amended, previous);
                });

                Assert.Equal(2, amendment.KeyStatistics.Count);

                var amendmentKeyStatText = Assert.IsType<KeyStatisticText>(amendment
                    .KeyStatistics.Find(ks => ks.GetType() == typeof(KeyStatisticText)));
                Assert.Equal((
                    originalRelease.KeyStatistics[0] as KeyStatisticText)!.Title, amendmentKeyStatText.Title);
                Assert.NotEqual(originalRelease.KeyStatistics[0].Id, amendmentKeyStatText.Id);
                Assert.Equal(8, amendmentContentBlocks.Count);

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

                var amendmentKeyStatDataBlock = Assert.IsType<KeyStatisticDataBlock>(amendment
                    .KeyStatistics.Find(ks => ks.GetType() == typeof(KeyStatisticDataBlock)));
                Assert.Equal(dataBlock3Parent.LatestDraftVersion!.Name, amendmentKeyStatDataBlock.DataBlock.Name);
                Assert.NotEqual(originalRelease.KeyStatistics[1].Id, amendmentKeyStatDataBlock.Id);
                Assert.NotEqual(dataBlock3Parent.LatestDraftVersion.Id, amendmentKeyStatDataBlock.DataBlockId);
                Assert.Equal(amendmentContentBlock3.Id, amendmentKeyStatDataBlock.DataBlockId);

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
                Assert.NotEqual(dataBlock3Parent.LatestDraftVersion.Id, amendmentContentBlock3.Id);

                var amendmentEmbedBlocks = amendmentContentBlocks.OfType<EmbedBlockLink>().ToList();
                var amendmentContentBlock4 = Assert.Single(amendmentEmbedBlocks);

                Assert.NotEqual(embedBlockLink.Id, amendmentContentBlock4.Id);
                Assert.NotEqual(embedBlockLink.EmbedBlockId, amendmentContentBlock4.EmbedBlockId);
                Assert.Equal(embedBlockLink.EmbedBlock.Title, amendmentContentBlock4.EmbedBlock.Title);
                Assert.Equal(embedBlockLink.EmbedBlock.Url, amendmentContentBlock4.EmbedBlock.Url);

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

                Assert.Equal(releaseFiles.Count, amendmentDataFiles.Count);

                var amendmentDataFile = amendmentDataFiles[0];
                var originalFile = releaseFiles.First(f =>
                    f.File.Filename == amendmentDataFile.File.Filename);
                AssertAmendedReleaseFileCorrect(originalFile, amendmentDataFile, amendment);

                var amendmentDataFile2 = amendmentDataFiles[1];
                var originalFile2 = releaseFiles.First(f =>
                    f.File.Filename == amendmentDataFile2.File.Filename);
                AssertAmendedReleaseFileCorrect(originalFile2, amendmentDataFile2, amendment);

                Assert.Equal(originalRelease.PreReleaseAccessList, amendment.PreReleaseAccessList);

                Assert.True(amendment.Amendment);
            }

            using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var releaseSubjectLinks = statisticsDbContext
                    .ReleaseSubject
                    .AsQueryable()
                    .Where(r => r.ReleaseId == capturedAmendmentId)
                    .ToList();

                Assert.Equal(2, releaseSubjectLinks.Count);
                Assert.Contains(subject1.Id, releaseSubjectLinks.Select(r => r.SubjectId));
                Assert.Contains(subject2.Id, releaseSubjectLinks.Select(r => r.SubjectId));
            }
        }

        private static DataBlock GetMatchingDataBlock(List<DataBlockVersion> amendmentDataBlockVersions, DataBlockParent dataBlockToFind)
        {
            return amendmentDataBlockVersions
                .Where(dataBlockVersion => dataBlockVersion.Name == dataBlockToFind.LatestDraftVersion!.Name)
                .Select(dataBlockParent => dataBlockParent.ContentBlock)
                .Single();
        }

        private static void AssertAmendedLinkCorrect(Link amended, Link previous)
        {
            Assert.True(amended.Id != Guid.Empty);
            Assert.NotEqual(previous.Id, amended.Id);
            Assert.Equal(previous.Description, amended.Description);
            Assert.Equal(previous.Url, amended.Url);
        }

        private static void AssertAmendedUpdateCorrect(Update amendedUpdate, Update previousUpdate, Release amendment)
        {
            Assert.True(amendedUpdate.Id != Guid.Empty);
            Assert.NotEqual(previousUpdate.Id, amendedUpdate.Id);
            Assert.Equal(previousUpdate.On, amendedUpdate.On);
            Assert.Equal(previousUpdate.Reason, amendedUpdate.Reason);
            Assert.Equal(previousUpdate.Created, amendedUpdate.Created);
            Assert.Equal(previousUpdate.CreatedById, amendedUpdate.CreatedById);
            Assert.Equal(amendment.Id, amendedUpdate.ReleaseId);
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void AssertAmendedContentSectionCorrect(
            Release amendment,
            ContentSection amendedSection,
            ContentSection previousSection)
        {
            Assert.Equal(amendment, amendedSection.Release);
            Assert.Equal(amendment.Id, amendedSection.ReleaseId);
            Assert.True(amendedSection.Id != Guid.Empty);
            Assert.NotEqual(previousSection.Id, amendedSection.Id);

            Assert.NotEqual(previousSection.Id, amendedSection.Id);
            Assert.Equal(previousSection.Caption, amendedSection.Caption);
            Assert.Equal(previousSection.Heading, amendedSection.Heading);
            Assert.Equal(previousSection.Order, amendedSection.Order);
            Assert.Equal(previousSection.Type, amendedSection.Type);
            Assert.Equal(previousSection.Content.Count, amendedSection.Content.Count);

            amendedSection.Content.ForEach(amendedBlock =>
            {
                var previousBlock = previousSection.Content.Find(b => b.Order == amendedBlock.Order);
                AssertAmendedContentBlockCorrect(previousBlock, amendedBlock, amendedSection);
            });
        }

        private static void AssertAmendedContentBlockCorrect(ContentBlock? previousBlock, ContentBlock amendedBlock,
            ContentSection amendedSection)
        {
            Assert.NotEqual(previousBlock?.Id, amendedBlock.Id);
            Assert.Equal(previousBlock?.Order, amendedBlock.Order);
            Assert.Equal(amendedSection, amendedBlock.ContentSection);
            Assert.Equal(amendedSection.Id, amendedBlock.ContentSectionId);
            Assert.NotEmpty(previousBlock!.Comments);
            Assert.Empty(amendedBlock.Comments);
        }

        private static void AssertAmendedReleaseRoleCorrect(UserReleaseRole previous, UserReleaseRole amended,
            Release amendment)
        {
            Assert.NotEqual(previous.Id, amended.Id);
            Assert.Equal(amendment, amended.Release);
            Assert.Equal(amendment.Id, amended.ReleaseId);
            Assert.Equal(previous.UserId, amended.UserId);
            Assert.Equal(previous.Role, amended.Role);
            Assert.Equal(previous.Created, amended.Created);
            Assert.Equal(previous.CreatedById, amended.CreatedById);
            Assert.Equal(previous.Deleted, amended.Deleted);
            Assert.Equal(previous.DeletedById, amended.DeletedById);
        }

        private static void AssertAmendedReleaseFileCorrect(ReleaseFile originalFile, ReleaseFile amendmentDataFile,
            Release amendment)
        {
            // assert it's a new link table entry between the Release amendment and the data file reference
            Assert.NotEqual(originalFile.Id, amendmentDataFile.Id);
            Assert.Equal(amendment, amendmentDataFile.Release);
            Assert.Equal(amendment.Id, amendmentDataFile.ReleaseId);
            Assert.Equal(originalFile.Name, amendmentDataFile.Name);
            Assert.Equal(originalFile.Order, amendmentDataFile.Order);

            // and assert that the file referenced is the SAME file reference as linked from the original Release's
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
            IReleaseService? releaseService = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IUserService? userService = null,
            IFootnoteService? footnoteService = null)
        {
            return new ReleaseAmendmentService(
                contentDbContext,
                releaseService ?? Mock.Of<IReleaseService>(Strict),
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService ?? UserServiceMock().Object,
                footnoteService ?? Mock.Of<IFootnoteService>(Strict),
                statisticsDbContext);
        }
    }
}
