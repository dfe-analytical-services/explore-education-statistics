#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class EnumUtilsTests
{
    public class GetFromEnumValueTests
    {
        [Fact]
        public void WithLabelValue_Success()
        {
            Assert.Equal(TestEnum.WithLabelValue, EnumUtil.GetFromEnumValue<TestEnum>("with-label-value"));
        }

        [Fact]
        public void WithLabelValue_UsingLabel_Invalid()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => EnumUtil.GetFromEnumValue<TestEnum>("With label value"));

            Assert.Equal(
                $"The value 'With label value' is not a valid {nameof(TestEnum)}",
                exception.Message);
        }

        [Fact]
        public void NoMatch_Invalid()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => EnumUtil.GetFromEnumValue<TestEnum>("Invalid label"));

            Assert.Equal(
                $"The value 'Invalid label' is not a valid {nameof(TestEnum)}",
                exception.Message);
        }
    }

    public class GetFromEnumLabelTests
    {
        [Fact]
        public void WithLabel_Success()
        {
            Assert.Equal(TestEnum.WithLabel, EnumUtil.GetFromEnumLabel<TestEnum>("With label"));
        }

        [Fact]
        public void WithLabelValue_Success()
        {
            Assert.Equal(TestEnum.WithLabelValue, EnumUtil.GetFromEnumLabel<TestEnum>("With label value"));
        }

        [Fact]
        public void WithLabelValue_UsingValue_Invalid()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => EnumUtil.GetFromEnumLabel<TestEnum>("with-label-value"));

            Assert.Equal(
                $"The label 'with-label-value' is not a valid {nameof(TestEnum)}",
                exception.Message);
        }

        [Fact]
        public void NoMatch_Invalid()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => EnumUtil.GetFromEnumLabel<TestEnum>("Invalid label"));

            Assert.Equal(
                $"The label 'Invalid label' is not a valid {nameof(TestEnum)}",
                exception.Message);
        }
    }

    public class GetEnumValuesTests
    {
        [Fact]
        public void Success()
        {
            var expected = new List<TestEnum>
            {
                TestEnum.WithLabel,
                TestEnum.WithLabelValue
            };

            Assert.Equal(expected, EnumUtil.GetEnumValues<TestEnum>());
        }
    }

    public class GetEnumValuesAsArrayTests
    {
        [Fact]
        public void Success()
        {
            var expected = new []
            {
                TestEnum.WithLabel,
                TestEnum.WithLabelValue
            };

            Assert.Equal(expected, EnumUtil.GetEnumValuesAsArray<TestEnum>());
        }
    }

    private enum TestEnum
    {
        [EnumLabelValue("With label")]
        WithLabel,

        [EnumLabelValue("With label value", "with-label-value")]
        WithLabelValue,
    }
}