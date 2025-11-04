#nullable enable
using FluentValidation.Results;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class ValidationFailureExtensionsTests
{
    public class GetErrorDetailTests
    {
        [Fact]
        public void AttemptedValueIsNull()
        {
            var failure = new ValidationFailure { AttemptedValue = null };

            var detail = failure.GetErrorDetail();

            Assert.Single(detail);
            Assert.Null(detail["value"]);
        }

        [Fact]
        public void MessagePlaceholdersNotNull()
        {
            var failure = new ValidationFailure
            {
                AttemptedValue = "Test value",
                FormattedMessagePlaceholderValues = new Dictionary<string, object>
                {
                    { "MaxLength", 6 },
                    { "MinLength", 3 },
                },
            };

            var detail = failure.GetErrorDetail();

            Assert.Equal(3, detail.Count);
            Assert.Equal("Test value", detail["value"]);
            Assert.Equal(6, detail["maxLength"]);
            Assert.Equal(3, detail["minLength"]);
        }

        [Fact]
        public void RedundantMessagePlaceholdersIgnored()
        {
            var failure = new ValidationFailure
            {
                AttemptedValue = "Test value",
                FormattedMessagePlaceholderValues = new Dictionary<string, object>
                {
                    { "PropertyName", "test" },
                    { "PropertyPath", "my.test" },
                    { "PropertyValue", "Test value" },
                    { "CollectionIndex", 0 },
                },
            };

            var detail = failure.GetErrorDetail();

            Assert.Single(detail);
            Assert.Equal("Test value", detail["value"]);
        }

        [Fact]
        public void CustomStateIsNull()
        {
            var failure = new ValidationFailure { AttemptedValue = "Test value", CustomState = null };

            var detail = failure.GetErrorDetail();

            Assert.Single(detail);
            Assert.Equal("Test value", detail["value"]);
        }

        [Fact]
        public void CustomStateIsAnonymousObject()
        {
            var failure = new ValidationFailure
            {
                AttemptedValue = "Test value",
                CustomState = new { Name = "Test name", Age = 30 },
            };

            var detail = failure.GetErrorDetail();

            Assert.Equal(3, detail.Count);
            Assert.Equal("Test value", detail["value"]);
            Assert.Equal("Test name", detail["name"]);
            Assert.Equal(30, detail["age"]);
        }

        [Fact]
        public void CustomStateAnonymousObject_WithMessagePlaceholders()
        {
            var failure = new ValidationFailure
            {
                AttemptedValue = "Test value",
                FormattedMessagePlaceholderValues = new Dictionary<string, object>
                {
                    { "MaxLength", 6 },
                    { "MinLength", 3 },
                },
                CustomState = new { Name = "Test name", Age = 30 },
            };

            var detail = failure.GetErrorDetail();

            Assert.Equal(5, detail.Count);
            Assert.Equal("Test value", detail["value"]);
            Assert.Equal("Test name", detail["name"]);
            Assert.Equal(30, detail["age"]);
            Assert.Equal(6, detail["maxLength"]);
            Assert.Equal(3, detail["minLength"]);
        }

        [Fact]
        public void CustomStateIsClass()
        {
            var failure = new ValidationFailure
            {
                AttemptedValue = "Test value",
                CustomState = new TestErrorDetail { Name = "Test name", Age = 30 },
            };

            var detail = failure.GetErrorDetail();

            Assert.Equal(3, detail.Count);
            Assert.Equal("Test value", detail["value"]);
            Assert.Equal("Test name", detail["name"]);
            Assert.Equal(30, detail["age"]);
        }

        [Fact]
        public void CustomStateIsClass_WithMessagePlaceholders()
        {
            var failure = new ValidationFailure
            {
                AttemptedValue = "Test value",
                FormattedMessagePlaceholderValues = new Dictionary<string, object>
                {
                    { "MaxLength", 6 },
                    { "MinLength", 3 },
                },
                CustomState = new TestErrorDetail { Name = "Test name", Age = 30 },
            };

            var detail = failure.GetErrorDetail();

            Assert.Equal(5, detail.Count);
            Assert.Equal("Test value", detail["value"]);
            Assert.Equal("Test name", detail["name"]);
            Assert.Equal(30, detail["age"]);
            Assert.Equal(6, detail["maxLength"]);
            Assert.Equal(3, detail["minLength"]);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(false)]
        [InlineData(TestEnum.Test)]
        public void CustomStateIsPrimitive(object customState)
        {
            var failure = new ValidationFailure { AttemptedValue = "Test value", CustomState = customState };

            var detail = failure.GetErrorDetail();

            Assert.Equal(2, detail.Count);
            Assert.Equal("Test value", detail["value"]);
            Assert.Equal(customState, detail["state"]);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(false)]
        [InlineData(TestEnum.Test)]
        public void CustomStateIsPrimitive_WithMessagePlaceholders(object customState)
        {
            var failure = new ValidationFailure
            {
                AttemptedValue = "Test value",
                FormattedMessagePlaceholderValues = new Dictionary<string, object>
                {
                    { "MaxLength", 6 },
                    { "MinLength", 3 },
                },
                CustomState = customState,
            };

            var detail = failure.GetErrorDetail();

            Assert.Equal(4, detail.Count);
            Assert.Equal("Test value", detail["value"]);
            Assert.Equal(customState, detail["state"]);
            Assert.Equal(6, detail["maxLength"]);
            Assert.Equal(3, detail["minLength"]);
        }

        [Fact]
        public void CustomStateOverridesMessagePlaceholders()
        {
            var failure = new ValidationFailure
            {
                AttemptedValue = "Test value",
                FormattedMessagePlaceholderValues = new Dictionary<string, object>
                {
                    { "MaxLength", 6 },
                    { "MinLength", 3 },
                },
                CustomState = new { MaxLength = 10, MinLength = 5 },
            };

            var detail = failure.GetErrorDetail();

            Assert.Equal(3, detail.Count);
            Assert.Equal("Test value", detail["value"]);
            Assert.Equal(10, detail["maxLength"]);
            Assert.Equal(5, detail["minLength"]);
        }

        [Fact]
        public void CustomStateWithValueOverridesAttemptedValue()
        {
            var failure = new ValidationFailure
            {
                AttemptedValue = "Test value",
                CustomState = new { Value = "Test value override" },
            };

            var detail = failure.GetErrorDetail();

            Assert.Single(detail);
            Assert.Equal("Test value override", detail["value"]);
        }

        private enum TestEnum
        {
            Test,
        }

        private record TestErrorDetail
        {
            public required string Name { get; init; }

            public required int Age { get; init; }
        }
    }
}
