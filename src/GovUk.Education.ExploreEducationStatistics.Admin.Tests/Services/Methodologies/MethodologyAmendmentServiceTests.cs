#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies;

public class MethodologyAmendmentServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task CreateMethodologyAmendment_ClonesBasicFields()
    {
        var originalVersion = new MethodologyVersion
        {
            // general fields
            Id = Guid.NewGuid(),
            AlternativeTitle = "Alternative Title",
            AlternativeSlug = "alternative-slug",

            // creation and update fields
            Created = DateTime.Today.AddDays(-2),
            CreatedBy = new User(),
            CreatedById = Guid.NewGuid(),
            Updated = DateTime.Today.AddDays(-10),

            // approval and publishing fields
            Status = Approved,
            Published = DateTime.Today.AddDays(-1),
            PublishingStrategy = MethodologyPublishingStrategy.WithRelease,
            ScheduledWithReleaseVersion = new ReleaseVersion(),
            ScheduledWithReleaseVersionId = Guid.NewGuid(),

            // versioning fields
            Version = 1,
            PreviousVersion = new MethodologyVersion(),
            PreviousVersionId = Guid.NewGuid(),

            // methodology field
            Methodology = new Methodology(),
            MethodologyId = Guid.NewGuid()
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.MethodologyVersions.AddAsync(originalVersion);
            await context.SaveChangesAsync();
        }

        Guid amendmentId;

        // Call the method under test
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var service = BuildService(context, methodologyService: methodologyService.Object);

            var amendmentCapture = new List<MethodologyVersion>();
            var viewModel = new MethodologyVersionViewModel();

            methodologyService
                .Setup(s => s.BuildMethodologyVersionViewModel(Capture.In(amendmentCapture)))
                .ReturnsAsync(viewModel);

            var result = await service.CreateMethodologyAmendment(originalVersion.Id);
            VerifyAllMocks(methodologyService);

            result.AssertRight(viewModel);
            var amendment = Assert.Single(amendmentCapture);
            amendmentId = amendment.Id;
        }

        // Check that the amendment was successfully saved.  More detailed field-by-field testing is available in
        // the MethodologyVersionTests class.
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var amendment = await context.MethodologyVersions
                .Include(m => m.Notes)
                .Include(m => m.MethodologyContent)
                .Include(m => m.CreatedBy)
                .Include(m => m.ScheduledWithReleaseVersion)
                .SingleAsync(m => m.Id == amendmentId);

            Assert.True(amendment.Amendment);

            // Check general fields.
            Assert.NotEqual(Guid.Empty, amendment.Id);
            Assert.NotEqual(originalVersion.Id, amendment.Id);
            Assert.Equal(originalVersion.AlternativeTitle, amendment.AlternativeTitle);
            Assert.Equal(originalVersion.AlternativeSlug, amendment.AlternativeSlug);

            // Check creation and update fields.
            amendment.Created.AssertUtcNow();
            Assert.Null(amendment.CreatedBy);
            Assert.Equal(_userId, amendment.CreatedById);
            Assert.Null(amendment.Updated);

            // Check approval and publishing fields.
            Assert.Equal(Draft, amendment.Status);
            Assert.Null(amendment.Published);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, amendment.PublishingStrategy);
            Assert.Null(amendment.ScheduledWithReleaseVersion);
            Assert.Null(amendment.ScheduledWithReleaseVersionId);

            // Check versioning fields.
            Assert.Equal(originalVersion.Version + 1, amendment.Version);
            Assert.Equal(originalVersion.Id, amendment.PreviousVersionId);
            Assert.Equal(originalVersion.Id, amendment.PreviousVersionId);

            // Check methodology field.
            Assert.Equal(originalVersion.MethodologyId, amendment.MethodologyId);
        }
    }

    [Fact]
    public async Task CreateMethodologyAmendment_ClonesAnnexContentSections()
    {
        var originalVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            MethodologyContent = new MethodologyVersionContent {
                Annexes = new List<ContentSection>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Caption = "Annex caption",
                        Heading = "Annex heading",
                        Order = 2,
                        Type = ContentSectionType.Generic,
                        Content = ListOf<ContentBlock>(new HtmlBlock
                        {
                            Id = Guid.NewGuid(),
                            Body = "Annex body",
                            Created = DateTime.Today.AddDays(-13),
                            Order = 5,
                            ContentSection = new ContentSection(),
                            ContentSectionId = Guid.NewGuid(),
                            Comments = new List<Comment>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Content = "Annex comment",
                                    Created = DateTime.Today.AddDays(-4),
                                    CreatedById = Guid.NewGuid(),
                                    Updated = DateTime.Today.AddDays(-3),
                                    CreatedBy = new User()
                                }
                            }
                        })
                    }
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.MethodologyVersions.AddAsync(originalVersion);
            await context.SaveChangesAsync();
        }

        Guid amendmentId;

        // Call the method under test
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var service = BuildService(context, methodologyService: methodologyService.Object);

            var amendmentCapture = new List<MethodologyVersion>();
            var viewModel = new MethodologyVersionViewModel();

            methodologyService
                .Setup(s => s.BuildMethodologyVersionViewModel(Capture.In(amendmentCapture)))
                .ReturnsAsync(viewModel);

            var result = await service.CreateMethodologyAmendment(originalVersion.Id);
            VerifyAllMocks(methodologyService);

            result.AssertRight(viewModel);
            var amendment = Assert.Single(amendmentCapture);
            amendmentId = amendment.Id;
        }

        // Check that the amendment was successfully saved.  More detailed field-by-field testing is available in
        // the MethodologyVersionTests class.
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var amendment = await context.MethodologyVersions
                .Include(m => m.MethodologyContent)
                .SingleAsync(m => m.Id == amendmentId);

            AssertContentSectionAmendedCorrectly(
                amendment.MethodologyContent.Annexes[0],
                originalVersion.MethodologyContent.Annexes[0]);
        }
    }

    [Fact]
    public async Task CreateMethodologyAmendment_ClonesContentSections()
    {
        var originalVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            MethodologyContent = new MethodologyVersionContent {
                Content = new List<ContentSection>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Caption = "Content caption",
                        Heading = "Content heading",
                        Order = 2,
                        Type = ContentSectionType.Generic,
                        Content = ListOf<ContentBlock>(new HtmlBlock
                        {
                            Id = Guid.NewGuid(),
                            Body = "Content body",
                            Comments = new List<Comment>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Content = "Content comment",
                                    Created = DateTime.Today.AddDays(-4),
                                    CreatedById = Guid.NewGuid(),
                                    Updated = DateTime.Today.AddDays(-3),
                                    CreatedBy = new User()
                                }
                            }
                        })
                    }
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.MethodologyVersions.AddAsync(originalVersion);
            await context.SaveChangesAsync();
        }

        Guid amendmentId;

        // Call the method under test
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var service = BuildService(context, methodologyService: methodologyService.Object);

            var amendmentCapture = new List<MethodologyVersion>();
            var viewModel = new MethodologyVersionViewModel();

            methodologyService
                .Setup(s => s.BuildMethodologyVersionViewModel(Capture.In(amendmentCapture)))
                .ReturnsAsync(viewModel);

            var result = await service.CreateMethodologyAmendment(originalVersion.Id);
            VerifyAllMocks(methodologyService);

            result.AssertRight(viewModel);
            var amendment = Assert.Single(amendmentCapture);
            amendmentId = amendment.Id;
        }

        // Check that the amendment was successfully saved.  More detailed field-by-field testing is available in
        // the MethodologyVersionTests class.
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var amendment = await context.MethodologyVersions
                .Include(m => m.MethodologyContent)
                .SingleAsync(m => m.Id == amendmentId);

            AssertContentSectionAmendedCorrectly(
                amendment.MethodologyContent.Content[0],
                originalVersion.MethodologyContent.Content[0]);
        }
    }

    private static void AssertContentSectionAmendedCorrectly(
        ContentSection amendmentContentSection,
        ContentSection originalContentSection)
    {
        // Check the Content Section has a new id.
        Assert.NotEqual(Guid.Empty, amendmentContentSection.Id);
        Assert.NotEqual(originalContentSection.Id, amendmentContentSection.Id);

        // Check the other basic Content Section fields are the same
        Assert.Equal(originalContentSection.Caption, amendmentContentSection.Caption);
        Assert.Equal(originalContentSection.Heading, amendmentContentSection.Heading);
        Assert.Equal(originalContentSection.Order, amendmentContentSection.Order);
        Assert.Equal(originalContentSection.Type, amendmentContentSection.Type);

        var amendmentContentBlock = Assert.IsType<HtmlBlock>(Assert.Single(amendmentContentSection.Content));
        var originalContentBlock = Assert.IsType<HtmlBlock>(originalContentSection.Content[0]);

        // Check the Content Block has a new id.
        Assert.NotEqual(Guid.Empty, amendmentContentBlock.Id);
        Assert.NotEqual(originalContentBlock.Id, amendmentContentBlock.Id);

        // Check the other Content Block basic fields.
        Assert.Equal(originalContentBlock.Body, amendmentContentBlock.Body);
        amendmentContentBlock.Created.AssertUtcNow();
        Assert.Equal(originalContentBlock.Order, amendmentContentBlock.Order);

        // Check the Content Section is null. Content Blocks within JSON do not need an owning
        // Content Section back reference.
        Assert.Null(amendmentContentBlock.ContentSection);
        Assert.Null(amendmentContentBlock.ContentSectionId);

        // Check the amendment's Content Block Comments are empty as we are starting with a clean slate.
        Assert.Empty(amendmentContentBlock.Comments);
    }

    [Fact]
    public async Task CreateMethodologyAmendment_ClonesNotes()
    {
        var originalVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid()
        };

        originalVersion.Notes = new List<MethodologyNote>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Content = "Note 1",
                DisplayDate = DateTime.Today.ToUniversalTime(),
                MethodologyVersion = originalVersion,
                MethodologyVersionId = originalVersion.Id,
                Created = DateTime.Today.AddDays(-6).ToUniversalTime(),
                CreatedBy = new User(),
                CreatedById = Guid.NewGuid(),
                Updated = DateTime.Today.AddDays(-5).ToUniversalTime(),
                UpdatedBy = new User(),
                UpdatedById = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Content = "Note 2",
                DisplayDate = DateTime.Today.ToUniversalTime(),
                MethodologyVersion = originalVersion,
                MethodologyVersionId = originalVersion.Id,
                Created = DateTime.Today.AddDays(-4).ToUniversalTime(),
                CreatedBy = new User(),
                CreatedById = Guid.NewGuid(),
                Updated = DateTime.Today.AddDays(-3).ToUniversalTime(),
                UpdatedBy = new User(),
                UpdatedById = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Content = "Note 3",
                DisplayDate = DateTime.Today.ToUniversalTime(),
                MethodologyVersion = originalVersion,
                MethodologyVersionId = originalVersion.Id,
                Created = DateTime.Today.AddDays(-2).ToUniversalTime(),
                CreatedBy = new User(),
                CreatedById = Guid.NewGuid(),
                Updated = DateTime.Today.AddDays(-1).ToUniversalTime(),
                UpdatedBy = new User(),
                UpdatedById = Guid.NewGuid()
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.MethodologyVersions.AddAsync(originalVersion);
            await context.SaveChangesAsync();
        }

        Guid amendmentId;

        // Call the method under test
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var service = BuildService(context, methodologyService: methodologyService.Object);

            var amendmentCapture = new List<MethodologyVersion>();
            var viewModel = new MethodologyVersionViewModel();

            methodologyService
                .Setup(s => s.BuildMethodologyVersionViewModel(Capture.In(amendmentCapture)))
                .ReturnsAsync(viewModel);

            var result = await service.CreateMethodologyAmendment(originalVersion.Id);
            VerifyAllMocks(methodologyService);

            result.AssertRight(viewModel);
            var amendment = Assert.Single(amendmentCapture);
            amendmentId = amendment.Id;
        }

        // Check that the amendment was successfully saved.  More detailed field-by-field testing is available in
        // the MethodologyVersionTests class.
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var amendment = await context
                .MethodologyVersions
                .Include(m => m.Notes)
                .SingleAsync(m => m.Id == amendmentId);

            Assert.Equal(3, amendment.Notes.Count);

            AssertMethodologyNoteAmendedCorrectly(amendment.Notes[0], originalVersion.Notes[0], amendment);
            AssertMethodologyNoteAmendedCorrectly(amendment.Notes[1], originalVersion.Notes[1], amendment);
            AssertMethodologyNoteAmendedCorrectly(amendment.Notes[2], originalVersion.Notes[2], amendment);
        }
    }

    private void AssertMethodologyNoteAmendedCorrectly(
        MethodologyNote amendmentNote,
        MethodologyNote originalNote,
        MethodologyVersion amendment)
    {
        // Check the note has a new id.
        Assert.NotEqual(Guid.Empty, amendmentNote.Id);
        Assert.NotEqual(originalNote.Id, amendmentNote.Id);

        // Check the note has the amended methodology version
        Assert.Equal(amendment, amendmentNote.MethodologyVersion);
        Assert.Equal(amendment.Id, amendmentNote.MethodologyVersionId);

        // Check the other fields are the same
        Assert.Equal(originalNote.Content, amendmentNote.Content);
        amendmentNote.Created.AssertUtcNow();
        Assert.Equal(_userId, amendmentNote.CreatedById);
        Assert.Equal(originalNote.DisplayDate, amendmentNote.DisplayDate);
        Assert.NotNull(originalNote.Updated);
        Assert.NotNull(originalNote.UpdatedById);
        Assert.Null(amendmentNote.Updated);
        Assert.Null(amendmentNote.UpdatedById);
    }

    [Fact]
    public async Task CreateMethodologyAmendment_ClonesMethodologyFiles()
    {
        var originalVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            Status = Approved,
            Published = DateTime.Today,
            Methodology = new Methodology
            {
                OwningPublicationTitle = "Owning Publication Title",
                OwningPublicationSlug = "owning-publication-slug",
            },
            MethodologyContent = new MethodologyVersionContent {
                Content = AsList(new ContentSection
                {
                    Content = AsList<ContentBlock>(new HtmlBlock
                    {
                        Body = "Content!"
                    })
                })
            }
        };

        var originalMethodologyFile1 = new MethodologyFile
        {
            Id = Guid.NewGuid(),
            MethodologyVersionId = originalVersion.Id,
            File = new File
            {
                Id = Guid.NewGuid()
            }
        };

        var originalMethodologyFile2 = new MethodologyFile
        {
            Id = Guid.NewGuid(),
            MethodologyVersionId = originalVersion.Id,
            File = new File
            {
                Id = Guid.NewGuid()
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.MethodologyVersions.AddAsync(originalVersion);
            await context.MethodologyFiles.AddRangeAsync(originalMethodologyFile1, originalMethodologyFile2);
            await context.SaveChangesAsync();
        }

        Guid amendmentId;

        // Call the method under test
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var service = BuildService(context, methodologyService: methodologyService.Object);

            var amendmentCapture = new List<MethodologyVersion>();
            var viewModel = new MethodologyVersionViewModel();

            methodologyService
                .Setup(s => s.BuildMethodologyVersionViewModel(Capture.In(amendmentCapture)))
                .ReturnsAsync(viewModel);

            var result = await service.CreateMethodologyAmendment(originalVersion.Id);
            VerifyAllMocks(methodologyService);

            result.AssertRight(viewModel);
            var amendment = Assert.Single(amendmentCapture);
            amendmentId = amendment.Id;
        }

        // Check that the Methodology Files were successfully linked to the Amendment.
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var amendmentMethodologyFiles = await context
                .MethodologyFiles
                .AsQueryable()
                .Where(f => f.MethodologyVersionId == amendmentId)
                .ToListAsync();

            Assert.Equal(2, amendmentMethodologyFiles.Count());

            var methodologyFile1ForAmendment =
                Assert.Single(amendmentMethodologyFiles, 
                    f => f.FileId == originalMethodologyFile1.FileId);

            var methodologyFile2ForAmendment =
                Assert.Single(amendmentMethodologyFiles, 
                    f => f.FileId == originalMethodologyFile2.FileId);

            Assert.NotEqual(originalMethodologyFile1.Id, methodologyFile1ForAmendment.Id);
            Assert.NotEqual(originalMethodologyFile2.Id, methodologyFile2ForAmendment.Id);
        }
    }

    private MethodologyAmendmentService BuildService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IMethodologyService? methodologyService = null,
        IUserService? userService = null)
    {
        return new(
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService ?? AlwaysTrueUserService(_userId).Object,
            methodologyService ?? Mock.Of<IMethodologyService>(Strict),
            contentDbContext
        );
    }
}
