#nullable enable
using System;
using System.Collections.Generic;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class MethodologyVersionTests
    {
        [Fact]
        public void ScheduledForPublishingImmediately_TrueWhenPublishingStrategyIsImmediately()
        {
            var methodology = new MethodologyVersion
            {
                PublishingStrategy = Immediately
            };

            Assert.True(methodology.ScheduledForPublishingImmediately);
        }

        [Fact]
        public void ScheduledForPublishingImmediately_FalseWhenPublishingStrategyIsWithRelease()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var methodology = new MethodologyVersion
            {
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = release.Id,
                ScheduledWithRelease = release
            };

            Assert.False(methodology.ScheduledForPublishingImmediately);
        }

        [Fact]
        public void ScheduledForPublishingWithRelease_FalseWhenPublishingStrategyIsImmediately()
        {
            var methodology = new MethodologyVersion
            {
                PublishingStrategy = Immediately
            };

            Assert.False(methodology.ScheduledForPublishingWithRelease);
        }

        [Fact]
        public void ScheduledForPublishingWithRelease_TrueWhenPublishingStrategyIsWithRelease()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var methodology = new MethodologyVersion
            {
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = release.Id,
                ScheduledWithRelease = release
            };

            Assert.True(methodology.ScheduledForPublishingWithRelease);
        }

        [Fact]
        public void ScheduledForPublishingWithPublishedRelease_TrueWhenScheduledWithLiveRelease()
        {
            var liveRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow
            };

            var methodology = new MethodologyVersion
            {
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = liveRelease,
                ScheduledWithReleaseId = liveRelease.Id
            };

            Assert.True(methodology.ScheduledForPublishingWithPublishedRelease);
        }

        [Fact]
        public void ScheduledForPublishingWithPublishedRelease_FalseWhenScheduledWithNonLiveRelease()
        {
            var nonLiveRelease = new Release
            {
                Id = Guid.NewGuid()
            };

            var methodology = new MethodologyVersion
            {
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = nonLiveRelease,
                ScheduledWithReleaseId = nonLiveRelease.Id
            };

            Assert.False(methodology.ScheduledForPublishingWithPublishedRelease);
        }

        [Fact]
        public void ScheduledForPublishingWithPublishedRelease_FalseWhenPublishingStrategyIsImmediately()
        {
            var methodology = new MethodologyVersion
            {
                PublishingStrategy = Immediately
            };

            Assert.False(methodology.ScheduledForPublishingWithPublishedRelease);
        }

        [Fact]
        public void ScheduledForPublishingWithPublishedRelease_ScheduledWithReleaseNotIncluded()
        {
            var methodology = new MethodologyVersion
            {
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = Guid.NewGuid()
            };

            Assert.Throws<InvalidOperationException>(() => methodology.ScheduledForPublishingWithPublishedRelease);
        }

        [Fact]
        public void GetTitle_NoAlternativeTitleSet()
        {
            var methodology = new MethodologyVersion
            {
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Owning Publication Title"
                }
            };
            
            Assert.Equal(methodology.Methodology.OwningPublicationTitle, methodology.Title);
        }

        [Fact]
        public void GetTitle_AlternativeTitleSet()
        {
            var methodology = new MethodologyVersion
            {
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Owning Publication Title"
                },
                AlternativeTitle = "Alternative Title"
            };
            
            Assert.Equal(methodology.AlternativeTitle, methodology.Title);
        }

        [Fact]
        public void GetSlug()
        {
            var methodology = new MethodologyVersion
            {
                Methodology = new Methodology
                {
                    Slug = "owning-publication-slug"
                }
            };
            
            Assert.Equal(methodology.Methodology.Slug, methodology.Slug);
        }

        [Fact]
        public void CreateMethodologyAmendment()
        {
            var userId = Guid.NewGuid();

            var originalVersion = new MethodologyVersion
            {
                // general fields
                Id = Guid.NewGuid(),
                AlternativeTitle = "Alternative Title",
                InternalReleaseNote = "Internal Release Note",

                // creation and update fields
                Created = DateTime.Today.AddDays(-2),
                CreatedBy = new User(),
                CreatedById = Guid.NewGuid(),
                Updated = DateTime.Today.AddDays(-10),
                
                // approval and publishing fields
                Status = Approved,
                Published = DateTime.Today.AddDays(-1),
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = new Release(),
                ScheduledWithReleaseId = Guid.NewGuid(),

                // versioning fields
                Version = 1,
                PreviousVersion = new MethodologyVersion(),
                PreviousVersionId = Guid.NewGuid(),
                
                // methodology field
                Methodology = new Methodology(),
                MethodologyId = Guid.NewGuid(),
                
                // annex field
                Annexes = AsList(new ContentSection
                {
                    Id = Guid.NewGuid(),
                    Caption = "Annex caption",
                    Heading = "Annex heading",
                    Order = 2,
                    Type = ContentSectionType.Generic,
                    Content = AsList<ContentBlock>(new HtmlBlock
                    {
                        Id = Guid.NewGuid(),
                        Body = "Annex body",
                        Created = DateTime.Today.AddDays(-13),
                        Order = 5,
                        ContentSection = new ContentSection(),
                        ContentSectionId = Guid.NewGuid(),
                        Comments = AsList(new Comment
                        {
                            Id = Guid.NewGuid(),
                            Content = "Annex comment",
                            Created = DateTime.Today.AddDays(-4),
                            CreatedById = Guid.NewGuid(),
                            Updated = DateTime.Today.AddDays(-3),
                            CreatedBy = new User()
                        })
                    })
                }),
                
                // content field
                Content = AsList(new ContentSection
                {
                    Id = Guid.NewGuid(),
                    Caption = "Content caption",
                    Heading = "Content heading",
                    Order = 2,
                    Type = ContentSectionType.Generic,
                    Content = AsList<ContentBlock>(new HtmlBlock
                    {
                        Id = Guid.NewGuid(),
                        Body = "Content body",
                        Comments = AsList(new Comment
                        {
                            Id = Guid.NewGuid(),
                            Content = "Content comment",
                            Created = DateTime.Today.AddDays(-4),
                            CreatedById = Guid.NewGuid(),
                            Updated = DateTime.Today.AddDays(-3),
                            CreatedBy = new User()
                        })
                    }),
                }),

                // notes field
                Notes = new List<MethodologyNote>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Content = "Note content",
                        Created = DateTime.Today.AddDays(-4),
                        CreatedBy = new User(),
                        CreatedById = Guid.NewGuid(),
                        DisplayDate = DateTime.Today.ToUniversalTime(),
                        Updated = DateTime.Today.AddDays(-3),
                        UpdatedBy = new User(),
                        UpdatedById = Guid.NewGuid()
                    }
                }
            };
            
            Assert.False(originalVersion.Amendment);

            var creationTime = DateTime.Today.AddMinutes(-2);
            
            var amendment = originalVersion.CreateMethodologyAmendment(creationTime, userId);
            
            Assert.True(amendment.Amendment);

            // Check general fields.
            Assert.NotEqual(Guid.Empty, amendment.Id);
            Assert.NotEqual(originalVersion.Id, amendment.Id);
            Assert.Equal(originalVersion.AlternativeTitle, amendment.AlternativeTitle);
            Assert.Null(amendment.InternalReleaseNote);

            // Check creation and update fields.
            Assert.Equal(creationTime, amendment.Created);
            Assert.Null(amendment.CreatedBy);
            Assert.Equal(userId, amendment.CreatedById);
            Assert.Null(amendment.Updated);

            // Check approval and publishing fields.
            Assert.Equal(Draft, amendment.Status);
            Assert.Null(amendment.Published);
            Assert.Equal(Immediately, amendment.PublishingStrategy);
            Assert.Null(amendment.ScheduledWithRelease);
            Assert.Null(amendment.ScheduledWithReleaseId);

            // Check versioning fields.
            Assert.Equal(originalVersion.Version + 1, amendment.Version);
            Assert.Equal(originalVersion.Id, amendment.PreviousVersionId);
            Assert.Null(amendment.PreviousVersion);

            // Check methodology field.
            Assert.Equal(originalVersion.MethodologyId, amendment.MethodologyId);
            Assert.Null(amendment.Methodology);

            // Check Annex Content Sections.
            var amendmentAnnexSection = Assert.Single(amendment.Annexes);
            var originalAnnexSection = originalVersion.Annexes[0];
            AssertContentSectionAmendedCorrectly(amendmentAnnexSection, originalAnnexSection, creationTime);
            
            // Check Content Content Sections.
            var amendmentContentSection = Assert.Single(amendment.Content);
            var originalContentSection = originalVersion.Content[0];
            AssertContentSectionAmendedCorrectly(amendmentContentSection, originalContentSection, creationTime);

            // Check notes
            var amendmentNote = Assert.Single(amendment.Notes);
            var originalNote = originalVersion.Notes[0];
            AssertMethodologyNoteAmendedCorrectly(amendmentNote, originalNote, amendment);
        }

        private static void AssertContentSectionAmendedCorrectly(ContentSection amendmentContentSection,
            ContentSection originalContentSection, DateTime creationTime)
        {
            // Check the Content Section has a new id.
            Assert.NotEqual(Guid.Empty, amendmentContentSection.Id);
            Assert.NotEqual(originalContentSection.Id, amendmentContentSection.Id);

            // Check the other basic Content Section fields are the same
            Assert.Equal(originalContentSection.Caption, amendmentContentSection.Caption);
            Assert.Equal(originalContentSection.Heading, amendmentContentSection.Heading);
            Assert.Equal(originalContentSection.Order, amendmentContentSection.Order);
            Assert.Equal(originalContentSection.Type, amendmentContentSection.Type);

            var amendmentContentBlock = Assert.Single(amendmentContentSection.Content) as HtmlBlock;
            var originalContentBlock = originalContentSection.Content[0] as HtmlBlock;

            // Check the Content Block has a new id.
            Assert.NotEqual(Guid.Empty, amendmentContentBlock.Id);
            Assert.NotEqual(originalContentBlock.Id, amendmentContentBlock.Id);
            
            // Check the other Content Block basic fields.
            Assert.Equal(originalContentBlock.Body, amendmentContentBlock.Body);
            Assert.Equal(creationTime, amendmentContentBlock.Created);
            Assert.Equal(originalContentBlock.Order, amendmentContentBlock.Order);

            // Check the Content Block belongs to the new amended Content Section. 
            Assert.Null(amendmentContentBlock.ContentSection);
            Assert.Equal(amendmentContentSection.Id, amendmentContentBlock.ContentSectionId);

            // Check the amendment's Content Block Comments are empty as we are starting with a clean slate.
            Assert.Empty(amendmentContentBlock.Comments);
        }

        private static void AssertMethodologyNoteAmendedCorrectly(
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
            Assert.Equal(originalNote.Created, amendmentNote.Created);
            Assert.Equal(originalNote.CreatedBy, amendmentNote.CreatedBy);
            Assert.Equal(originalNote.CreatedById, amendmentNote.CreatedById);
            Assert.Equal(originalNote.DisplayDate, amendmentNote.DisplayDate);
            Assert.Equal(originalNote.Updated, amendmentNote.Updated);
            Assert.Equal(originalNote.UpdatedBy, amendmentNote.UpdatedBy);
            Assert.Equal(originalNote.UpdatedById, amendmentNote.UpdatedById);
        }

        [Fact]
        public void AmendmentFlag_NotAmendment_FirstUnpublishedVersion()
        {
            var methodology = new MethodologyVersion
            {
                PreviousVersionId = null,
                Published = null
            };
            
            Assert.False(methodology.Amendment);
        }

        [Fact]
        public void AmendmentFlag_NotAmendment_FirstPublishedVersion()
        {
            var methodology = new MethodologyVersion
            {
                PreviousVersionId = null,
                Published = DateTime.Now
            };
            
            Assert.False(methodology.Amendment);
        }

        [Fact]
        public void AmendmentFlag_Amendment_UnpublishedNewVersion()
        {
            var methodology = new MethodologyVersion
            {
                PreviousVersionId = Guid.NewGuid(),
                Published = null
            };
            
            Assert.True(methodology.Amendment);
        }

        [Fact]
        public void AmendmentFlag_NotAmendment_PublishedNewVersion()
        {
            var methodology = new MethodologyVersion
            {
                PreviousVersionId = Guid.NewGuid(),
                Published = DateTime.Now
            };
            
            Assert.False(methodology.Amendment);
        }

        [Fact]
        public void DraftFirstVersionFlag()
        {
            var methodology = new MethodologyVersion
            {
                Status = Draft,
                PreviousVersionId = null
            };
            
            Assert.True(methodology.DraftFirstVersion);
        }

        [Fact]
        public void DraftFirstVersionFlag_NotDraft()
        {
            var methodology = new MethodologyVersion
            {
                Status = Approved,
                PreviousVersionId = null
            };
            
            Assert.False(methodology.DraftFirstVersion);
        }
        
        [Fact]
        public void DraftFirstVersionFlag_NotFirstVersion()
        {
            var methodology = new MethodologyVersion
            {
                Status = Draft,
                PreviousVersionId = Guid.NewGuid()
            };
            
            Assert.False(methodology.DraftFirstVersion);
        }
    }
}
