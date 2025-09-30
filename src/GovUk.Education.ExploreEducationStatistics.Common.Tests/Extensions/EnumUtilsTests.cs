#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
            Assert.Equal(
                TestEnum.WithLabelValue,
                EnumUtil.GetFromEnumValue<TestEnum>("with-label-value")
            );
        }

        [Fact]
        public void WithLabelValue_UsingLabel_Invalid()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                EnumUtil.GetFromEnumValue<TestEnum>("With label value")
            );

            Assert.StartsWith(
                $"The value 'With label value' is not a valid {nameof(TestEnum)}",
                exception.Message
            );
        }

        [Fact]
        public void NoMatch_Invalid()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                EnumUtil.GetFromEnumValue<TestEnum>("Invalid label")
            );

            Assert.StartsWith(
                $"The value 'Invalid label' is not a valid {nameof(TestEnum)}",
                exception.Message
            );
        }
    }

    public class TryGetFromEnumValueTests
    {
        [Fact]
        public void WithLabelValue_Success()
        {
            Assert.True(EnumUtil.TryGetFromEnumValue<TestEnum>("with-label-value", out var @enum));
            Assert.Equal(TestEnum.WithLabelValue, @enum);
        }

        [Fact]
        public void NoMatch_Invalid()
        {
            Assert.False(EnumUtil.TryGetFromEnumValue<TestEnum>("Invalid", out _));
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
            Assert.Equal(
                TestEnum.WithLabelValue,
                EnumUtil.GetFromEnumLabel<TestEnum>("With label value")
            );
        }

        [Fact]
        public void WithLabelValue_UsingValue_Invalid()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                EnumUtil.GetFromEnumLabel<TestEnum>("with-label-value")
            );

            Assert.StartsWith(
                $"The label 'with-label-value' is not a valid {nameof(TestEnum)}",
                exception.Message
            );
        }

        [Fact]
        public void NoMatch_Invalid()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                EnumUtil.GetFromEnumLabel<TestEnum>("Invalid label")
            );

            Assert.StartsWith(
                $"The label 'Invalid label' is not a valid {nameof(TestEnum)}",
                exception.Message
            );
        }
    }

    public class TryGetFromEnumLabelTests
    {
        [Fact]
        public void WithLabel_Success()
        {
            Assert.True(EnumUtil.TryGetFromEnumLabel<TestEnum>("With label", out var @enum));
            Assert.Equal(TestEnum.WithLabel, @enum);
        }

        [Fact]
        public void WithLabelValue_Success()
        {
            Assert.True(EnumUtil.TryGetFromEnumLabel<TestEnum>("With label value", out var @enum));
            Assert.Equal(TestEnum.WithLabelValue, @enum);
        }

        [Fact]
        public void NoMatch_Invalid()
        {
            Assert.False(EnumUtil.TryGetFromEnumLabel<TestEnum>("Invalid", out _));
        }
    }

    public class GetEnumsTests
    {
        [Fact]
        public void Success()
        {
            var expected = new List<TestEnum> { TestEnum.WithLabel, TestEnum.WithLabelValue };

            Assert.Equal(expected, EnumUtil.GetEnums<TestEnum>());
        }
    }

    public class GetEnumsArrayTests
    {
        [Fact]
        public void Success()
        {
            var expected = new[] { TestEnum.WithLabel, TestEnum.WithLabelValue };

            Assert.Equal(expected, EnumUtil.GetEnumsArray<TestEnum>());
        }
    }

    public class GetEnumLabelsTests
    {
        [Fact]
        public void Success()
        {
            var expected = new List<string>
            {
                TestEnum.WithLabel.GetEnumLabel(),
                TestEnum.WithLabelValue.GetEnumLabel(),
            };

            Assert.Equal(expected, EnumUtil.GetEnumLabels<TestEnum>());
        }
    }

    public class GetEnumLabelsSetTests
    {
        [Fact]
        public void Success()
        {
            var expected = new HashSet<string>
            {
                TestEnum.WithLabel.GetEnumLabel(),
                TestEnum.WithLabelValue.GetEnumLabel(),
            };

            Assert.Equal(expected, EnumUtil.GetEnumLabelsSet<TestEnum>());
        }
    }

    public class GetEnumValuesTests
    {
        [Fact]
        public void Success()
        {
            var expected = new List<string>
            {
                TestEnum.WithLabel.ToString(),
                TestEnum.WithLabelValue.GetEnumValue(),
            };

            Assert.Equal(expected, EnumUtil.GetEnumValues<TestEnum>());
        }
    }

    public class GetEnumValuesSetTests
    {
        [Fact]
        public void Success()
        {
            var expected = new HashSet<string>
            {
                TestEnum.WithLabel.ToString(),
                TestEnum.WithLabelValue.GetEnumValue(),
            };

            Assert.Equal(expected, EnumUtil.GetEnumValuesSet<TestEnum>());
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
