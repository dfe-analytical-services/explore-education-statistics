using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class PartialDateTest
    {
        [Fact]
        public void TestAcceptablePartialDates()
        {
            Assert.True(new PartialDate {Year = "2019", Month = "1", Day = "1"}.IsValid());
            Assert.True(new PartialDate {Year = "2019", Month = "01", Day = "01"}.IsValid());
            
            Assert.True(new PartialDate {Year = "", Month = "1", Day = "1"}.IsValid());
            Assert.True(new PartialDate {Year = null, Month = "1", Day = "1"}.IsValid());
            Assert.True(new PartialDate {Year = "2019", Month = "", Day = "1"}.IsValid());
            Assert.True(new PartialDate {Year = "2019", Month = null, Day = "1"}.IsValid());
            Assert.True(new PartialDate {Year = "2019", Month = "1", Day = ""}.IsValid());
            Assert.True(new PartialDate {Year = "2019", Month = "1", Day = null}.IsValid());
            
            Assert.True(new PartialDate {Year = "2019", Month = "", Day = ""}.IsValid());
            Assert.True(new PartialDate {Year = "2019", Month = null, Day = null}.IsValid());
            Assert.True(new PartialDate {Year = "", Month = "1", Day = ""}.IsValid());
            Assert.True(new PartialDate {Year = null, Month = "1", Day = null}.IsValid());
            Assert.True(new PartialDate {Year = "", Month = "", Day = "1"}.IsValid());
            Assert.True(new PartialDate {Year = null, Month = null, Day = "1"}.IsValid());
            
            Assert.True(new PartialDate {Year = "", Month = "", Day = ""}.IsValid());
            Assert.True(new PartialDate {Year = null, Month = null, Day = null}.IsValid());
            
            Assert.True(new PartialDate {Year = "", Month = "2", Day = "29"}.IsValid());
            Assert.True(new PartialDate {Year = "2016", Month = "2", Day = "29"}.IsValid()); // Leap year
        }
        
        [Fact]
        public void TestUnacceptablePartialDates()
        {
            Assert.False(new PartialDate {Year = "Hello", Month = "1", Day = "1"}.IsValid());
            Assert.False(new PartialDate {Year = "2019", Month = "Hello", Day = "1"}.IsValid());
            Assert.False(new PartialDate {Year = "2019", Month = "1", Day = "Hello"}.IsValid());
            
            Assert.False(new PartialDate {Year = "", Month = "13", Day = "1"}.IsValid());
            Assert.False(new PartialDate {Year = "", Month = "0", Day = "1"}.IsValid());
            Assert.False(new PartialDate {Year = "", Month = "1", Day = "32"}.IsValid());
            Assert.False(new PartialDate {Year = "", Month = "1", Day = "0"}.IsValid());
            
            Assert.False(new PartialDate {Year = "2017", Month = "2", Day = "29"}.IsValid()); // Not a leap year
        }
        
    }
}