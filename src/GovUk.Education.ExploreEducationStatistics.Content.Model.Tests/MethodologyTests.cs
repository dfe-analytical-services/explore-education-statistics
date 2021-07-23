#nullable enable
using System;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class MethodologyTests
    {
        [Fact]
        public void ScheduledForPublishingImmediately_TrueWhenPublishingStrategyIsImmediately()
        {
            var methodology = new Methodology
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

            var methodology = new Methodology
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
            var methodology = new Methodology
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

            var methodology = new Methodology
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

            var methodology = new Methodology
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

            var methodology = new Methodology
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
            var methodology = new Methodology
            {
                PublishingStrategy = Immediately
            };

            Assert.False(methodology.ScheduledForPublishingWithPublishedRelease);
        }

        [Fact]
        public void ScheduledForPublishingWithPublishedRelease_ScheduledWithReleaseNotIncluded()
        {
            var methodology = new Methodology
            {
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = Guid.NewGuid()
            };

            Assert.Throws<InvalidOperationException>(() => methodology.ScheduledForPublishingWithPublishedRelease);
        }

        [Fact]
        public void GetTitle_NoAlternativeTitleSet()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
                {
                    OwningPublicationTitle = "Owning Publication Title"
                }
            };
            
            Assert.Equal(methodology.MethodologyParent.OwningPublicationTitle, methodology.Title);
        }

        [Fact]
        public void GetTitle_AlternativeTitleSet()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
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
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
                {
                    Slug = "owning-publication-slug"
                }
            };
            
            Assert.Equal(methodology.MethodologyParent.Slug, methodology.Slug);
        }

        [Fact]
        public void CreateMethodologyAmendment()
        {
            var userId = Guid.NewGuid();

            var originalMethodology = new Methodology
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
                PreviousVersion = new Methodology(),
                PreviousVersionId = Guid.NewGuid(),
                
                // parent field
                MethodologyParent = new MethodologyParent(),
                MethodologyParentId = Guid.NewGuid(),
                
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
                })
            };
            
            Assert.False(originalMethodology.Amendment);

            var creationTime = DateTime.Today.AddMinutes(-2);
            
            var amendment = originalMethodology.CreateMethodologyAmendment(creationTime, userId);
            
            Assert.True(amendment.Amendment);

            // Check general fields.
            Assert.NotEqual(Guid.Empty, amendment.Id);
            Assert.NotEqual(originalMethodology.Id, amendment.Id);
            Assert.Equal(originalMethodology.AlternativeTitle, amendment.AlternativeTitle);
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
            Assert.Equal(originalMethodology.Version + 1, amendment.Version);
            Assert.Equal(originalMethodology.Id, amendment.PreviousVersionId);
            Assert.Null(amendment.PreviousVersion);

            // Check parent field.
            Assert.Equal(originalMethodology.MethodologyParentId, amendment.MethodologyParentId);
            Assert.Null(amendment.MethodologyParent);

            // Check Annex Content Sections.
            var amendmentAnnexSection = Assert.Single(amendment.Annexes);
            var originalAnnexSection = originalMethodology.Annexes[0];
            AssertContentSectionAmendedCorrectly(amendmentAnnexSection, originalAnnexSection, creationTime);
            
            // Check Content Content Sections.
            var amendmentContentSection = Assert.Single(amendment.Content);
            var originalContentSection = originalMethodology.Content[0];
            AssertContentSectionAmendedCorrectly(amendmentContentSection, originalContentSection, creationTime);
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

        [Fact]
        public void AmendmentFlag_NotAmendment_FirstUnpublishedVersion()
        {
            var methodology = new Methodology
            {
                PreviousVersionId = null,
                Published = null
            };
            
            Assert.False(methodology.Amendment);
        }

        [Fact]
        public void AmendmentFlag_NotAmendment_FirstPublishedVersion()
        {
            var methodology = new Methodology
            {
                PreviousVersionId = null,
                Published = DateTime.Now
            };
            
            Assert.False(methodology.Amendment);
        }

        [Fact]
        public void AmendmentFlag_Amendment_UnpublishedNewVersion()
        {
            var methodology = new Methodology
            {
                PreviousVersionId = Guid.NewGuid(),
                Published = null
            };
            
            Assert.True(methodology.Amendment);
        }

        [Fact]
        public void AmendmentFlag_NotAmendment_PublishedNewVersion()
        {
            var methodology = new Methodology
            {
                PreviousVersionId = Guid.NewGuid(),
                Published = DateTime.Now
            };
            
            Assert.False(methodology.Amendment);
        }

        [Fact]
        public void DraftFirstVersionFlag()
        {
            var methodology = new Methodology
            {
                Status = Draft,
                PreviousVersionId = null
            };
            
            Assert.True(methodology.DraftFirstVersion);
        }

        [Fact]
        public void DraftFirstVersionFlag_NotDraft()
        {
            var methodology = new Methodology
            {
                Status = Approved,
                PreviousVersionId = null
            };
            
            Assert.False(methodology.DraftFirstVersion);
        }
        
        [Fact]
        public void DraftFirstVersionFlag_NotFirstVersion()
        {
            var methodology = new Methodology
            {
                Status = Draft,
                PreviousVersionId = Guid.NewGuid()
            };
            
            Assert.False(methodology.DraftFirstVersion);
        }
    }
}
