#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Converters
{
    public class PermalinkResultSubjectMetaJsonConverterTests
    {
        [Fact]
        public void ReadJson_LegacyLocationsFieldIsTransformed()
        {
            const string jsonText = @"
            {
                ""Locations"": [
                {
                    ""Level"": ""localAuthority"",
                    ""GeoJson"": [{ ""properties"": { ""code"": ""E06000009"" } }],
                    ""Label"": ""Blackpool"",
                    ""Value"": ""E06000009""
                },
                {
                    ""Level"": ""localAuthority"",
                    ""GeoJson"": [{ ""properties"": { ""code"": ""E06000015"" } }],
                    ""Label"": ""Derby"",
                    ""Value"": ""E06000015""
                },
                {
                    ""Level"": ""localAuthority"",
                    ""GeoJson"": [{ ""properties"": { ""code"": ""E06000018"" } }],
                    ""Label"": ""Nottingham"",
                    ""Value"": ""E06000018""
                }
                ]
            }";

            var subjectMeta = JsonConvert.DeserializeObject<PermalinkResultSubjectMeta>(jsonText, BuildSettings());

            // Expect Locations to have been transformed to LocationsHierarchical
            Assert.NotNull(subjectMeta);
            Assert.Single(subjectMeta!.LocationsHierarchical);
            Assert.True(subjectMeta.LocationsHierarchical.ContainsKey("localAuthority"));

            var localAuthorities = subjectMeta.LocationsHierarchical["localAuthority"];
            Assert.Equal(3, localAuthorities.Count);

            Assert.Equal("Blackpool", localAuthorities[0].Label);
            Assert.Equal("E06000009", localAuthorities[0].Value);
            Assert.NotNull(localAuthorities[0].GeoJson);

            Assert.Equal("Derby", localAuthorities[1].Label);
            Assert.Equal("E06000015", localAuthorities[1].Value);
            Assert.NotNull(localAuthorities[1].GeoJson);

            Assert.Equal("Nottingham", localAuthorities[2].Label);
            Assert.Equal("E06000018", localAuthorities[2].Value);
            Assert.NotNull(localAuthorities[2].GeoJson);
        }

        [Fact]
        public void ReadJson_LocationsHierarchicalIsUntouched()
        {
            const string jsonText = @"
            {
                ""LocationsHierarchical"": {
                    ""country"": [
                    {
                        ""GeoJson"": [{ ""properties"": { ""code"": ""E92000001"" } }],
                        ""Label"": ""England"",
                        ""Value"": ""E92000001""
                    }
                    ]
                }
            }";

            var subjectMeta = JsonConvert.DeserializeObject<PermalinkResultSubjectMeta>(jsonText, BuildSettings());

            Assert.Single(subjectMeta!.LocationsHierarchical);
            Assert.True(subjectMeta.LocationsHierarchical.ContainsKey("country"));

            var countries = subjectMeta.LocationsHierarchical["country"];
            Assert.Single(countries);

            Assert.Equal("England", countries[0].Label);
            Assert.Equal("E92000001", countries[0].Value);
            Assert.NotNull(countries[0].GeoJson);
        }

        [Fact]
        public void ReadJson_LocationsHierarchicalTakesPrecedenceOverLocations()
        {
            // Should never happen but presence of elements in 'LocationsHierarchical' means 'Locations' is ignored  

            const string jsonText = @"
            {
                ""Locations"": [
                {
                    ""Level"": ""localAuthority"",
                    ""GeoJson"": [{ ""properties"": { ""code"": ""E06000009"" } }],
                    ""Label"": ""Blackpool"",
                    ""Value"": ""E06000009""
                }
                ],
                ""LocationsHierarchical"": {
                    ""country"": [
                    {
                        ""GeoJson"": [{ ""properties"": { ""code"": ""E92000001"" } }],
                        ""Label"": ""England"",
                        ""Value"": ""E92000001""
                    }
                    ]
                }
            }";

            var subjectMeta = JsonConvert.DeserializeObject<PermalinkResultSubjectMeta>(jsonText, BuildSettings());

            Assert.NotNull(subjectMeta);
            Assert.Single(subjectMeta!.LocationsHierarchical);
            Assert.True(subjectMeta.LocationsHierarchical.ContainsKey("country"));

            var countries = subjectMeta.LocationsHierarchical["country"];
            Assert.Single(countries);

            Assert.Equal("England", countries[0].Label);
            Assert.Equal("E92000001", countries[0].Value);
            Assert.NotNull(countries[0].GeoJson);
        }

        [Fact]
        public void ReadJson_LegacyLocationsFieldsIsEmpty()
        {
            const string jsonText = @"{""Locations"":[]}";

            var subjectMeta = JsonConvert.DeserializeObject<PermalinkResultSubjectMeta>(jsonText, BuildSettings());

            Assert.Empty(subjectMeta!.LocationsHierarchical);
        }

        [Fact]
        public void ReadJson_LegacyLocationsFieldsIsNull()
        {
            const string jsonText = @"{""Locations"":null}";

            var subjectMeta = JsonConvert.DeserializeObject<PermalinkResultSubjectMeta>(jsonText, BuildSettings());

            Assert.Empty(subjectMeta!.LocationsHierarchical);
        }

        [Fact]
        public void ReadJson_NoLocationsFields()
        {
            const string jsonText = "{}";

            var subjectMeta = JsonConvert.DeserializeObject<PermalinkResultSubjectMeta>(jsonText, BuildSettings());

            Assert.Empty(subjectMeta!.LocationsHierarchical);
        }

        private static JsonSerializerSettings BuildSettings()
        {
            return new()
            {
                Converters = new List<JsonConverter>
                {
                    new PermalinkResultSubjectMetaJsonConverter()
                }
            };
        }
    }
}
