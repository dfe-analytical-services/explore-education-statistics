using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServiceAmendmentTests
    {
        private readonly Guid _userId = Guid.NewGuid();
        
       [Fact]
       public void CreateReleaseAmendmentAsync()
        {
            var (userService, _, publishingService, repository, subjectService, tableStorageService, fileStorageService, importStatusService, footnoteService) = Mocks();

            var releaseId = Guid.NewGuid();
            var releaseType = new ReleaseType
            {
                Id = Guid.NewGuid(),
                Title = "Official Statistics"
            };
            var publicationId = Guid.NewGuid();
            var publishedDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));
            var createdDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2));
            var originalReleaseId = Guid.NewGuid();
            var version = 2;
            var createdById = Guid.NewGuid();
            var createdBy = new User
            {
                Id = createdById
            };
            var internalReleaseNote = "Release note";
            var releaseStatus = ReleaseStatus.Approved;
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
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Name = "Comment 1",
                        CommentText = "Comment 1 Text"
                    },
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Name = "Comment 2",
                        CommentText = "Comment 2 Text"
                    }
                }
            };

            var dataBlock2 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Data Block 2"
            };
            
            var release = new Release
            {
                
                Id = releaseId,
                Type = releaseType,
                TypeId = releaseType.Id,
                PublishScheduled = publishScheduled,
                NextReleaseDate = nextReleaseDate,
                ReleaseName = releaseName,
                TimePeriodCoverage = timePeriodCoverage,
                PublicationId = publicationId,
                Published = publishedDate,
                Status = releaseStatus,
                Version = version,
                OriginalId = originalReleaseId,
                Created = createdDate,
                CreatedBy = createdBy,
                CreatedById = createdById,
                InternalReleaseNote = internalReleaseNote,
                RelatedInformation = new List<BasicLink>
                {
                    new BasicLink
                    {
                        Id = Guid.NewGuid(),
                        Description = "Link 1",
                        Url = "URL 1"
                    },
                    new BasicLink
                    {
                        Id = Guid.NewGuid(),
                        Description = "Link 2",
                        Url = "URL 2"
                    }
                },
                Updates = new List<Update>
                {
                    new Update
                    {
                        Id = Guid.NewGuid(),
                        On = DateTime.UtcNow.Subtract(TimeSpan.FromDays(4)),
                        Reason = "Reason 1",
                        ReleaseId = releaseId
                    },
                    new Update
                    {
                        Id = Guid.NewGuid(),
                        On = DateTime.UtcNow.Subtract(TimeSpan.FromDays(5)),
                        Reason = "Reason 2",
                        ReleaseId = releaseId
                    }
                },
                Content = new List<ReleaseContentSection>
                {
                    new ReleaseContentSection
                    {
                        ReleaseId = Guid.NewGuid(),
                        ContentSection = new ContentSection
                        {
                            Id = Guid.NewGuid(),
                            Caption = "Template caption index 0",
                            Heading = "Template heading index 0",
                            Type = ContentSectionType.Generic,
                            Order = 1,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = Guid.NewGuid(),
                                    Body = @"<div></div>",
                                    Order = 1,
                                    Comments = new List<Comment>
                                    {
                                        new Comment
                                        {
                                            Id = Guid.NewGuid(),
                                            Name = "Html Comment 1",
                                            CommentText = "Comment 1 Text"
                                        },
                                        new Comment
                                        {
                                            Id = Guid.NewGuid(),
                                            Name = "Html Comment 2",
                                            CommentText = "Comment 2 Text"
                                        }
                                    }
                                },
                                dataBlock1
                            }
                        }
                    },

                    new ReleaseContentSection
                    {
                        ReleaseId = Guid.NewGuid(),
                        ContentSection = new ContentSection
                        {
                            Id = Guid.NewGuid(),
                            Caption = "Template caption index 1",
                            Heading = "Template heading index 1",
                            Type = ContentSectionType.Generic,
                            Order = 2,
                            Content = new List<IContentBlock>
                            {
                                new InsetTextBlock
                                {
                                    Id = Guid.NewGuid(),
                                    Body = "Text",
                                    Heading = "Heading",
                                    Comments = new List<Comment>
                                    {
                                        new Comment
                                        {
                                            Id = Guid.NewGuid(),
                                            Name = "Inset Comment 1",
                                            CommentText = "Inset Comment 1 Text"
                                        }
                                    }
                                }
                            }
                        }
                    },

                    new ReleaseContentSection
                    {
                        ReleaseId = Guid.NewGuid(),
                        ContentSection = new ContentSection
                        {
                            Id = Guid.NewGuid(),
                            Caption = "Template caption index 2",
                            Heading = "Template heading index 2",
                            Type = ContentSectionType.Headlines,
                            Order = 1,
                            Content = new List<IContentBlock>
                            {
                                new InsetTextBlock
                                {
                                    Id = Guid.NewGuid(),
                                    Body = "Text",
                                    Heading = "Heading",
                                    Comments = new List<Comment> {
                                        new Comment
                                        {
                                            Id = Guid.NewGuid(),
                                            Name = "Inset Comment 1",
                                            CommentText = "Inset Comment 1 Text"
                                        }
                                    }
                                }
                            }
                        }
                    }
                },

                ContentBlocks = new List<ReleaseContentBlock>
                {
                    new ReleaseContentBlock
                    {
                        ReleaseId = releaseId,
                        ContentBlock = dataBlock1,
                        ContentBlockId = dataBlock1.Id,
                    },
                    new ReleaseContentBlock
                    {
                        ReleaseId = releaseId,
                        ContentBlock = dataBlock2,
                        ContentBlockId = dataBlock2.Id,
                    }
                }
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
            
            var userReleaseRoles = new List<UserReleaseRole>
            {
                approverReleaseRole,
                contributorReleaseRole
            };

            var dataFileReference1 = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "Filename 1",
                Release = release,
                ReleaseId = releaseId,
                SubjectId = Guid.NewGuid()
            };
            
            var dataFileReference2 = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "Filename 2",
                Release = release,
                ReleaseId = releaseId,
                SubjectId = Guid.NewGuid()
            };

            var releaseFiles = new List<ReleaseFile>
            {
                new ReleaseFile
                {
                    Id = Guid.NewGuid(),
                    Release = release,
                    ReleaseId = releaseId,
                    ReleaseFileReference = dataFileReference1,
                    ReleaseFileReferenceId = dataFileReference1.Id
                },
                new ReleaseFile
                {
                    Id = Guid.NewGuid(),
                    Release = release,
                    ReleaseId = releaseId,
                    ReleaseFileReference = dataFileReference2,
                    ReleaseFileReferenceId = dataFileReference2.Id
                }
            };

            using (var context = InMemoryApplicationDbContext("CreateReleaseAmendment"))
            {
                context.AddRange(new List<ReleaseType>
                {
                    releaseType
                });

                context.Add(new Publication
                {
                    Id = publicationId,
                    Releases = new List<Release>
                    {
                        release
                    }
                });

                context.Add(createdBy);
                context.Add(new User
                {
                    Id = _userId
                });
                context.AddRange(userReleaseRoles);
                context.AddRange(releaseFiles);

                context.SaveChanges();
            }

            var newReleaseId = Guid.Empty;
            
            using (var context = InMemoryApplicationDbContext("CreateReleaseAmendment"))
            {
                var newReleaseIdCapture = new CaptureMatch<Guid>(id => newReleaseId = id, id => id != null);

                fileStorageService
                    .Setup(s => s.CopyReleaseFilesAsync(releaseId, Capture.With(newReleaseIdCapture),
                        ReleaseFileTypes.Ancillary))
                    .ReturnsAsync(() =>
                    {
                        var newRelease = context.Releases.FirstOrDefault(r => r.Id == newReleaseId);
                        return new Either<ActionResult, Release>(newRelease);
                    });

                fileStorageService
                    .Setup(s => s.CopyReleaseFilesAsync(releaseId, Capture.With(newReleaseIdCapture),
                        ReleaseFileTypes.Chart))
                    .ReturnsAsync(() =>
                    {
                        var newRelease = context.Releases.FirstOrDefault(r => r.Id == newReleaseId);
                        return new Either<ActionResult, Release>(newRelease);
                    });

                var releaseService = new ReleaseService(context, AdminMapper(),
                    publishingService.Object, new PersistenceHelper<ContentDbContext>(context), userService.Object,
                    repository.Object,
                    subjectService.Object, tableStorageService.Object, fileStorageService.Object,
                    importStatusService.Object, footnoteService.Object);

                // Method under test 
                var amendmentViewModel = releaseService.CreateReleaseAmendmentAsync(releaseId).Result.Right;
                Assert.Equal(newReleaseId, amendmentViewModel.Id);
            }
            
            using (var context = InMemoryApplicationDbContext("CreateReleaseAmendment"))
            {
                var amendment = context
                    .Releases
                    .Include(r => r.Original)
                    .Include(r => r.Type)
                    .Include(r => r.CreatedBy)
                    .Include(r => r.Publication)
                    .Include(r => r.Content)
                    .ThenInclude(c => c.ContentSection)
                    .ThenInclude(c => c.Content)
                    .ThenInclude(c => c.Comments)
                    .Include(r => r.Updates)
                    .Include(r => r.ContentBlocks)
                    .ThenInclude(r => r.ContentBlock)
                    .First(r => r.Id == newReleaseId);

                // check fields that should be set to new values for an amendment, rather than copied from its original
                // Release
                Assert.Equal(newReleaseId, amendment.Id);
                Assert.Null(amendment.PublishScheduled);
                Assert.Null( amendment.Published);
                Assert.Equal(release.Version + 1, amendment.Version);
                Assert.Equal(ReleaseStatus.Draft, amendment.Status);
                Assert.Equal(release.Id, amendment.Original.Id);
                Assert.Equal(release.Id, amendment.OriginalId);
                Assert.Equal(createdBy.Id, amendment.CreatedBy.Id);
                Assert.Equal(createdBy.Id, amendment.CreatedById);
                Assert.True(amendment.Created.CompareTo(DateTime.UtcNow.AddSeconds(-1)) > 0 
                            && amendment.Created.CompareTo(DateTime.UtcNow) <= 0);
                Assert.Null(amendment.InternalReleaseNote);

                Assert.Equal(releaseType, amendment.Type);
                Assert.Equal(releaseType.Id, amendment.TypeId);
                Assert.Equal(nextReleaseDate, amendment.NextReleaseDate); 
                Assert.Equal(releaseName, amendment.ReleaseName);
                Assert.Equal(timePeriodCoverage, amendment.TimePeriodCoverage);
                Assert.Equal(publicationId, amendment.PublicationId);
                
                Assert.Equal(release.RelatedInformation.Count, amendment.RelatedInformation.Count);
                amendment.RelatedInformation.ForEach(amended =>
                {
                    var index = amendment.RelatedInformation.IndexOf(amended);
                    var original = release.RelatedInformation[index];
                    AssertAmendedBasicLinkCorrect(amended, original);
                });
                
                Assert.Equal(release.Updates.Count, amendment.Updates.Count);
                amendment.Updates.ForEach(amended =>
                {
                    var index = amendment.Updates.IndexOf(amended);
                    var original = release.Updates[index];
                    AssertAmendedUpdateCorrect(amended, original, amendment);
                });
             
                Assert.Equal(release.Content.Count, amendment.Content.Count);
                amendment.Content.ForEach(amended =>
                {
                    var index = amendment.Content.IndexOf(amended);
                    var original = release.Content[index];
                    AssertAmendedContentSectionCorrect(amendment, amended, original);
                });
                
                Assert.Equal(release.ContentBlocks.Count, amendment.ContentBlocks.Count);
                var amendmentContentBlock1 = amendment.ContentBlocks[0].ContentBlock;
                var amendmentContentBlock2 = amendment.ContentBlocks[1].ContentBlock;
                var amendmentContentBlock1InContent = amendment.Content[0].ContentSection.Content[0];

                // Check that the DataBlock that is included in this Release amendment's Content is successfully
                // identified as the exact same DataBlock that is attached to the Release amendment through the 
                // additional "Release.ContentBlocks" relationship (which is used to determine which Data Blocks
                // belong to which Release when a Data Block has not yet been - or is removed from - the Release's 
                // Content
                Assert.NotEqual(dataBlock1.Id, amendmentContentBlock1.Id);
                Assert.Equal(amendmentContentBlock1, amendmentContentBlock1InContent);
                
                // and check that the Data Block that is not yet included in any content is copied across OK still 
                Assert.NotEqual(dataBlock2.Id, amendmentContentBlock2.Id);
                Assert.Equal((amendmentContentBlock2 as DataBlock).Name, dataBlock2.Name);

                var amendmentReleaseRoles = context
                    .UserReleaseRoles
                    .Where(r => r.ReleaseId == amendment.Id)
                    .ToList();
                
                Assert.Equal(userReleaseRoles.Count, amendmentReleaseRoles.Count);
                var approverAmendmentRole = amendmentReleaseRoles.First(r => r.Role == ReleaseRole.Approver);
                AssertAmendedReleaseRoleCorrect(approverReleaseRole, approverAmendmentRole, amendment);

                var contributorAmendmentRole = amendmentReleaseRoles.First(r => r.Role == ReleaseRole.Contributor);
                Assert.NotEqual(contributorReleaseRole.Id, contributorAmendmentRole.Id);
                AssertAmendedReleaseRoleCorrect(contributorReleaseRole, contributorAmendmentRole, amendment);

                var amendmentDataFiles = context
                    .ReleaseFiles
                    .Include(f => f.ReleaseFileReference)
                    .Where(f => f.ReleaseId == amendment.Id)
                    .ToList();
                
                Assert.Equal(releaseFiles.Count, amendmentDataFiles.Count);

                var amendmentDataFile = amendmentDataFiles[0];
                var originalFile = releaseFiles.First(f =>
                    f.ReleaseFileReference.Filename == amendmentDataFile.ReleaseFileReference.Filename);
                AssertAmendedReleaseFileCorrect(originalFile, amendmentDataFile, amendment);

                var amendmentDataFile2 = amendmentDataFiles[1];
                var originalFile2 = releaseFiles.First(f =>
                    f.ReleaseFileReference.Filename == amendmentDataFile2.ReleaseFileReference.Filename);
                AssertAmendedReleaseFileCorrect(originalFile2, amendmentDataFile2, amendment);

                Assert.True(amendment.Amendment);
            }
        }

       private static void AssertAmendedBasicLinkCorrect(BasicLink amended, BasicLink original)
       {
           Assert.True(amended.Id != Guid.Empty);
           Assert.NotEqual(original.Id, amended.Id);
           Assert.Equal(original.Description, amended.Description);
           Assert.Equal(original.Url, amended.Url);
       }

       private static void AssertAmendedUpdateCorrect(Update amended, Update original, Release amendment)
       {
           Assert.True(amended.Id != Guid.Empty);
           Assert.NotEqual(original.Id, amended.Id);
           Assert.Equal(original.On, amended.On);
           Assert.Equal(original.Reason, amended.Reason);
           Assert.Equal(amendment, amended.Release);
           Assert.Equal(amendment.Id, amended.ReleaseId);
       }

       private static void AssertAmendedContentSectionCorrect(Release amendment, ReleaseContentSection amended,
           ReleaseContentSection original)
       {
           Assert.Equal(amendment, amended.Release);
           Assert.Equal(amendment.Id, amended.ReleaseId);
           Assert.True(amended.ContentSectionId != Guid.Empty);
           Assert.NotEqual(original.ContentSectionId, amended.ContentSectionId);

           var originalSection = original.ContentSection;
           var amendedSection = amended.ContentSection;

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

       private static void AssertAmendedContentBlockCorrect(IContentBlock originalBlock, IContentBlock amendedBlock,
           ContentSection amendedSection)
       {
           Assert.NotEqual(originalBlock.Id, amendedBlock.Id);
           Assert.Equal(originalBlock.Order, amendedBlock.Order);
           Assert.Equal(originalBlock.Type, amendedBlock.Type);
           Assert.Equal(amendedSection, amendedBlock.ContentSection);
           Assert.Equal(amendedSection.Id, amendedBlock.ContentSectionId);
           Assert.NotEmpty(originalBlock.Comments);
           Assert.Empty(amendedBlock.Comments);
       }

       private static void AssertAmendedReleaseRoleCorrect(UserReleaseRole original, UserReleaseRole amended,
           Release amendment)
       {
           Assert.NotEqual(original.Id, amended.Id);
           Assert.Equal(amendment, amended.Release);
           Assert.Equal(amendment.Id, amended.ReleaseId);
           Assert.Equal(original.UserId, amended.UserId);
           Assert.Equal(original.Role, amended.Role);
       }

       private static void AssertAmendedReleaseFileCorrect(ReleaseFile originalFile, ReleaseFile amendmentDataFile, Release amendment)
       {
           // assert it's a new link table entry between the Release amendment and the data file reference 
           Assert.NotEqual(originalFile.Id, amendmentDataFile.Id);
           Assert.Equal(amendment, amendmentDataFile.Release);
           Assert.Equal(amendment.Id, amendmentDataFile.ReleaseId);
           
           // and assert that the file referenced is the SAME file reference as linked from the original Release's
           // link table entry
           Assert.Equal(originalFile.ReleaseFileReference.Id, amendmentDataFile.ReleaseFileReference.Id);
       }

       private (
            Mock<IUserService>, 
            Mock<IPersistenceHelper<ContentDbContext>>, 
            Mock<IPublishingService>,
            Mock<IReleaseRepository>,
            Mock<ISubjectService>,
            Mock<ITableStorageService>,
            Mock<IFileStorageService>,
            Mock<IImportStatusService>,
            Mock<IFootnoteService>) Mocks()
        {
            var userService = MockUtils.AlwaysTrueUserService();

            userService
                .Setup(s => s.GetUserId())
                .Returns(_userId);

            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
            MockUtils.SetupCall<ContentDbContext, Release>(persistenceHelper);
            MockUtils.SetupCall<ContentDbContext, Publication>(persistenceHelper);
            
            return (
                userService, 
                persistenceHelper, 
                new Mock<IPublishingService>(), 
                new Mock<IReleaseRepository>(),
                new Mock<ISubjectService>(), 
                new Mock<ITableStorageService>(), 
                new Mock<IFileStorageService>(),
                new Mock<IImportStatusService>(),
                new Mock<IFootnoteService>());
        }
    }
}