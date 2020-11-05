using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class ReleaseTests
    {
        [Fact]
        public void Live_True()
        {
            var releasePublished = new Release
            {
                Published = DateTime.Now.AddDays(-1)
            };

            Assert.True(releasePublished.Live);
        }

        [Fact]
        public void Live_False_PublishedIsNull()
        {
            var releaseNoPublishedDate = new Release
            {
                Published = null
            };

            Assert.False(releaseNoPublishedDate.Live);
        }

        [Fact]
        public void Live_False_PublishedInFuture()
        {
            // Note that this should not happen, but we test the edge case.
            var releasePublishedDateInFuture = new Release
            {
                Published = DateTime.Now.AddDays(1)
            };

            Assert.False(releasePublishedDateInFuture.Live);
        }

        [Fact]
        public void NextReleaseDate_Ok()
        {
            var releaseDate = new PartialDate {Day = "01"};
            var release = new Release
            {
                NextReleaseDate = releaseDate
            };

            Assert.Equal(releaseDate, release.NextReleaseDate);
        }

        [Fact]
        public void NextReleaseDate_InvalidDate()
        {
            Assert.Throws<FormatException>(
                () => new Release
                {
                    NextReleaseDate = new PartialDate {Day = "45"}
                }
            );
        }

        [Fact]
        public void ReleaseName_Ok()
        {
            // None should throw
            new Release {ReleaseName = "1990"};
            new Release {ReleaseName = "2011"};
            new Release {ReleaseName = "3000"};
            new Release {ReleaseName = ""}; // considered not set
            new Release {ReleaseName = null}; // considered not set
        }

        [Fact]
        public void ReleaseName_InvalidFormats()
        {
            Assert.Throws<FormatException>(
                () => new Release {ReleaseName = "Hello"}
            );

            Assert.Throws<FormatException>(
                () => new Release {ReleaseName = "190"}
            );

            Assert.Throws<FormatException>(
                () => new Release {ReleaseName = "ABC123"}
            );

            Assert.Throws<FormatException>(
                () => new Release {ReleaseName = "20000"}
            );
        }

        [Fact]
        public void CreateReleaseAmendment_CorrectBasicDetails()
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

            var amendment = release.CreateReleaseAmendment(createdDate, createdById);

            Assert.NotEqual(release.Id, amendment.Id);
            Assert.Equal(2, amendment.Version);
            Assert.Equal(release.Id, amendment.PreviousVersionId);

            Assert.Null(amendment.Published);
            Assert.Null(amendment.PublishScheduled);
            Assert.Equal(ReleaseStatus.Draft, amendment.Status);

            Assert.Equal(createdDate, amendment.Created);
            Assert.Equal(createdById, amendment.CreatedById);
        }

        [Fact]
        public void CreateReleaseAmendment_ClonesContent()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            var section1Id = Guid.NewGuid();
            var section2Id = Guid.NewGuid();

            release.Content = new List<ReleaseContentSection>
            {
                new ReleaseContentSection
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
                new ReleaseContentSection
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

            var amendment = release.CreateReleaseAmendment(createdDate, createdById);

            Assert.Equal(2, amendment.Content.Count);

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
        }

        [Fact]
        public void CreateReleaseAmendment_ClonesContent_RemovesComments()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            var section1Id = Guid.NewGuid();
            var section2Id = Guid.NewGuid();

            release.Content = new List<ReleaseContentSection>
            {
                new ReleaseContentSection
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
                                    new Comment
                                    {
                                        Content = "Comment 1"
                                    }
                                }
                            },
                        },
                    }
                },
                new ReleaseContentSection
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
                                    new Comment
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

            var amendment = release.CreateReleaseAmendment(createdDate, createdById);

            Assert.Equal(2, amendment.Content.Count);

            var section1 = amendment.Content[0];
            var section2 = amendment.Content[1];

            var block1 = Assert.IsType<HtmlBlock>(section1.ContentSection.Content[0]);
            Assert.Empty(block1.Comments);

            var block2 = Assert.IsType<HtmlBlock>(section2.ContentSection.Content[0]);
            Assert.Empty(block2.Comments);
        }

        [Fact]
        public void CreateReleaseAmendment_ClonesRelatedInformation()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            release.RelatedInformation = new List<Link>
            {
                new Link
                {
                    Id = Guid.NewGuid(),
                    Description = "Link 1 description",
                    Url = "Link 1 url"
                },
                new Link
                {
                    Id = Guid.NewGuid(),
                    Description = "Link 2 description",
                    Url = "Link 2 url"
                }
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateReleaseAmendment(createdDate, createdById);

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
        public void CreateReleaseAmendment_ClonesUpdates()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            release.Updates = new List<Update>
            {
                new Update
                {
                    Id = Guid.NewGuid(),
                    Reason = "Update 1 reason",
                    Release = release,
                    ReleaseId = release.Id

                },
                new Update
                {
                    Id = Guid.NewGuid(),
                    Reason = "Update 2 reason",
                    Release = release,
                    ReleaseId = release.Id
                }
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateReleaseAmendment(createdDate, createdById);

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
        public void CreateReleaseAmendment_ClonesContentBlocks()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            var block1Id = Guid.NewGuid();
            var block2Id = Guid.NewGuid();

            release.ContentBlocks = new List<ReleaseContentBlock>
            {
                new ReleaseContentBlock
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
                new ReleaseContentBlock
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

            var amendment = release.CreateReleaseAmendment(createdDate, createdById);

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
        public void CreateReleaseAmendment_ClonesContentBlocks_SameAsBlocksInContent()
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
                new ReleaseContentSection
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
                new ReleaseContentBlock
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock.Id,
                    ContentBlock = contentBlock
                },
                new ReleaseContentBlock
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = dataBlock.Id,
                    ContentBlock =  dataBlock
                },
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateReleaseAmendment(createdDate, createdById);

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
        public void CreateReleaseAmendment_UpdatesFastTrackLinkIds()
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
                Order = 1,
                Body = $"<p>Content block 3 <a href=\"http://localhost/fast-track/{dataBlock1.Id}\">link text</a></p>"
            };
            var contentBlock4 = new HtmlBlock
            {
                Id = Guid.NewGuid(),
                Order = 2,
                Body = $@"
                    <p>Content block 4 http://localhost/fast-track/{dataBlock1.Id} http://localhost/fast-track/{dataBlock2.Id}</p>
                    <p><a href=""http://localhost/fast-track/{dataBlock1.Id}"">link 1 text</a></p>
                    <p><a href=""http://localhost/fast-track/{dataBlock2.Id}/"">link 2 text</a></p>
                    "
            };

            release.ContentBlocks = new List<ReleaseContentBlock>
            {
                new ReleaseContentBlock
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = dataBlock1.Id,
                    ContentBlock = dataBlock1
                },
                new ReleaseContentBlock
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = dataBlock2.Id,
                    ContentBlock = dataBlock2
                },
                new ReleaseContentBlock
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock1.Id,
                    ContentBlock = contentBlock1
                },
                new ReleaseContentBlock
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock2.Id,
                    ContentBlock = contentBlock2
                },
                new ReleaseContentBlock
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock3.Id,
                    ContentBlock = contentBlock3
                },
                new ReleaseContentBlock
                {
                    ReleaseId = release.Id,
                    Release = release,
                    ContentBlockId = contentBlock4.Id,
                    ContentBlock = contentBlock4
                },
            };

            var section1Id = Guid.NewGuid();
            var section2Id = Guid.NewGuid();

            release.Content = new List<ReleaseContentSection>
            {
                new ReleaseContentSection
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
                new ReleaseContentSection
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
                            contentBlock4
                        },
                    }
                }
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = release.CreateReleaseAmendment(createdDate, createdById);

            var amendmentDataBlock1 = Assert.IsType<DataBlock>(amendment.ContentBlocks[0].ContentBlock);
            var amendmentDataBlock2 = Assert.IsType<DataBlock>(amendment.ContentBlocks[1].ContentBlock);

            Assert.NotEqual(dataBlock1.Id, amendmentDataBlock1.Id);
            Assert.NotEqual(dataBlock2.Id, amendmentDataBlock2.Id);

            var section1 = amendment.Content[0].ContentSection;

            var amendmentContentBlock1 = Assert.IsType<HtmlBlock>(section1.Content[0]);
            var amendmentContentBlock2 = Assert.IsType<HtmlBlock>(section1.Content[1]);

            Assert.Equal(
                $"Content block 1 http://localhost/fast-track/{amendmentDataBlock1.Id}",
                amendmentContentBlock1.Body
            );
            Assert.Equal(
                $"Content block 2 http://localhost/fast-track/{amendmentDataBlock2.Id}/ some other text",
                amendmentContentBlock2.Body
            );

            var section2 = amendment.Content[1].ContentSection;

            var amendmentContentBlock3 = Assert.IsType<HtmlBlock>(section2.Content[0]);
            var amendmentContentBlock4 = Assert.IsType<HtmlBlock>(section2.Content[1]);

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
    }
}