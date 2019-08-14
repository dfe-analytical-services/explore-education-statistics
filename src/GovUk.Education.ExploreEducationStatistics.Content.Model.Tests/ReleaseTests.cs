using System;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class ReleaseTests
    {
        [Fact]
        public void Release_Not_Live_Published_Null()
        {
            var releaseNoPublishedDate = new Release {Published = null};
            Assert.False(releaseNoPublishedDate.Live);
        }

        [Fact]
        public void Release_Not_Live_Published_In_Future()
        {
            // Note that this should not happen, but we test the edge case.
            var releasePublishedDateInFuture = new Release {Published = DateTime.Now.AddDays(1)};
            Assert.False(releasePublishedDateInFuture.Live);
        }


        [Fact]
        public void Release_Live()
        {
            var releasePublished = new Release {Published = DateTime.Now.AddDays(-1)};
            Assert.True(releasePublished.Live);
        }

        [Fact]
        public void Release_Name_Ok()
        {
            new Release {ReleaseName = "2014"}; // Should not throw
        }


        [Fact]
        public void Release_Name_Not_Ok()
        {
            try
            {
                new Release {ReleaseName = "Hello"}; // Not Ok
                Assert.True(false, "Should have failed validation");
            }
            catch (FormatException e)
            {
            }
        }

        [Fact]
        public void Release_NextReleaseDate_Ok()
        {
            new Release {NextReleaseDate = new PartialDate {Day = "01"}}; // Should not throw
        }

        [Fact]
        public void Release_NextReleaseDate_Not_Ok()
        {
            try
            {
                new Release {NextReleaseDate = new PartialDate {Day = "45"}}; // Not Ok
                Assert.True(false, "Should have failed validation");
            }
            catch (FormatException e)
            {
            }
        }

        [Fact]
        public void Acceptable_ReleaseName()
        {
            new Release {ReleaseName = "1990"};
            new Release {ReleaseName = "2011"};
            new Release {ReleaseName = "3000"};
            new Release {ReleaseName = ""}; // considered not set
            new Release {ReleaseName = null}; // considered not set
            // None should throw
        }
        
        [Fact]
        public void Unacceptable_ReleaseName()
        {
            try
            {
                new Release {ReleaseName = "190"};
                Assert.True(false, "190 is not a year we are interested in");
            }
            catch (FormatException e)
            {
            }
            
            try
            {
                new Release {ReleaseName = "ABC123"};
                Assert.True(false, "Not a year");
            }
            catch (FormatException e)
            {
            }

            try
            {
                new Release {ReleaseName = "20000"};
                Assert.True(false, "20000 is not a year we are interested in");
            }
            catch (FormatException e)
            {
            }

            new Release {ReleaseName = "2011"};
            new Release {ReleaseName = "3000"};
            // None should throw
        }
        
        
        
        [Fact]
        public void Title_CalendarYear()
        {
            var title = new Release {ReleaseName = "2019", TimePeriodCoverage = CalendarYear}.Title;
            Assert.Equal("Calendar Year 2019", title);
        }
        
        [Fact]
        public void Title_NonCalendarYear()
        {
            var title1 = new Release { ReleaseName = "2019", TimePeriodCoverage = TaxYear}.Title;
            Assert.Equal("Tax Year 2019/20", title1);
            
            var title2 = new Release {ReleaseName = "2010", TimePeriodCoverage = TaxYear}.Title;
            Assert.Equal("Tax Year 2010/11", title2);
        }

        [Fact]
        public void Title_NoYear()
        {
            var title1 = new Release {ReleaseName = "", TimePeriodCoverage = TaxYear}.Title;
            Assert.Equal("Tax Year", title1);
            
            var title2 = new Release {ReleaseName = null, TimePeriodCoverage = TaxYear}.Title;
            Assert.Equal("Tax Year", title2);
        }
        
        [Fact]
        public void CoverageTitle()
        {
            var coverageTitle = new Release {TimePeriodCoverage = TaxYear}.CoverageTitle;
            Assert.Equal("Tax Year", coverageTitle);
        }
        
        [Fact]
        public void ReleaseYearTitle_CalendarYear()
        {
            var yearTitle = new Release {ReleaseName = "2019", TimePeriodCoverage = CalendarYear}.YearTitle;
            Assert.Equal("2019", yearTitle );
        }
        
        [Fact]
        public void ReleaseYearTitle_NonCalendarYear()
        {
            var yearTitle = new Release {ReleaseName = "2019", TimePeriodCoverage = TaxYear}.YearTitle;
            Assert.Equal("2019/20", yearTitle);
        }
    }
}