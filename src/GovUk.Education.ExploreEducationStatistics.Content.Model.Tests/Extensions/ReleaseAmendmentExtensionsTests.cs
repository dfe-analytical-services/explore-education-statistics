#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions
{
    public class ReleaseAmendmentExtensionsTests
    {
        [Fact]
        public void CreateAmendment_CorrectBasicDetails()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Version = 1,
                Published = DateTime.Parse("2020-10-10T13:00:00"),
                PublishScheduled = DateTime.Parse("2020-10-09T12:00:00")
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateAmendment(createdDate, createdById);

            Assert.NotEqual(release.Id, amendment.Id);
            Assert.Equal(2, amendment.Version);
            Assert.Equal(release.Id, amendment.PreviousVersionId);

            Assert.Null(amendment.Published);
            Assert.Null(amendment.PublishScheduled);
            Assert.Equal(ReleaseApprovalStatus.Draft, amendment.ApprovalStatus);

            Assert.Equal(createdDate, amendment.Created);
            Assert.Equal(createdById, amendment.CreatedById);
        }

        [Fact]
        public void CreateAmendment_ClonesContent()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
            };

            var section1Id = Guid.NewGuid();
            var section2Id = Guid.NewGuid();

            release.Content = new List<ReleaseContentSection>
            {
                new()
                {
                    Release = release,
                    ReleaseId = release.Id,
                    ContentSectionId = section1Id,
                    ContentSection = new ContentSection
                    {
                        Id = section1Id,
                        Heading = "Section 1",
                        Content = new List<ContentBlock>
                        {
                            new HtmlBlock
                            {
                                Id = Guid.NewGuid(),
                                Order = 1,
                                Body = "Block 1 body"
                            },
                            new DataBlock
                            {
                                Id = Guid.NewGuid(),
                                Order = 2,
                                Heading = "Block 2 heading",
                                Name = "Block 2 name",
                                Source = "Block 2 source"
                            }
                        },
                    }
                },
                new()
                {
                    Release = release,
                    ReleaseId = release.Id,
                    ContentSectionId = section2Id,
                    ContentSection = new ContentSection
                    {
                        Id = section2Id,
                        Heading = "Section 2",
                        Content = new List<ContentBlock>
                        {
                            new HtmlBlock
                            {
                                Id = Guid.NewGuid(),
                                Order = 1,
                                Body = "Block 3 body"
                            },
                        },
                    }
                }
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateAmendment(createdDate, createdById);

            Assert.Equal(3, amendment.Content.Count);

            var section1 = amendment.Content[0];
            Assert.Equal(amendment, section1.Release);
            Assert.Equal(amendment.Id, section1.ReleaseId);

            Assert.NotEqual(release.Content[0].ContentSectionId, section1.ContentSectionId);
            Assert.NotEqual(release.Content[0].ContentSection.Id, section1.ContentSection.Id);
            Assert.Equal("Section 1", section1.ContentSection.Heading);

            Assert.Equal(2, section1.ContentSection.Content.Count);

            var block1 = Assert.IsType<HtmlBlock>(section1.ContentSection.Content[0]);

            Assert.NotEqual(release.Content[0].ContentSection.Content[0].Id, block1.Id);
            Assert.Equal(1, block1.Order);
            Assert.Equal("Block 1 body", block1.Body);

            var block2 = Assert.IsType<DataBlock>(section1.ContentSection.Content[1]);
            Assert.NotEqual(release.Content[0].ContentSection.Content[1].Id, block2.Id);
            Assert.Equal(2, block2.Order);
            Assert.Equal("Block 2 heading", block2.Heading);
            Assert.Equal("Block 2 name", block2.Name);
            Assert.Equal("Block 2 source", block2.Source);

            var section2 = amendment.Content[1];

            Assert.Equal(amendment, section2.Release);
            Assert.Equal(amendment.Id, section2.ReleaseId);

            Assert.NotEqual(release.Content[1].ContentSectionId, section2.ContentSectionId);
            Assert.NotEqual(release.Content[1].ContentSection.Id, section2.ContentSection.Id);
            Assert.Equal("Section 2", section2.ContentSection.Heading);

            Assert.Single(section2.ContentSection.Content);

            var block3 = Assert.IsType<HtmlBlock>(section2.ContentSection.Content[0]);
            Assert.NotEqual(release.Content[1].ContentSection.Content[0].Id, block3.Id);
            Assert.Equal(1, block3.Order);
            Assert.Equal("Block 3 body", block3.Body);

            // NOTE: If a RelatedDashboards ContentSection doesn't exist, one gets created on amendments
            // because older releases may not have one.
            var section3 = amendment.Content[2];

            Assert.Equal(amendment, section3.Release);
            Assert.Equal(amendment.Id, section3.ReleaseId);

            Assert.Equal(ContentSectionType.RelatedDashboards, section3.ContentSection.Type);
            Assert.Empty(section3.ContentSection.Content);
        }

        [Fact]
        public void CreateAmendment_ClonesContent_RemovesComments()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
            };

            var section1Id = Guid.NewGuid();
            var section2Id = Guid.NewGuid();

            release.Content = new List<ReleaseContentSection>
            {
                new()
                {
                    Release = release,
                    ReleaseId = release.Id,
                    ContentSectionId = section1Id,
                    ContentSection = new ContentSection
                    {
                        Id = section1Id,
                        Heading = "Section 1",
                        Content = new List<ContentBlock>
                        {
                            new HtmlBlock
                            {
                                Id = Guid.NewGuid(),
                                Order = 1,
                                Body = "Block 1 body",
                                Comments = new List<Comment>
                                {
                                    new()
                                    {
                                        Content = "Comment 1"
                                    }
                                }
                            },
                        },
                    }
                },
                new()
                {
                    Release = release,
                    ReleaseId = release.Id,
                    ContentSectionId = section2Id,
                    ContentSection = new ContentSection
                    {
                        Id = section2Id,
                        Heading = "Section 2",
                        Content = new List<ContentBlock>
                        {
                            new HtmlBlock
                            {
                                Id = Guid.NewGuid(),
                                Order = 1,
                                Body = "Block 2",
                                Comments = new List<Comment>
                                {
                                    new()
                                    {
                                        Content = "Comment 1"
                                    }
                                }
                            },
                        },
                    }
                }
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateAmendment(createdDate, createdById);

            Assert.Equal(3, amendment.Content.Count);

            var section1 = amendment.Content[0];
            var section2 = amendment.Content[1];
            var section3 = amendment.Content[2];

            var block1 = Assert.IsType<HtmlBlock>(section1.ContentSection.Content[0]);
            Assert.Empty(block1.Comments);

            var block2 = Assert.IsType<HtmlBlock>(section2.ContentSection.Content[0]);
            Assert.Empty(block2.Comments);

            // NOTE: Some older releases do not have a RelatedDashboards ContentSection, so it is created
            // on amendments.
            Assert.Equal(ContentSectionType.RelatedDashboards, section3.ContentSection.Type);
            Assert.Empty(section3.ContentSection.Content);
        }

        [Fact]
        public void CreateAmendment_ClonesRelatedInformation()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                RelatedInformation = new List<Link>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Description = "Link 1 description",
                        Url = "Link 1 url"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Description = "Link 2 description",
                        Url = "Link 2 url"
                    }
                }
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateAmendment(createdDate, createdById);

            Assert.Equal(2, amendment.RelatedInformation.Count);

            var link1 = amendment.RelatedInformation[0];

            Assert.NotEqual(release.RelatedInformation[0].Id, link1.Id);
            Assert.Equal("Link 1 description", link1.Description);
            Assert.Equal("Link 1 url", link1.Url);

            var link2 = amendment.RelatedInformation[1];

            Assert.NotEqual(release.RelatedInformation[1].Id, link2.Id);
            Assert.Equal("Link 2 description", link2.Description);
            Assert.Equal("Link 2 url", link2.Url);
        }

        [Fact]
        public void CreateAmendment_ClonesUpdates()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            release.Updates = new List<Update>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Reason = "Update 1 reason",
                    Release = release,
                    ReleaseId = release.Id

                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Reason = "Update 2 reason",
                    Release = release,
                    ReleaseId = release.Id
                }
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateAmendment(createdDate, createdById);

            Assert.Equal(2, amendment.Updates.Count);

            var update1 = amendment.Updates[0];

            Assert.NotEqual(release.Updates[0].Id, update1.Id);
            Assert.Equal("Update 1 reason", update1.Reason);
            Assert.Equal(amendment, update1.Release);
            Assert.Equal(amendment.Id, update1.ReleaseId);

            var update2 = amendment.Updates[1];

            Assert.NotEqual(release.Updates[1].Id, update2.Id);
            Assert.Equal("Update 2 reason", update2.Reason);
            Assert.Equal(amendment, update2.Release);
            Assert.Equal(amendment.Id, update2.ReleaseId);
        }

        [Fact]
        public void CreateAmendment_ClonesContentBlocks()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            var block1Id = Guid.NewGuid();
            var block2Id = Guid.NewGuid();

            release.ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = block1Id,
                    ContentBlock = new HtmlBlock
                    {
                        Id = block1Id,
                        Order = 1,
                        Body = "Block 1 body"
                    }
                },
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = block2Id,
                    ContentBlock =  new DataBlock
                    {
                        Id = block2Id,
                        Order = 2,
                        Heading = "Block 2 heading",
                        Name = "Block 2 name",
                        Source = "Block 2 source"
                    }
                },
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateAmendment(createdDate, createdById);

            Assert.Equal(2, amendment.ContentBlocks.Count);

            var releaseBlock1 = amendment.ContentBlocks[0];
            Assert.Equal(amendment, releaseBlock1.Release);
            Assert.Equal(amendment.Id, releaseBlock1.ReleaseId);
            Assert.NotEqual(release.ContentBlocks[0].ContentBlockId, releaseBlock1.ContentBlockId);

            var block1 = Assert.IsType<HtmlBlock>(releaseBlock1.ContentBlock);
            Assert.NotEqual(release.ContentBlocks[0].ContentBlock.Id, block1.Id);
            Assert.Equal(1, block1.Order);
            Assert.Equal("Block 1 body", block1.Body);

            var releaseBlock2 = amendment.ContentBlocks[1];
            Assert.Equal(amendment, releaseBlock2.Release);
            Assert.Equal(amendment.Id, releaseBlock2.ReleaseId);
            Assert.NotEqual(release.ContentBlocks[1].ContentBlockId, releaseBlock2.ContentBlockId);

            var block2 = Assert.IsType<DataBlock>(releaseBlock2.ContentBlock);
            Assert.NotEqual(release.ContentBlocks[1].ContentBlock.Id, block2.Id);
            Assert.Equal(2, block2.Order);
            Assert.Equal("Block 2 heading", block2.Heading);
            Assert.Equal("Block 2 name", block2.Name);
            Assert.Equal("Block 2 source", block2.Source);
        }

        [Fact]
        public void CreateAmendment_ClonesContentBlocks_SameAsBlocksInContent()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            var section1Id = Guid.NewGuid();

            var contentBlock = new HtmlBlock
            {
                Id = Guid.NewGuid(),
                Order = 1,
                Body = "Block 1 body",
                ContentSectionId = section1Id,
            };
            var dataBlock = new DataBlock
            {
                Id = Guid.NewGuid(),
                Order = 2,
                Heading = "Block 2 heading",
                Name = "Block 2 name",
                Source = "Block 2 source",
                ContentSectionId = section1Id,
            };

            release.Content = new List<ReleaseContentSection>
            {
                new()
                {
                    Release = release,
                    ReleaseId = release.Id,
                    ContentSectionId = section1Id,
                    ContentSection = new ContentSection
                    {
                        Id = section1Id,
                        Heading = "Section 1",
                        Content = new List<ContentBlock>
                        {
                            contentBlock,
                            dataBlock,
                        },
                    }
                },
            };

            release.ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock.Id,
                    ContentBlock = contentBlock
                },
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = dataBlock.Id,
                    ContentBlock =  dataBlock
                },
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateAmendment(createdDate, createdById);

            Assert.Equal(2, amendment.ContentBlocks.Count);

            var releaseBlock1 = amendment.ContentBlocks[0];
            Assert.Equal(amendment, releaseBlock1.Release);
            Assert.Equal(amendment.Id, releaseBlock1.ReleaseId);
            Assert.NotEqual(release.ContentBlocks[0].ContentBlockId, releaseBlock1.ContentBlockId);

            var block1 = Assert.IsType<HtmlBlock>(releaseBlock1.ContentBlock);
            Assert.NotEqual(release.ContentBlocks[0].ContentBlock.Id, block1.Id);
            Assert.Equal(1, block1.Order);
            Assert.Equal("Block 1 body", block1.Body);

            var contentSection1Block1 = Assert.IsType<HtmlBlock>(amendment.Content[0].ContentSection.Content[0]);
            Assert.Equal(block1, contentSection1Block1);

            var releaseBlock2 = amendment.ContentBlocks[1];
            Assert.Equal(amendment, releaseBlock2.Release);
            Assert.Equal(amendment.Id, releaseBlock2.ReleaseId);
            Assert.NotEqual(release.ContentBlocks[1].ContentBlockId, releaseBlock2.ContentBlockId);

            var block2 = Assert.IsType<DataBlock>(releaseBlock2.ContentBlock);
            Assert.NotEqual(release.ContentBlocks[1].ContentBlock.Id, block2.Id);
            Assert.Equal(2, block2.Order);
            Assert.Equal("Block 2 heading", block2.Heading);
            Assert.Equal("Block 2 name", block2.Name);
            Assert.Equal("Block 2 source", block2.Source);

            var contentSection1Block2 = Assert.IsType<DataBlock>(amendment.Content[0].ContentSection.Content[1]);
            Assert.Equal(block2, contentSection1Block2);
        }

        [Fact]
        public void CreateAmendment_CopiesKeyStatistics()
        {
            var dataBlock = new DataBlock
            {
                Name = "DataBlock name",
            };
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Version = 1,
                Published = DateTime.Parse("2020-10-10T13:00:00"),
                PublishScheduled = DateTime.Parse("2020-10-09T12:00:00"),
                KeyStatistics = new List<KeyStatistic>
                {
                    new KeyStatisticText
                    {
                        Id = Guid.NewGuid(),
                        Title = "KeyStatText title",
                        Statistic = "KeyStatText statistic",
                        Trend = "KeyStatText trend",
                        GuidanceTitle = "KeyStatText guidance title",
                        GuidanceText = "KeyStatText guidance text",
                        Order = 0,
                        Created = new DateTime(2023, 01, 01),
                        Updated = new DateTime(2023, 01, 02),
                    },
                    new KeyStatisticDataBlock
                    {
                        Id = Guid.NewGuid(),
                        DataBlock = dataBlock,
                        Trend = "KeyStatDataBlock trend",
                        GuidanceTitle = "KeyStatDataBlock guidance title",
                        GuidanceText = "KeyStatDataBlock guidance text",
                        Order = 1,
                        Created = new DateTime(2023, 01, 03),
                        Updated = new DateTime(2023, 01, 04),
                    },
                },
                ContentBlocks = new List<ReleaseContentBlock>
                {
                    new ()
                    {
                        ContentBlock = dataBlock,
                    },
                },
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateAmendment(createdDate, createdById);

            Assert.NotEqual(release.Id, amendment.Id);
            Assert.Equal(2, amendment.Version);
            Assert.Equal(release.Id, amendment.PreviousVersionId);

            Assert.Null(amendment.Published);
            Assert.Null(amendment.PublishScheduled);
            Assert.Equal(ReleaseApprovalStatus.Draft, amendment.ApprovalStatus);

            Assert.Equal(2, amendment.KeyStatistics.Count);

            var amendmentKeyStatText = Assert.IsType<KeyStatisticText>(amendment.KeyStatistics[0]);
            var originalKeyStatText = (KeyStatisticText)release.KeyStatistics[0];
            Assert.NotEqual(originalKeyStatText.Id, amendmentKeyStatText.Id);
            Assert.Equal(originalKeyStatText.Title, amendmentKeyStatText.Title);
            Assert.Equal(originalKeyStatText.Statistic, amendmentKeyStatText.Statistic);
            Assert.Equal(originalKeyStatText.Trend, amendmentKeyStatText.Trend);
            Assert.Equal(originalKeyStatText.GuidanceTitle, amendmentKeyStatText.GuidanceTitle);
            Assert.Equal(originalKeyStatText.GuidanceText, amendmentKeyStatText.GuidanceText);
            Assert.Equal(originalKeyStatText.Order, amendmentKeyStatText.Order);
            Assert.Equal(originalKeyStatText.Created, amendmentKeyStatText.Created);
            Assert.Equal(originalKeyStatText.Updated, amendmentKeyStatText.Updated);

            var amendmentKeyStatDataBlock = Assert.IsType<KeyStatisticDataBlock>(amendment.KeyStatistics[1]);
            var originalKeyStatDataBlock = (KeyStatisticDataBlock)release.KeyStatistics[1];
            Assert.NotEqual(originalKeyStatDataBlock.Id, amendmentKeyStatDataBlock.Id);
            Assert.Equal(originalKeyStatDataBlock.Trend, amendmentKeyStatDataBlock.Trend);
            Assert.Equal(originalKeyStatDataBlock.GuidanceTitle, amendmentKeyStatDataBlock.GuidanceTitle);
            Assert.Equal(originalKeyStatDataBlock.GuidanceText, amendmentKeyStatDataBlock.GuidanceText);
            Assert.Equal(originalKeyStatDataBlock.Order, amendmentKeyStatDataBlock.Order);
            Assert.Equal(originalKeyStatDataBlock.Created, amendmentKeyStatDataBlock.Created);
            Assert.Equal(originalKeyStatDataBlock.Updated, amendmentKeyStatDataBlock.Updated);

            var originalDataBlock = (DataBlock)release.ContentBlocks.Single().ContentBlock;
            var amendmentDataBlock = Assert.IsType<DataBlock>(amendment.ContentBlocks.Single().ContentBlock);
            Assert.Equal(originalDataBlock.Name, amendmentDataBlock.Name);
            Assert.NotEqual(
                originalDataBlock.Id,
                amendmentDataBlock.Id);

            // Check original and amendment data blocks are linked to correct key stats
            Assert.Equal(originalDataBlock.Id, originalKeyStatDataBlock.DataBlockId);
            Assert.Equal(
                amendmentDataBlock.Id,
                amendmentKeyStatDataBlock.DataBlockId);
        }


        [Fact]
        public void CreateAmendment_UpdatesFastTrackLinkIds()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            var dataBlock1 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Order = 1,
                Heading = "Data block 1",
            };
            var dataBlock2 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Order = 1,
                Heading = "Data block 2",
            };
            var contentBlock1 = new HtmlBlock
            {
                Id = Guid.NewGuid(),
                Order = 1,
                Body = $"Content block 1 http://localhost/fast-track/{dataBlock1.Id}"
            };
            var contentBlock2 = new HtmlBlock
            {
                Id = Guid.NewGuid(),
                Order = 2,
                Body = $"Content block 2 http://localhost/fast-track/{dataBlock2.Id}/ some other text"
            };
            var contentBlock3 = new HtmlBlock
            {
                Id = Guid.NewGuid(),
                Order = 3,
                Body = $"<p>Content block 3 <a href=\"http://localhost/fast-track/{dataBlock1.Id}\">link text</a></p>"
            };
            var contentBlock4 = new HtmlBlock
            {
                Id = Guid.NewGuid(),
                Order = 4,
                Body = $@"
                    <p>Content block 4 http://localhost/fast-track/{dataBlock1.Id} http://localhost/fast-track/{dataBlock2.Id}</p>
                    <p><a href=""http://localhost/fast-track/{dataBlock1.Id}"">link 1 text</a></p>
                    <p><a href=""http://localhost/fast-track/{dataBlock2.Id}/"">link 2 text</a></p>
                    "
            };

            release.ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = dataBlock1.Id,
                    ContentBlock = dataBlock1
                },
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = dataBlock2.Id,
                    ContentBlock = dataBlock2
                },
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock1.Id,
                    ContentBlock = contentBlock1
                },
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock2.Id,
                    ContentBlock = contentBlock2
                },
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock3.Id,
                    ContentBlock = contentBlock3
                },
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock4.Id,
                    ContentBlock = contentBlock4
                },
            };

            var section1Id = Guid.NewGuid();

            release.Content = new List<ReleaseContentSection>
            {
                new()
                {
                    Release = release,
                    ReleaseId = release.Id,
                    ContentSectionId = section1Id,
                    ContentSection = new ContentSection
                    {
                        Id = section1Id,
                        Heading = "Section 1",
                        Content = new List<ContentBlock>
                        {
                            contentBlock1,
                            contentBlock2,
                            contentBlock3,
                            contentBlock4
                        },
                    }
                },
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateAmendment(createdDate, createdById);

            var amendmentDataBlock1 = Assert.IsType<DataBlock>(amendment.ContentBlocks[0].ContentBlock);
            var amendmentDataBlock2 = Assert.IsType<DataBlock>(amendment.ContentBlocks[1].ContentBlock);

            Assert.NotEqual(dataBlock1.Id, amendmentDataBlock1.Id);
            Assert.NotEqual(dataBlock2.Id, amendmentDataBlock2.Id);

            var section1 = amendment.Content[0].ContentSection;

            var amendmentContentBlock1 = Assert.IsType<HtmlBlock>(section1.Content[0]);
            var amendmentContentBlock2 = Assert.IsType<HtmlBlock>(section1.Content[1]);
            var amendmentContentBlock3 = Assert.IsType<HtmlBlock>(section1.Content[2]);
            var amendmentContentBlock4 = Assert.IsType<HtmlBlock>(section1.Content[3]);

            Assert.Equal(
                $"Content block 1 http://localhost/fast-track/{amendmentDataBlock1.Id}",
                amendmentContentBlock1.Body
            );
            Assert.Equal(
                $"Content block 2 http://localhost/fast-track/{amendmentDataBlock2.Id}/ some other text",
                amendmentContentBlock2.Body
            );

            Assert.Equal(
                $"<p>Content block 3 <a href=\"http://localhost/fast-track/{amendmentDataBlock1.Id}\">link text</a></p>",
                amendmentContentBlock3.Body
            );
            Assert.Equal(
                $@"
                    <p>Content block 4 http://localhost/fast-track/{amendmentDataBlock1.Id} http://localhost/fast-track/{amendmentDataBlock2.Id}</p>
                    <p><a href=""http://localhost/fast-track/{amendmentDataBlock1.Id}"">link 1 text</a></p>
                    <p><a href=""http://localhost/fast-track/{amendmentDataBlock2.Id}/"">link 2 text</a></p>
                    ",
                amendmentContentBlock4.Body
            );
        }

        [Fact]
        public void CreateAmendment_FiltersContent()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            var dataBlock1 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Order = 1,
                Heading = "Data block 1",
            };
            var contentBlock1 = new HtmlBlock
            {
                Id = Guid.NewGuid(),
                Order = 1,
                Body = $"<p>Content block 1 <a href=\"http://localhost/fast-track/{dataBlock1.Id}\">link</a></p>"
            };

            var contentBlock2 = new HtmlBlock
            {
                Id = Guid.NewGuid(),
                Order = 1,
                Body = @"
                <p>
                    Content 1 <comment-start name=""comment-1""></comment-start>goes here<comment-end name=""comment-1""></comment-end>
                </p>
                <ul>
                    <li><comment-start name=""comment-2""/>Content 2<comment-end name=""comment-2""/></li>
                    <li><commentplaceholder-start name=""comment-3""/>Content 3<commentplaceholder-end name=""comment-3""/></li>
                    <li><resolvedcomment-start name=""comment-4""/>Content 4<resolvedcomment-end name=""comment-4""/></li>
                </ul>".TrimIndent()
            };
            var contentBlock3 = new HtmlBlock
            {
                Id = Guid.NewGuid(),
                Order = 2,
                Body = $@"
                    <p>
                        Content block 3
                        <comment-start name=""comment-1""></comment-start>
                            <a href=""http://localhost/fast-track/{dataBlock1.Id}"">link</a>
                        <comment-end name=""comment-1""></comment-end>

                        <a href=""http://localhost/fast-track/{dataBlock1.Id}"">Another link</a>
                    </p>".TrimIndent()
            };

            release.ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = dataBlock1.Id,
                    ContentBlock = dataBlock1
                },
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock1.Id,
                    ContentBlock = contentBlock1
                },
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock2.Id,
                    ContentBlock = contentBlock2
                },
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock3.Id,
                    ContentBlock = contentBlock3
                },
            };

            // Test that we are amending content across multiple sections too
            var section1Id = Guid.NewGuid();
            var section2Id = Guid.NewGuid();

            release.Content = new List<ReleaseContentSection>
            {
                new()
                {
                    Release = release,
                    ReleaseId = release.Id,
                    ContentSectionId = section1Id,
                    ContentSection = new ContentSection
                    {
                        Id = section1Id,
                        Heading = "Section 1",
                        Content = new List<ContentBlock>
                        {
                            contentBlock1,
                            contentBlock2,
                        },
                    }
                },
                new()
                {
                    Release = release,
                    ReleaseId = release.Id,
                    ContentSectionId = section2Id,
                    ContentSection = new ContentSection
                    {
                        Id = section2Id,
                        Heading = "Section 2",
                        Content = new List<ContentBlock>
                        {
                            contentBlock3,
                        },
                    }
                },
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateAmendment(createdDate, createdById);

            var amendmentDataBlock1 = Assert.IsType<DataBlock>(amendment.ContentBlocks[0].ContentBlock);

            Assert.NotEqual(dataBlock1.Id, amendmentDataBlock1.Id);

            var section1 = amendment.Content[0].ContentSection;
            var section2 = amendment.Content[1].ContentSection;

            var amendmentContentBlock1 = Assert.IsType<HtmlBlock>(section1.Content[0]);
            var amendmentContentBlock2 = Assert.IsType<HtmlBlock>(section1.Content[1]);
            var amendmentContentBlock3 = Assert.IsType<HtmlBlock>(section2.Content[0]);

            Assert.Equal(
                $"<p>Content block 1 <a href=\"http://localhost/fast-track/{amendmentDataBlock1.Id}\">link</a></p>",
                amendmentContentBlock1.Body
            );
            Assert.Equal(
                @"
                    <p>
                        Content 1 goes here
                    </p>
                    <ul>
                        <li>Content 2</li>
                        <li>Content 3</li>
                        <li>Content 4</li>
                    </ul>".TrimIndent(),
                amendmentContentBlock2.Body
            );
            Assert.Equal(
                $@"
                    <p>
                        Content block 3
                        
                            <a href=""http://localhost/fast-track/{amendmentDataBlock1.Id}"">link</a>
                        

                        <a href=""http://localhost/fast-track/{amendmentDataBlock1.Id}"">Another link</a>
                    </p>".TrimIndent(),
                amendmentContentBlock3.Body
            );
        }

        [Fact]
        public void CreateAmendment_NullHtmlBlockBody()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var section1Id = Guid.NewGuid();

            var contentBlock = new HtmlBlock
            {
                Id = Guid.NewGuid(),
                Order = 1,
                Body = null,
                ContentSectionId = section1Id
            };

            var dataBlock = new DataBlock
            {
                Id = Guid.NewGuid(),
                Order = 2,
                Heading = "Block 2 heading",
                Name = "Block 2 name",
                Source = "Block 2 source",
                ContentSectionId = section1Id
            };

            release.Content = new List<ReleaseContentSection>
            {
                new()
                {
                    Release = release,
                    ReleaseId = release.Id,
                    ContentSectionId = section1Id,
                    ContentSection = new ContentSection
                    {
                        Id = section1Id,
                        Heading = "Section 1",
                        Content = new List<ContentBlock>
                        {
                            contentBlock,
                            dataBlock
                        }
                    }
                }
            };

            release.ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock.Id,
                    ContentBlock = contentBlock
                },
                new()
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = dataBlock.Id,
                    ContentBlock =  dataBlock
                }
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            // Minimal test to make sure that a null HtmlBlock body doesn't affect creating a Release amendment
            var amendment = release.CreateAmendment(createdDate, createdById);

            Assert.Equal(2, amendment.ContentBlocks.Count);

            var releaseBlock1 = amendment.ContentBlocks[0];
            Assert.NotEqual(release.ContentBlocks[0].ContentBlockId, releaseBlock1.ContentBlockId);

            var block1 = Assert.IsType<HtmlBlock>(releaseBlock1.ContentBlock);
            Assert.NotEqual(release.ContentBlocks[0].ContentBlock.Id, block1.Id);
            Assert.Null(block1.Body);
        }
    }
}
