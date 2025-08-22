#nullable enable
using FluentValidation.Results;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.ViewModels;

public static class ErrorViewModelTests
{
    public class CreateFromValidationFailureTests
    {
        [Fact]
        public void Success()
        {
            var failure = new ValidationFailure
            {
                ErrorCode = "TestCode",
                ErrorMessage = "Test message",
                PropertyName = "testProperty",
                AttemptedValue = "Test value"
            };

            var error = ErrorViewModel.Create(failure);

            Assert.Equal("TestCode", error.Code);
            Assert.Equal("Test message", error.Message);
            Assert.Equal("testProperty", error.Path);

            var detail = Assert.IsType<Dictionary<string, object>>(error.Detail);

            Assert.Single(detail);
            Assert.Equal("Test value", detail["value"]);
        }

        [Fact]
        public void ValidatorCodeIsTrimmed()
        {
            var failure = new ValidationFailure
            {
                ErrorCode = "TestCodeValidator",
                ErrorMessage = "Test message",
                PropertyName = "testProperty"
            };

            var error = ErrorViewModel.Create(failure);

            Assert.Equal("TestCode", error.Code);
        }
    }
}
