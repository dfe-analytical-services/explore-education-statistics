#nullable enable
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters;

public abstract class EnumToEnumValueListConverterTests
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum Numbers
    {
        [EnumLabelValue("1", "Value-1")]
        One,

        [EnumLabelValue("2", "Value-2")]
        Two,

        [EnumLabelValue("3", "Value-3")]
        Three,
    }

    private readonly EnumToEnumValueListConverter<Numbers> _converter = new();

    public class ConvertToProviderTests : EnumToEnumValueListConverterTests
    {
        [Fact]
        public void Success()
        {
            var converted = _converter.ConvertToProviderTyped([Numbers.Two, Numbers.Three]);

            Assert.Equal(2, converted.Count);
            Assert.Equal("Value-2", converted[0]);
            Assert.Equal("Value-3", converted[1]);
        }
    }

    public class ConvertFromProviderTests : EnumToEnumValueListConverterTests
    {
        [Fact]
        public void Success()
        {
            var converted = _converter.ConvertFromProviderTyped(["Value-1", "Value-2"]);

            Assert.Equal(2, converted.Count);
            Assert.Equal(Numbers.One, converted[0]);
            Assert.Equal(Numbers.Two, converted[1]);
        }
    }
}
