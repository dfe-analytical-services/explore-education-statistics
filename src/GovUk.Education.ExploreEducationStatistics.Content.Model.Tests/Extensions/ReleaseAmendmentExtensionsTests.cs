#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions.AssertExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions
{
    public class ReleaseAmendmentExtensionsTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public void CreateAmendment_CorrectBasicDetails()
        {
            var originalRelease = _fixture
                .DefaultRelease()
                .WithPublished(DateTime.Now.AddDays(-2))
                .WithPublishScheduled(DateTime.Now.AddDays(-1))
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .Generate();

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = originalRelease.CreateAmendment(createdDate, createdById);

            // Assert the general amendment fields are copied correctly.
            amendment.AssertDeepEqualTo(originalRelease, Ignoring<Release>(
                r => r.Id,
                r => r.Amendment,
                r => r.Content,
                r => r.RelatedDashboardsSection,
                r => r.PreviousVersionId!,
                r => r.Version,
                r => r.Published!,
                r => r.PublishScheduled!,
                r => r.Live,
                r => r.ApprovalStatus,
                r => r.Created,
                r => r.CreatedById));

            // Assert the amendment has a new id.
            Assert.NotEqual(Guid.Empty, amendment.Id);
            Assert.NotEqual(originalRelease.Id, amendment.Id);

            // Assert the amendment has the Amendment flag.
            Assert.False(originalRelease.Amendment);
            Assert.True(amendment.Amendment);

            // Assert the amendment has a new RelatedDashboards section.
            Assert.Empty(originalRelease.Content);
            var contentSection = Assert.Single(amendment.Content);
            Assert.Equal(ContentSectionType.RelatedDashboards, contentSection.Type);

            // Assert the amendment has an incremented version.
            Assert.Equal(originalRelease.Version + 1, amendment.Version);

            // Assert the amendment is pointing to the original Release with its PreviousVersionId value.
            Assert.Equal(originalRelease.Id, amendment.PreviousVersionId);

            // Assert the Published and PublishScheduled dates are blanked.
            Assert.NotNull(originalRelease.Published);
            Assert.Null(amendment.Published);
            Assert.NotNull(originalRelease.PublishScheduled);
            Assert.Null(amendment.PublishScheduled);

            // Assert the amendment has its Live flag set to false.
            Assert.True(originalRelease.Live);
            Assert.False(amendment.Live);

            // Assert the amendment has its ApprovalStatus initially set to Draft.
            Assert.Equal(ReleaseApprovalStatus.Approved, originalRelease.ApprovalStatus);
            Assert.Equal(ReleaseApprovalStatus.Draft, amendment.ApprovalStatus);

            // Assert the amendment has its Created and CreatedBy set to now and to the current userId.
            Assert.Equal(createdDate, amendment.Created);
            Assert.Equal(createdById, amendment.CreatedById);
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

        // TODO check latestversion and latestpublishedversions on parents
        [Fact]
        public void CreateAmendment_ClonesContentBlocks()
        {
            var dataBlockParents = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(() => _fixture
                    .DefaultDataBlockVersion()
                    .Generate())
                .GenerateList(2);

            // Create a Release with 2 ContentSections and 2 DataBlocks.
            var originalRelease = _fixture
                .DefaultRelease()
                .WithDataBlockVersions(dataBlockParents
                    .Select(dataBlockParent => dataBlockParent.LatestPublishedVersion!))
                .Generate();

            // Add an HtmlBlock to the 1st ContentSection and a DataBlock to the 2nd.
            // Leave a DataBlock unused from Content so we can test that DataBlocks that aren't used in Content
            // are also copied correctly.
            originalRelease.Content = _fixture
                .DefaultContentSection()
                .ForIndex(0, s => s.SetContentBlocks(_fixture
                    .DefaultHtmlBlock()
                    .Generate(1)))
                .ForIndex(1, s => s.SetContentBlocks(
                    ListOf(dataBlockParents[1].LatestPublishedVersion!.ContentBlock)))
                .GenerateList();

            var originalHtmlBlock = (originalRelease.Content[0].Content[0] as HtmlBlock)!;
            var originalDataBlockStandaloneParent = dataBlockParents[0];
            var originalDataBlockInContentParent = dataBlockParents[1];

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = originalRelease.CreateAmendment(createdDate, createdById);

            // Assert that we have both DataBlocks on the amendment.
            Assert.Equal(2, amendment.DataBlockVersions.Count);

            // Assert that the DataBlockVersions and ContentBlocks of type "DataBlock" have matching Ids,
            // for easier migration as part of EES-4640.
            amendment.DataBlockVersions.ForEach(dataBlockVersion =>
                Assert.Equal(dataBlockVersion.Id, dataBlockVersion.ContentBlockId));

            var amendmentDataBlockStandaloneParent = amendment.DataBlockVersions[0].DataBlockParent;
            Assert.Equal(originalDataBlockStandaloneParent.Id, amendmentDataBlockStandaloneParent.Id);
            AssertExistingDataBlockVersionCopiedOk(
                originalDataBlockStandaloneParent,
                amendmentDataBlockStandaloneParent);
            AssertNewDataBlockVersionAddedOk(
                originalDataBlockStandaloneParent,
                amendmentDataBlockStandaloneParent,
                amendment);

            var amendmentDataBlockInContentParent = amendment.DataBlockVersions[1].DataBlockParent;
            Assert.Equal(originalDataBlockInContentParent.Id, amendmentDataBlockInContentParent.Id);
            AssertExistingDataBlockVersionCopiedOk(
                originalDataBlockInContentParent,
                amendmentDataBlockInContentParent);
            AssertNewDataBlockVersionAddedOk(
                originalDataBlockInContentParent,
                amendmentDataBlockInContentParent,
                amendment);

            // Check that we have the expected copy of the Release Content in the amendment.
            // We should have the 2 original ContentSections.
            // Check that we also have a new RelatedDashboards ContentSection added if our original Release
            // didn't have one. We can hopefully remove this with a migration in the future so that all
            // Releases have a RelatedDashboards ContentSection.
            Assert.Equal(3, amendment.Content.Count);
            Assert.Equal(ContentSectionType.Generic, amendment.Content[0].Type);
            Assert.Equal(ContentSectionType.Generic, amendment.Content[1].Type);
            Assert.Equal(ContentSectionType.RelatedDashboards, amendment.Content[2].Type);

            // Grab the 1st generic ContentSection which was copied from the original Release. Assert that its general
            // properties and its HtmlBlock are copied across OK.
            var originalContentSection1 = originalRelease.Content[0];
            var amendmentContentSection1 = amendment.Content[0];
            AssertContentSectionCopiedOk(originalContentSection1, amendmentContentSection1, amendment);

            // Assert that the 1st ContentSection's HtmlBlock is copied OK.
            var amendmentHtmlBlock = Assert.IsType<HtmlBlock>(Assert.Single(amendmentContentSection1.Content));
            AssertHtmlBlockCopiedOk(amendmentContentSection1, amendmentHtmlBlock, originalHtmlBlock, amendment);

            // Grab the 2nd generic ContentSection which was copied from the original Release. Assert that its general
            // properties and its DataBlock are copied across OK.
            var originalContentSection2 = originalRelease.Content[1];
            var amendmentContentSection2 = amendment.Content[1];
            AssertContentSectionCopiedOk(originalContentSection2, amendmentContentSection2, amendment);

            // Assert that the DataBlock in the amendment Content is the same as the one linked directly to the amended
            // Release.
            var amendmentDataBlockFromContent = Assert.Single(amendmentContentSection2.Content);
            Assert.Equal(amendmentDataBlockFromContent, amendmentDataBlockInContentParent.LatestVersion!.ContentBlock);
        }

        private static void AssertExistingDataBlockVersionCopiedOk(
            DataBlockParent originalDataBlockParent,
            DataBlockParent amendmentDataBlockParent)
        {
            var originalDataBlockVersion = originalDataBlockParent.LatestPublishedVersion!;

            // Assert that the original DataBlockVersion has been superseded. The original DataBlockVersion
            // will no longer be the LatestVersion but will still be the LatestPublishedVersion.
            // The new DataBlockVersion will be the next Version up.
            var amendmentDataBlockOriginalVersion = amendmentDataBlockParent.LatestPublishedVersion!;

            // The LatestPublishedVersion on the amendment should be the same LatestPublishedVersion as was on the
            // original Release.
            amendmentDataBlockOriginalVersion.AssertDeepEqualTo(originalDataBlockVersion);
        }

        private static void AssertNewDataBlockVersionAddedOk(
            DataBlockParent originalDataBlockParent,
            DataBlockParent amendmentDataBlockParent,
            Release amendment)
        {
            var originalDataBlockVersion = originalDataBlockParent.LatestPublishedVersion!;

            // Assert that a new DataBlockVersion has been created as a part of creating the Release amendment.
            var amendmentDataBlockOriginalVersion = amendmentDataBlockParent.LatestPublishedVersion!;
            var amendmentDataBlockNewVersion = amendmentDataBlockParent.LatestVersion!;

            // Assert that most of the information from the old DataBlockVersion is carried into the new version.
            amendmentDataBlockNewVersion.AssertDeepEqualTo(originalDataBlockVersion,
                Ignoring<DataBlockVersion>(
                    d => d.Id,
                    d => d.ReleaseId,
                    d => d.Release,
                    d => d.ContentBlockId,
                    d => d.ContentBlock,
                    d => d.Version,
                    d => d.Created,
                    d => d.Comments,
                    d => d.ContentSection!,
                    d => d.ContentSectionId!));

            // Assert the new DataBlockVersion has a new non-empty id of its own.
            Assert.Equal(originalDataBlockVersion.Id, amendmentDataBlockOriginalVersion.Id);
            Assert.Equal(originalDataBlockVersion.Version, amendmentDataBlockOriginalVersion.Version);
            Assert.NotEqual(Guid.Empty, amendmentDataBlockNewVersion.Id);
            Assert.NotEqual(originalDataBlockVersion.Id, amendmentDataBlockNewVersion.Id);

            // Assert the new DataBlockVersion has an incremented Version.
            Assert.Equal(originalDataBlockVersion.Version + 1, amendmentDataBlockNewVersion.Version);

            // Assert the new DataBlockVersion is linked to the new Release amendment rather than the original
            // Release.
            Assert.Equal(amendment, amendmentDataBlockNewVersion.Release);
            Assert.Equal(amendment.Id, amendmentDataBlockNewVersion.ReleaseId);
            Assert.Equal(amendment, amendmentDataBlockNewVersion.ContentBlock.Release);
            Assert.Equal(amendment.Id, amendmentDataBlockNewVersion.ContentBlock.ReleaseId);

            // Assert the Created date has been set to have just been created.
            amendmentDataBlockNewVersion.Created.AssertUtcNow();
        }

        private static void AssertHtmlBlockCopiedOk(
            ContentSection amendmentContentSection,
            HtmlBlock amendmentHtmlBlock,
            HtmlBlock originalHtmlBlock,
            Release amendment)
        {
            amendmentHtmlBlock.AssertDeepEqualTo(originalHtmlBlock, Ignoring<HtmlBlock>(
                b => b.Id,
                b => b.Comments,
                b => b.ContentSection!,
                b => b.ContentSectionId!,
                b => b.ReleaseId,
                b => b.Release));

            // Expect the amendment's ContentBlocks' Comments to be cleared.
            Assert.Equal(2, originalHtmlBlock.Comments.Count);
            Assert.Empty(amendmentHtmlBlock.Comments);

            Assert.Equal(amendmentContentSection.Id, amendmentHtmlBlock.ContentSectionId);
            Assert.Equal(amendmentContentSection, amendmentHtmlBlock.ContentSection);
            Assert.Equal(amendment.Id, amendmentHtmlBlock.ReleaseId);
            Assert.Equal(amendment, amendmentHtmlBlock.Release);
        }

        private static void AssertContentSectionCopiedOk(
            ContentSection originalContentSection,
            ContentSection amendmentContentSection,
            Release amendment)
        {
            // Assert that general properties were copied over when cloning ContentSections.
            amendmentContentSection.AssertDeepEqualTo(originalContentSection, Ignoring<ContentSection>(
                s => s.Id,
                s => s.Release,
                s => s.ReleaseId,
                s => s.Content));

            Assert.Equal(amendment, amendmentContentSection.Release);
            Assert.Equal(amendment.Id, amendmentContentSection.ReleaseId);
        }

        [Fact]
        public void CreateAmendment_CopiesKeyStatistics()
        {
            var dataBlockParents = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(() => _fixture
                    .DefaultDataBlockVersion()
                    .Generate())
                .GenerateList(1);

            var originalRelease = _fixture
                .DefaultRelease()
                .WithDataBlockVersions(dataBlockParents
                    .Select(dataBlockParent => dataBlockParent.LatestPublishedVersion!))
                .Generate();

            var originalDataBlock = dataBlockParents[0].LatestPublishedVersion!.ContentBlock;

            originalRelease.KeyStatistics = new List<KeyStatistic>
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
                    DataBlock = originalDataBlock,
                    Trend = "KeyStatDataBlock trend",
                    GuidanceTitle = "KeyStatDataBlock guidance title",
                    GuidanceText = "KeyStatDataBlock guidance text",
                    Order = 1,
                    Created = new DateTime(2023, 01, 03),
                    Updated = new DateTime(2023, 01, 04),
                }
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = originalRelease.CreateAmendment(createdDate, createdById);

            Assert.NotEqual(originalRelease.Id, amendment.Id);
            Assert.Equal(originalRelease.Version + 1, amendment.Version);
            Assert.Equal(originalRelease.Id, amendment.PreviousVersionId);

            Assert.Null(amendment.Published);
            Assert.Null(amendment.PublishScheduled);
            Assert.Equal(ReleaseApprovalStatus.Draft, amendment.ApprovalStatus);

            Assert.Equal(2, amendment.KeyStatistics.Count);

            // Assert that the KeyStatisticText was copied successfully.
            var amendmentKeyStatText = Assert.IsType<KeyStatisticText>(amendment.KeyStatistics[0]);
            var originalKeyStatText = (KeyStatisticText)originalRelease.KeyStatistics[0];

            amendmentKeyStatText.AssertDeepEqualTo(originalKeyStatText, Ignoring<KeyStatisticText>(
                k => k.Id,
                k => k.ReleaseId,
                k => k.Release));

            Assert.NotEqual(Guid.Empty, amendmentKeyStatText.Id);
            Assert.NotEqual(originalKeyStatText.Id, amendmentKeyStatText.Id);
            Assert.Equal(amendment.Id, amendmentKeyStatText.ReleaseId);
            Assert.Equal(amendment, amendmentKeyStatText.Release);

            // Assert that the KeyStatisticDataBlock was copied successfully.
            var amendmentKeyStatDataBlock = Assert.IsType<KeyStatisticDataBlock>(amendment.KeyStatistics[1]);
            var originalKeyStatDataBlock = (KeyStatisticDataBlock)originalRelease.KeyStatistics[1];
            var amendmentDataBlock = amendment.DataBlockVersions[0].ContentBlock;

            amendmentKeyStatDataBlock.AssertDeepEqualTo(originalKeyStatDataBlock, Ignoring<KeyStatisticDataBlock>(
                k => k.Id,
                k => k.ReleaseId,
                k => k.Release,
                k => k.DataBlock,
                k => k.DataBlockId));

            Assert.NotEqual(Guid.Empty, amendmentKeyStatDataBlock.Id);
            Assert.NotEqual(originalKeyStatDataBlock.Id, amendmentKeyStatDataBlock.Id);
            Assert.Equal(amendment.Id, amendmentKeyStatDataBlock.ReleaseId);
            Assert.Equal(amendment, amendmentKeyStatDataBlock.Release);

            // Check original and amendment data blocks are linked to correct key stats
            Assert.Equal(amendmentDataBlock.Id, amendmentKeyStatDataBlock.DataBlock.Id);
            Assert.Equal(amendmentDataBlock, amendmentKeyStatDataBlock.DataBlock);
        }

        [Fact]
        public void CreateAmendment_CopiesFeaturedTables()
        {
            var dataBlockParents = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(() => _fixture
                    .DefaultDataBlockVersion()
                    .Generate())
                .GenerateList(1);

            var originalRelease = _fixture
                .DefaultRelease()
                .WithDataBlockVersions(dataBlockParents
                    .Select(dataBlockParent => dataBlockParent.LatestPublishedVersion!))
                .WithPublished(DateTime.Now.AddDays(-2))
                .WithPublishScheduled(DateTime.Now.AddDays(-1))
                .Generate();

            var originalDataBlock = dataBlockParents[0]
                .LatestPublishedVersion!
                .ContentBlock;

            originalRelease.FeaturedTables = new List<FeaturedTable>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Featured table 1",
                    Description = "Featured table 1 description",
                    Order = 0,
                    DataBlock = originalDataBlock,
                    DataBlockId = originalDataBlock.Id,
                    ReleaseId = originalRelease.Id,
                    Created = new DateTime(2023, 01, 01),
                    Updated = new DateTime(2023, 01, 02),
                }
            };

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = originalRelease.CreateAmendment(createdDate, createdById);

            var amendmentDataBlock = amendment.DataBlockVersions[0].ContentBlock;
            Assert.Equal(originalDataBlock.Name, amendmentDataBlock.Name);
            Assert.NotEqual(originalDataBlock.Id, amendmentDataBlock.Id);

            var amendmentFeaturedTable = Assert.Single(amendment.FeaturedTables);
            var originalFeaturedTable = originalRelease.FeaturedTables[0];

            amendmentFeaturedTable.AssertDeepEqualTo(originalFeaturedTable, Ignoring<FeaturedTable>(
                f => f.Id,
                f => f.DataBlock,
                f => f.DataBlockId,
                f => f.Release,
                f => f.ReleaseId));

            Assert.NotEqual(Guid.Empty, amendmentFeaturedTable.Id);
            Assert.NotEqual(originalFeaturedTable.Id, amendmentFeaturedTable.Id);

            Assert.Equal(amendmentDataBlock, amendmentFeaturedTable.DataBlock);
            Assert.Equal(amendmentDataBlock.Id, amendmentFeaturedTable.DataBlockId);

            Assert.Equal(amendment, amendmentFeaturedTable.Release);
            Assert.Equal(amendment.Id, amendmentFeaturedTable.ReleaseId);

            // Check original and amendment data blocks/releases are linked to correct featured table
            Assert.Equal(originalRelease.Id, originalFeaturedTable.ReleaseId);
            Assert.Equal(originalDataBlock.Id, originalFeaturedTable.DataBlockId);
            Assert.Equal(amendment.Id, amendmentFeaturedTable.ReleaseId);
            Assert.Equal(amendmentDataBlock.Id, amendmentFeaturedTable.DataBlockId);
        }

        [Fact]
        public void CreateAmendment_FiltersCommentsFromContent()
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

            var originalRelease = _fixture.DefaultRelease().Generate();

            // Test that we are amending content across multiple sections.
            originalRelease.Content = _fixture
                .DefaultContentSection()
                .ForIndex(0, section => section.SetContentBlocks(_fixture
                    .DefaultHtmlBlock()
                    .ForIndex(0, block => block.SetBody(htmlBlock1Body))
                    .ForIndex(1, block => block.SetBody(htmlBlock2Body))
                    .GenerateList()))
                .ForIndex(1, s => s.SetContentBlocks(_fixture
                    .DefaultHtmlBlock()
                    .WithBody(htmlBlock3Body)
                    .Generate(1)))
                .GenerateList();

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            var amendment = originalRelease.CreateAmendment(createdDate, createdById);

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

        [Fact]
        public void CreateAmendment_NullHtmlBlockBody()
        {
            var dataBlockParents = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(() => _fixture
                    .DefaultDataBlockVersion()
                    .Generate())
                .GenerateList(1);

            var originalRelease = _fixture
                .DefaultRelease()
                .WithDataBlockVersions(dataBlockParents
                    .Select(dataBlockParent => dataBlockParent.LatestPublishedVersion!))
                .WithPublished(DateTime.Now.AddDays(-2))
                .WithPublishScheduled(DateTime.Now.AddDays(-1))
                .Generate();

            originalRelease.Content = _fixture
                .DefaultContentSection()
                .WithContentBlocks(_fixture
                    .DefaultHtmlBlock()
                    .WithBody(null!)
                    .Generate(1)
                    .Concat(ListOf(originalRelease.DataBlockVersions[0].ContentBlock as ContentBlock)))
                .GenerateList(1);

            var createdDate = DateTime.Now;
            var createdById = Guid.NewGuid();

            // Minimal test to make sure that a null HtmlBlock body doesn't affect creating a Release amendment.
            var amendment = originalRelease.CreateAmendment(createdDate, createdById);
            Assert.Single(amendment.DataBlockVersions);

            var amendmentHtmlBlock =  Assert.IsType<HtmlBlock>(amendment.Content[0].Content[0]);
            Assert.NotEqual(originalRelease.Content[0].Content[0].Id, amendmentHtmlBlock.Id);
            Assert.Null(amendmentHtmlBlock.Body);
        }
    }
}
