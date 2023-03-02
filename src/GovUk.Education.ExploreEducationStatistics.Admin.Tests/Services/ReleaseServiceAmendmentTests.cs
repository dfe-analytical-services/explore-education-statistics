#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServiceAmendmentTests
    {
        private readonly Guid _userId = Guid.NewGuid();

        [Fact]
        public void CreateReleaseAmendment()
        {
            var releaseId = Guid.NewGuid();
            var publicationId = Guid.NewGuid();
            var publishedDate = DateTime.UtcNow.AddDays(-1);
            var createdDate = DateTime.UtcNow.AddDays(-2);
            var previousVersionReleaseId = Guid.NewGuid();
            var version = 2;
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

            var dataBlock1 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Data Block 1",
                Order = 2,
                Comments = new List<Comment>
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
                }
            };

            var dataBlock2 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Data Block 2"
            };

            var dataBlock3 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Data block to be used by key stat",
            };

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
            };

            var release = new Release
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
                Version = version,
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
                        DataBlock = dataBlock3,
                    },
                },
                Content = new List<ReleaseContentSection>
                {
                    new()
                    {
                        ReleaseId = Guid.NewGuid(),
                        ContentSection = new ContentSection
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
                                dataBlock1,
                                embedBlockLink,
                            }
                        }
                    },

                    new()
                    {
                        ReleaseId = Guid.NewGuid(),
                        ContentSection = new ContentSection
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
                        }
                    },

                    new()
                    {
                        ReleaseId = Guid.NewGuid(),
                        ContentSection = new ContentSection
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
                        }
                    },
                    new ()
                    {
                        ReleaseId = Guid.NewGuid(),
                        ContentSection = new ContentSection
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
                        }
                    }
                },

                ContentBlocks = new List<ReleaseContentBlock>
                {
                    new()
                    {
                        ReleaseId = releaseId,
                        ContentBlock = dataBlock1,
                        ContentBlockId = dataBlock1.Id,
                    },
                    new()
                    {
                        ReleaseId = releaseId,
                        ContentBlock = dataBlock2,
                        ContentBlockId = dataBlock2.Id,
                    },
                    new()
                    {
                        ReleaseId = releaseId,
                        ContentBlock = dataBlock3,
                        ContentBlockId = dataBlock3.Id,
                    },
                    new()
                    {
                        ReleaseId = releaseId,
                        ContentBlock = embedBlockLink,
                        ContentBlockId = embedBlockLink.Id,
                    },
                },
            };

            var approverReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.Approver,
                Release = release,
                ReleaseId = releaseId
            };

            var contributorReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.Contributor,
                Release = release,
                ReleaseId = releaseId
            };

            var deletedReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.Lead,
                Release = release,
                ReleaseId = releaseId,
                Deleted = DateTime.UtcNow,
                DeletedById = Guid.NewGuid(),
            };

            var prereleaseReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.PrereleaseViewer,
                Release = release,
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
                    Release = release,
                    ReleaseId = releaseId,
                    File = dataFile1,
                    FileId = dataFile1.Id
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Release = release,
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

            using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Add(new Publication
                {
                    Id = publicationId,
                    Contact = new Contact(),
                    Releases = new List<Release>
                    {
                        release
                    }
                });

                contentDbContext.Add(createdBy);
                contentDbContext.Add(new User
                {
                    Id = _userId
                });
                contentDbContext.AddRange(userReleaseRoles);
                contentDbContext.AddRange(releaseFiles);

                contentDbContext.SaveChanges();
            }

            using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Release.Add(new Data.Model.Release
                {
                    Id = release.Id,
                    PublicationId = release.PublicationId,
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

            Guid newReleaseId;

            var footnoteService = new Mock<IFootnoteService>(Strict);

            footnoteService
                .Setup(service => service.CopyFootnotes(releaseId, It.IsAny<Guid>()))
                .ReturnsAsync(new Either<ActionResult, List<Footnote>>(new List<Footnote>()));

            using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var releaseService = BuildReleaseService(
                    contentDbContext,
                    statisticsDbContext,
                    footnoteService: footnoteService.Object
                );

                // Method under test
                var amendmentViewModel = releaseService.CreateReleaseAmendment(releaseId).Result.Right;

                MockUtils.VerifyAllMocks(footnoteService);

                Assert.NotEqual(release.Id, amendmentViewModel.Id);
                Assert.NotEqual(Guid.Empty, amendmentViewModel.Id);
                newReleaseId = amendmentViewModel.Id;
            }

            using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var amendment = contentDbContext
                    .Releases
                    .Include(r => r.PreviousVersion)
                    .Include(r => r.CreatedBy)
                    .Include(r => r.Publication)
                    .Include(r => r.Content)
                    .ThenInclude(c => c.ContentSection)
                    .ThenInclude(c => c.Content)
                    .ThenInclude(c => c.Comments)
                    .Include(r => r.Content)
                    .ThenInclude(c => c.ContentSection)
                    .ThenInclude(c => c.Content)
                    .ThenInclude(c => (c as EmbedBlockLink)!.EmbedBlock)
                    .Include(r => r.Updates)
                    .Include(r => r.KeyStatistics)
                    .ThenInclude(ks => (ks as KeyStatisticDataBlock)!.DataBlock)
                    .Include(r => r.ContentBlocks)
                    .ThenInclude(r => r.ContentBlock)
                    .First(r => r.Id == newReleaseId);

                // check fields that should be set to new values for an amendment, rather than copied from its original
                // Release
                Assert.Equal(newReleaseId, amendment.Id);
                Assert.Null(amendment.NotifiedOn);
                Assert.False(amendment.NotifySubscribers);
                Assert.False(amendment.UpdatePublishedDate);
                Assert.Null(amendment.PublishScheduled);
                Assert.Null(amendment.Published);
                Assert.Equal(release.Version + 1, amendment.Version);
                Assert.Equal(ReleaseApprovalStatus.Draft, amendment.ApprovalStatus);
                Assert.Equal(release.Id, amendment.PreviousVersion?.Id);
                Assert.Equal(release.Id, amendment.PreviousVersionId);
                Assert.Equal(_userId, amendment.CreatedBy.Id);
                Assert.Equal(_userId, amendment.CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(amendment.Created).Milliseconds, 0, 1500);

                Assert.Equal(ReleaseType.OfficialStatistics, amendment.Type);
                Assert.Equal(nextReleaseDate, amendment.NextReleaseDate);
                Assert.Equal(releaseName, amendment.ReleaseName);
                Assert.Equal(timePeriodCoverage, amendment.TimePeriodCoverage);
                Assert.Equal(publicationId, amendment.PublicationId);

                Assert.Equal(release.RelatedInformation.Count, amendment.RelatedInformation.Count);
                amendment.RelatedInformation.ForEach(amended =>
                {
                    var index = amendment.RelatedInformation.IndexOf(amended);
                    var previous = release.RelatedInformation[index];
                    AssertAmendedLinkCorrect(amended, previous);
                });

                Assert.Equal(release.Updates.Count, amendment.Updates.Count);
                amendment.Updates.ForEach(amended =>
                {
                    var index = amendment.Updates.IndexOf(amended);
                    var previous = release.Updates[index];
                    AssertAmendedUpdateCorrect(amended, previous, amendment);
                });

                Assert.Equal(release.Content.Count, amendment.Content.Count);
                amendment.Content.ForEach(amended =>
                {
                    var index = amendment.Content.IndexOf(amended);
                    var previous = release.Content[index];
                    AssertAmendedContentSectionCorrect(amendment, amended, previous);
                });

                Assert.Equal(2, amendment.KeyStatistics.Count);

                var amendmentKeyStatText = Assert.IsType<KeyStatisticText>(amendment
                    .KeyStatistics.Find(ks => ks.GetType() == typeof(KeyStatisticText)));
                Assert.Equal((
                    release.KeyStatistics[0] as KeyStatisticText)!.Title, amendmentKeyStatText.Title);
                Assert.NotEqual(release.KeyStatistics[0].Id, amendmentKeyStatText.Id);

                var amendmentKeyStatDataBlock = Assert.IsType<KeyStatisticDataBlock>(amendment
                    .KeyStatistics.Find(ks => ks.GetType() == typeof(KeyStatisticDataBlock)));
                Assert.Equal(dataBlock3.Name, amendmentKeyStatDataBlock.DataBlock.Name);
                Assert.NotEqual(release.KeyStatistics[1].Id, amendmentKeyStatDataBlock.Id);
                Assert.NotEqual(dataBlock3.Id, amendmentKeyStatDataBlock.DataBlockId);
                var amendmentDataBlock3 = Assert.IsType<DataBlock>(amendment.ContentBlocks[2].ContentBlock);
                Assert.Equal(amendmentDataBlock3.Id, amendmentKeyStatDataBlock.DataBlockId);

                Assert.Equal(release.ContentBlocks.Count, amendment.ContentBlocks.Count);
                var amendmentContentBlock1 = Assert.IsType<DataBlock>(amendment.ContentBlocks[0].ContentBlock);
                var amendmentContentBlock2 = Assert.IsType<DataBlock>(amendment.ContentBlocks[1].ContentBlock);
                var amendmentContentBlock3 = Assert.IsType<DataBlock>(amendment.ContentBlocks[2].ContentBlock);
                var amendmentContentBlock4 = Assert.IsType<EmbedBlockLink>(amendment.ContentBlocks[3].ContentBlock);

                var amendmentContentBlock1InContent = amendment.Content[0].ContentSection.Content[0];

                // Check that the DataBlock that is included in this Release amendment's Content is successfully
                // identified as the exact same DataBlock that is attached to the Release amendment through the
                // additional "Release.ContentBlocks" relationship (which is used to determine which Data Blocks
                // belong to which Release when a Data Block has not yet been - or is removed from - the Release's
                // Content
                Assert.NotEqual(dataBlock1.Id, amendmentContentBlock1.Id);
                Assert.Equal(dataBlock1.Name, amendmentContentBlock1.Name);
                Assert.Equal(amendmentContentBlock1, amendmentContentBlock1InContent);

                // and check that the Data Block that is not yet included in any content is copied across OK still
                Assert.NotEqual(dataBlock2.Id, amendmentContentBlock2.Id);
                Assert.Equal(dataBlock2.Name, amendmentContentBlock2.Name);

                // and check DataBlock previously associated with key stat is copied correctly
                Assert.NotEqual(dataBlock3.Id, amendmentContentBlock3.Id);
                Assert.Equal(dataBlock3.Name, amendmentContentBlock3.Name);

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
                
                // Assert that the amendment does not have the PreReleaseAccessList copied over from the 
                // original Release.
                Assert.Null(amendment.PreReleaseAccessList);

                Assert.True(amendment.Amendment);
            }

            using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var releaseSubjectLinks = statisticsDbContext
                    .ReleaseSubject
                    .AsQueryable()
                    .Where(r => r.ReleaseId == newReleaseId)
                    .ToList();

                Assert.Equal(2, releaseSubjectLinks.Count);
                Assert.Contains(subject1.Id, releaseSubjectLinks.Select(r => r.SubjectId));
                Assert.Contains(subject2.Id, releaseSubjectLinks.Select(r => r.SubjectId));
            }
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
        private static void AssertAmendedContentSectionCorrect(Release amendment, ReleaseContentSection amended,
            ReleaseContentSection previous)
        {
            Assert.Equal(amendment, amended.Release);
            Assert.Equal(amendment.Id, amended.ReleaseId);
            Assert.True(amended.ContentSectionId != Guid.Empty);
            Assert.NotEqual(previous.ContentSectionId, amended.ContentSectionId);

            var previousSection = previous.ContentSection;
            var amendedSection = amended.ContentSection;

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
            var userService = MockUtils.AlwaysTrueUserService();

            userService
                .Setup(s => s.GetUserId())
                .Returns(_userId);

            return userService;
        }

        private ReleaseService BuildReleaseService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IUserService? userService = null,
            IReleaseRepository? repository = null,
            IReleaseCacheService? releaseCacheService = null,
            IReleaseFileRepository? releaseFileRepository = null,
            ISubjectRepository? subjectRepository = null,
            IReleaseDataFileService? releaseDataFileService = null,
            IReleaseFileService? releaseFileService = null,
            IDataImportService? dataImportService = null,
            IFootnoteService? footnoteService = null,
            IFootnoteRepository? footnoteRepository = null,
            IDataBlockService? dataBlockService = null,
            IReleaseSubjectRepository? releaseSubjectRepository = null,
            IGuidGenerator? guidGenerator = null)
        {
            return new ReleaseService(
                contentDbContext,
                AdminMapper(),
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService ?? UserServiceMock().Object,
                repository ?? Mock.Of<IReleaseRepository>(Strict),
                releaseCacheService ?? Mock.Of<IReleaseCacheService>(Strict),
                releaseFileRepository ?? Mock.Of<IReleaseFileRepository>(Strict),
                subjectRepository ?? Mock.Of<ISubjectRepository>(Strict),
                releaseDataFileService ?? Mock.Of<IReleaseDataFileService>(Strict),
                releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
                dataImportService ?? Mock.Of<IDataImportService>(Strict),
                footnoteService ?? Mock.Of<IFootnoteService>(Strict),
                footnoteRepository ?? Mock.Of<IFootnoteRepository>(Strict),
                statisticsDbContext,
                dataBlockService ?? Mock.Of<IDataBlockService>(Strict),
                releaseSubjectRepository ?? Mock.Of<IReleaseSubjectRepository>(Strict),
                guidGenerator ?? new SequentialGuidGenerator(),
                Mock.Of<IBlobCacheService>(Strict)
            );
        }
    }
}
