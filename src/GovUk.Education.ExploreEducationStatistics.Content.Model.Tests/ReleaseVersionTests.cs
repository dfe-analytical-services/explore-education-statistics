using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests;

public abstract class ReleaseVersionTests
{
    public class LiveTests : ReleaseVersionTests
    {
        [Fact]
        public void WhenPublishedDateInThePast_ReturnsTrue()
        {
            var releaseVersion = new ReleaseVersion { Published = DateTimeOffset.UtcNow.AddDays(-1) };

            Assert.True(releaseVersion.Live);
        }

        [Fact]
        public void WhenNotPublished_ReturnsFalse()
        {
            var releaseVersion = new ReleaseVersion { Published = null };

            Assert.False(releaseVersion.Live);
        }

        [Fact]
        public void WhenPublishedDateInTheFuture_ReturnsTrue()
        {
            // Note that this should not happen, but we test the edge case.
            var releaseVersion = new ReleaseVersion { Published = DateTimeOffset.UtcNow.AddDays(1) };

            Assert.False(releaseVersion.Live);
        }
    }

    public class NextReleaseDateTests : ReleaseVersionTests
    {
        [Fact]
        public void WhenSettingNextReleaseDateWithValidValue_SetsValueSuccessfully()
        {
            var releaseVersion = new ReleaseVersion
            {
                NextReleaseDate = new PartialDate { Year = "2021", Month = "1" },
            };

            Assert.Equal(new PartialDate { Year = "2021", Month = "1" }, releaseVersion.NextReleaseDate);
        }

        [Fact]
        public void WhenSettingNextReleaseDateWithInvalidValue_ThrowsException()
        {
            Assert.Throws<FormatException>(() =>
                new ReleaseVersion { NextReleaseDate = new PartialDate { Day = "45" } }
            );
        }
    }

    public class GenericContentTests : ReleaseVersionTests
    {
        [Fact]
        public void WhenContentContainsGenericAndNonGenericSections_GenericContentOnlyReturnsGenericSections()
        {
            var genericSection1 = new ContentSection { Type = ContentSectionType.Generic };
            var genericSection2 = new ContentSection { Type = ContentSectionType.Generic };
            var nonGenericSection = new ContentSection { Type = ContentSectionType.Headlines };

            var releaseVersion = new ReleaseVersion { Content = [genericSection1, nonGenericSection, genericSection2] };

            var actualGenericContent = releaseVersion.GenericContent.ToList();

            Assert.Equal(2, actualGenericContent.Count);
            Assert.All(actualGenericContent, section => Assert.Equal(ContentSectionType.Generic, section.Type));
            Assert.Contains(genericSection1, actualGenericContent);
            Assert.Contains(genericSection2, actualGenericContent);
            Assert.DoesNotContain(nonGenericSection, actualGenericContent);
        }

        [Fact]
        public void WhenContentContainsNoGenericSections_GenericContentReturnsEmptyCollection()
        {
            var nonGenericSection1 = new ContentSection { Type = ContentSectionType.Headlines };
            var nonGenericSection2 = new ContentSection { Type = ContentSectionType.Warning };

            var releaseVersion = new ReleaseVersion { Content = [nonGenericSection1, nonGenericSection2] };

            Assert.Empty(releaseVersion.GenericContent);
        }

        [Fact]
        public void WhenContentContainsNoSections_GenericContentReturnsEmptyCollection()
        {
            var releaseVersion = new ReleaseVersion { Content = [] };

            Assert.Empty(releaseVersion.GenericContent);
        }

        [Fact]
        public void WhenSettingGenericContent_ReplacesExistingGenericSectionsInContent()
        {
            var existingGenericSection1 = new ContentSection { Type = ContentSectionType.Generic };
            var existingGenericSection2 = new ContentSection { Type = ContentSectionType.Generic };
            var nonGenericSection = new ContentSection { Type = ContentSectionType.Headlines };

            var releaseVersion = new ReleaseVersion
            {
                Content = [existingGenericSection1, existingGenericSection2, nonGenericSection],
            };

            var newGenericSection1 = new ContentSection { Type = ContentSectionType.Generic };
            var newGenericSection2 = new ContentSection { Type = ContentSectionType.Generic };

            releaseVersion.GenericContent = [newGenericSection1, newGenericSection2];

            Assert.Equal(3, releaseVersion.Content.Count);
            Assert.Contains(nonGenericSection, releaseVersion.Content);
            Assert.Contains(newGenericSection1, releaseVersion.Content);
            Assert.Contains(newGenericSection2, releaseVersion.Content);
            Assert.DoesNotContain(existingGenericSection1, releaseVersion.Content);
            Assert.DoesNotContain(existingGenericSection2, releaseVersion.Content);
        }

        [Fact]
        public void WhenSettingGenericContentWithEmptyCollection_RemovesExistingGenericSectionsFromContent()
        {
            var existingGenericSection1 = new ContentSection { Type = ContentSectionType.Generic };
            var existingGenericSection2 = new ContentSection { Type = ContentSectionType.Generic };
            var nonGenericSection = new ContentSection { Type = ContentSectionType.Headlines };

            var releaseVersion = new ReleaseVersion
            {
                Content = [existingGenericSection1, existingGenericSection2, nonGenericSection],
            };

            ContentSection[] newGenericContent = [];
            releaseVersion.GenericContent = newGenericContent;

            Assert.Single(releaseVersion.Content);
            Assert.Contains(nonGenericSection, releaseVersion.Content);
            Assert.DoesNotContain(existingGenericSection1, releaseVersion.Content);
            Assert.DoesNotContain(existingGenericSection2, releaseVersion.Content);
        }

        [Fact]
        public void WhenSettingGenericContentWithNonGenericSection_ThrowsException()
        {
            var releaseVersion = new ReleaseVersion { Content = [] };

            var newGenericSection = new ContentSection { Type = ContentSectionType.Generic };
            var nonGenericSection = new ContentSection { Type = ContentSectionType.Headlines };

            var message = Assert.Throws<InvalidOperationException>(() =>
                releaseVersion.GenericContent = [newGenericSection, nonGenericSection]
            );

            Assert.Equal($"All content sections must be of type {ContentSectionType.Generic}.", message.Message);
        }
    }

    public class HeadlinesSectionTests : ReleaseVersionTests
    {
        // Only HeadlinesSection is tested here. Tests for the other single content section properties
        // (e.g. Warning) are omitted for brevity, as they follow the same pattern as HeadlinesSection
        // and differ only in the ContentSectionType they target.

        [Fact]
        public void WhenContentContainsHeadlinesSection_HeadlinesReturnsThatSection()
        {
            var headlinesSection = new ContentSection { Type = ContentSectionType.Headlines };
            var otherSection1 = new ContentSection { Type = ContentSectionType.Generic };
            var otherSection2 = new ContentSection { Type = ContentSectionType.KeyStatisticsSecondary };

            var releaseVersion = new ReleaseVersion { Content = [otherSection1, headlinesSection, otherSection2] };

            Assert.Equal(headlinesSection, releaseVersion.HeadlinesSection);
        }

        [Fact]
        public void WhenContentContainsNoHeadlinesSection_HeadlinesSectionReturnsNull()
        {
            var otherSection1 = new ContentSection { Type = ContentSectionType.Generic };
            var otherSection2 = new ContentSection { Type = ContentSectionType.KeyStatisticsSecondary };

            var releaseVersion = new ReleaseVersion { Content = [otherSection1, otherSection2] };

            Assert.Null(releaseVersion.HeadlinesSection);
        }

        [Fact]
        public void WhenSettingHeadlinesSection_ReplacesExistingSectionOfSameTypeInContent()
        {
            var existingHeadlinesSection = new ContentSection { Type = ContentSectionType.Headlines };

            var releaseVersion = new ReleaseVersion { Content = [existingHeadlinesSection] };

            var newHeadlinesSection = new ContentSection { Type = ContentSectionType.Headlines };
            releaseVersion.HeadlinesSection = newHeadlinesSection;

            Assert.Single(releaseVersion.Content);
            Assert.Contains(newHeadlinesSection, releaseVersion.Content);
            Assert.DoesNotContain(existingHeadlinesSection, releaseVersion.Content);
        }

        [Fact]
        public void WhenSettingHeadlinesSectionWithNull_RemovesExistingSectionOfSameTypeFromContent()
        {
            var existingHeadlinesSection = new ContentSection { Type = ContentSectionType.Headlines };
            var otherSection1 = new ContentSection { Type = ContentSectionType.Generic };
            var otherSection2 = new ContentSection { Type = ContentSectionType.KeyStatisticsSecondary };

            var releaseVersion = new ReleaseVersion
            {
                Content = [otherSection1, existingHeadlinesSection, otherSection2],
            };

            ContentSection? newHeadlinesSection = null;
            releaseVersion.HeadlinesSection = newHeadlinesSection;

            Assert.Equal(2, releaseVersion.Content.Count);
            Assert.Contains(otherSection1, releaseVersion.Content);
            Assert.Contains(otherSection2, releaseVersion.Content);
            Assert.DoesNotContain(existingHeadlinesSection, releaseVersion.Content);
        }

        [Fact]
        public void WhenSettingHeadlinesSectionWithNonMatchingType_ThrowsException()
        {
            var releaseVersion = new ReleaseVersion { Content = [] };

            var newSection = new ContentSection { Type = ContentSectionType.Generic };

            var message = Assert.Throws<InvalidOperationException>(() => releaseVersion.HeadlinesSection = newSection);

            Assert.Equal(
                $"The replacement content section must be of type {ContentSectionType.Headlines}.",
                message.Message
            );
        }
    }
}
