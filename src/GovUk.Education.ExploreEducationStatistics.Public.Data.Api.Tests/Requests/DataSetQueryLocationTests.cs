using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public abstract class DataSetQueryLocationTests
{
    public class ParseTests
    {
        public static readonly TheoryData<string> LocationLevelCodes = new(
            EnumUtil.GetEnumValues<GeographicLevel>()
        );

        [Theory]
        [MemberData(nameof(LocationLevelCodes))]
        public void IdProperty_ReturnsLocationId(string level)
        {
            const string id = "12345";
            var location = IDataSetQueryLocation.Parse($"{level}|id|{id}");

            var locationId = Assert.IsType<DataSetQueryLocationId>(location);

            Assert.Equal(level, locationId.Level);
            Assert.Equal(id, locationId.Id);
        }

        [Theory]
        [InlineData("NAT", "code", typeof(DataSetQueryLocationCode))]
        [InlineData("REG", "code", typeof(DataSetQueryLocationCode))]
        [InlineData("LA", "code", typeof(DataSetQueryLocationLocalAuthorityCode))]
        [InlineData("LA", "oldCode", typeof(DataSetQueryLocationLocalAuthorityOldCode))]
        [InlineData("PROV", "ukprn", typeof(DataSetQueryLocationProviderUkprn))]
        [InlineData("SCH", "laEstab", typeof(DataSetQueryLocationSchoolLaEstab))]
        [InlineData("SCH", "urn", typeof(DataSetQueryLocationSchoolUrn))]
        public void OtherProperty_ReturnsCorrectLocationType(
            string level,
            string property,
            Type expectedType
        )
        {
            const string expectedValue = "12345";

            var location = IDataSetQueryLocation.Parse($"{level}|{property}|{expectedValue}");

            Assert.IsType(expectedType, location);

            var value = location.GetType().GetProperty(property.ToUpperFirst())!.GetValue(location);

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("NATT")]
        [InlineData("LADD")]
        [InlineData("nat")]
        [InlineData("la")]
        [InlineData("0")]
        [InlineData("1")]
        public void InvalidLevel_Throws(string level)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                IDataSetQueryLocation.Parse($"{level}|id|12345")
            );
        }

        [Theory]
        [InlineData("NAT", "invalid")]
        [InlineData("NAT", "laEstab")]
        [InlineData("NAT", "oldCode")]
        [InlineData("NAT", "urn")]
        [InlineData("NAT", "ukprn")]
        [InlineData("LA", "invalid")]
        [InlineData("LA", "laEstab")]
        [InlineData("LA", "urn")]
        [InlineData("LA", "ukprn")]
        [InlineData("PROV", "invalid")]
        [InlineData("PROV", "code")]
        [InlineData("PROV", "oldCode")]
        [InlineData("PROV", "laEstab")]
        [InlineData("PROV", "urn")]
        [InlineData("SCH", "invalid")]
        [InlineData("SCH", "code")]
        [InlineData("SCH", "oldCode")]
        [InlineData("SCH", "ukprn")]
        public void InvalidProperty_Throws(string level, string property)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                IDataSetQueryLocation.Parse($"{level}|{property}|12345")
            );
        }

        public static readonly TheoryData<Type> LocationTypes = new(
            Assembly
                .GetAssembly(typeof(IDataSetQueryLocation))!
                .GetTypes()
                .Where(type => type != typeof(IDataSetQueryLocation))
                .Where(type => typeof(IDataSetQueryLocation).IsAssignableFrom(type))
        );

        [Theory]
        [MemberData(nameof(LocationTypes))]
        public void AllTypesCanBeParsed(Type locationType)
        {
            string[] locationStrings =
            [
                "NAT|id|12345",
                "REG|code|12345",
                "LA|code|12345",
                "LA|oldCode|12345",
                "PROV|ukprn|12345",
                "SCH|laEstab|12345",
                "SCH|urn|12345",
            ];

            var parsedLocationTypes = locationStrings
                .Select(IDataSetQueryLocation.Parse)
                .Select(l => l.GetType())
                .ToHashSet();

            Assert.True(
                parsedLocationTypes.Contains(locationType),
                $"""
                The location type could not be parsed. Ensure that:

                - There is a branch for this type in '{nameof(IDataSetQueryLocation)}.{nameof(
                    IDataSetQueryLocation.Parse
                )}'
                - There is a corresponding string for this in the '{nameof(
                    locationStrings
                )}' variable
                """
            );
        }
    }
}
