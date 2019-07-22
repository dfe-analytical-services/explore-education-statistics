using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class PartialDateUtilTest
    {
        [Fact]
        public void TestAcceptablePartialDates()
        {
            
            Assert.Equal("2019", PartialDateUtil.PartialDateValid2("2019-12-01").Year);
            Assert.Equal("12", PartialDateUtil.PartialDateValid2("2019-12-01").Month);
            Assert.Equal("01", PartialDateUtil.PartialDateValid2("2019-12-01").Day);
            Assert.Equal("", PartialDateUtil.PartialDateValid2("-12-01").Year);
//            Assert.True(PartialDateUtil.PartialDateValid("-12-01"));
//            Assert.True(PartialDateUtil.PartialDateValid("2019--01"));
//            Assert.True(PartialDateUtil.PartialDateValid("2019-12-"));
//            Assert.True(PartialDateUtil.PartialDateValid("2019--"));
//            Assert.True(PartialDateUtil.PartialDateValid("-12-"));
//            Assert.True(PartialDateUtil.PartialDateValid("--01"));
//            Assert.True(PartialDateUtil.PartialDateValid("--"));
        }
        
    }
}