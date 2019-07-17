using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters
{
    public class TimeIdentifierCategoryJsonConverterTests
    {
        private class SampleClass
        {
            [JsonConverter(typeof(TimeIdentifierCategoryJsonConverter))]
            public TimeIdentifierCategory SampleField { get; set; }
        }

        [Fact]
        public void SerializeObject()
        {
            var objectToSerialize = new SampleClass
            {
                SampleField = TimeIdentifierCategory.AcademicYear
            };
            
            Assert.Equal("{\"SampleField\":{\"value\":\"AcademicYear\",\"label\":\"Academic year\"}}", JsonConvert.SerializeObject(objectToSerialize));
        }
    }
}