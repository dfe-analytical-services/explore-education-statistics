using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters
{
    public class TimeIdentifierJsonConverterTests
    {
        private class SampleClass
        {
            [JsonConverter(typeof(TimeIdentifierJsonConverter))]
            public TimeIdentifier SampleField { get; set; }
        }

        [Fact]
        public void SerializeObject()
        {
            var objectToSerialize = new SampleClass
            {
                SampleField = TimeIdentifier.AcademicYear
            };
            
            Assert.Equal("{\"SampleField\":{\"value\":\"AY\",\"label\":\"Academic Year\"}}", JsonConvert.SerializeObject(objectToSerialize));
        }
    }
}