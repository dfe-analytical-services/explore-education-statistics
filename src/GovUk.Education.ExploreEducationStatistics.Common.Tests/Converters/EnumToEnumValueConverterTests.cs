#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters;

public class EnumToEnumValueConverterTests
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum Numbers
    {
        [EnumLabelValue("1", "Value-1")] One,
        [EnumLabelValue("2", "Value-2")] Two,
        [EnumLabelValue("3", "Value-3")] Three
    }

    [Fact]
    public void ConvertToProvider()
    {
        var converter = new EnumToEnumValueConverter<Numbers>();
        var converted = converter.ConvertToProvider.Invoke(Numbers.Two);
        Assert.IsType<string>(converted);
        Assert.Equal("Value-2", converted);
    }

    [Fact]
    public void ConvertFromProvider()
    {
        var converter = new EnumToEnumValueConverter<Numbers>();
        var converted = converter.ConvertFromProvider.Invoke("Value-2");
        Assert.IsType<Numbers>(converted);
        Assert.Equal(Numbers.Two, converted);
    }

    [Fact]
    public void ConvertFromProvider_StoredValueIsNull()
    {
        var converter = new EnumToEnumValueConverter<Numbers>();
        var converted = converter.ConvertFromProvider.Invoke(null);
        Assert.Null(converted);
    }

    [Fact]
    public void ConvertFromProvider_StoredValueIsOutOfRange()
    {
        var converter = new EnumToEnumValueConverter<Numbers>();
        Assert.Throws<ArgumentOutOfRangeException>(() => converter.ConvertFromProvider.Invoke("Value-4"));
    }
}
