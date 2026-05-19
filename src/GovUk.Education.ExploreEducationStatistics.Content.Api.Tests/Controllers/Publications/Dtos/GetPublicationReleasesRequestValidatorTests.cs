using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications.Dtos;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.Publications.Dtos;

public abstract class GetPublicationReleasesRequestValidatorTests
{
    private readonly GetPublicationReleasesRequest.Validator _validator = new();

    private const string PublicationSlug = "test-publication";

    public class PublicationSlugTests : GetPublicationReleasesRequestValidatorTests
    {
        [Fact]
        public void WhenPublicationSlugIsNotEmpty_PublicationSlugIsValid()
        {
            // Arrange
            var request = new GetPublicationReleasesRequest
            {
                PublicationSlug = PublicationSlug,
                Page = 1,
                PageSize = 10,
            };

            // Act & Assert
            _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void WhenPublicationSlugIsEmpty_PublicationSlugHasValidationError(string publicationSlug)
        {
            // Arrange
            var request = new GetPublicationReleasesRequest
            {
                PublicationSlug = publicationSlug,
                Page = 1,
                PageSize = 10,
            };

            // Act & Assert
            _validator.TestValidate(request).ShouldHaveValidationErrorFor(r => r.PublicationSlug).Only();
        }
    }

    public class PaginationTests : GetPublicationReleasesRequestValidatorTests
    {
        [Fact]
        public void WhenPageAndPageSizeAreBothNull_PageAndPageSizeAreValid()
        {
            // Arrange
            var request = new GetPublicationReleasesRequest
            {
                PublicationSlug = PublicationSlug,
                Page = null,
                PageSize = null,
            };

            // Act & Assert
            _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void WhenPageIsGreaterThanOrEqualToOne_PageIsValid(int page)
        {
            // Arrange
            var request = new GetPublicationReleasesRequest
            {
                PublicationSlug = PublicationSlug,
                Page = page,
                PageSize = 10,
            };

            // Act & Assert
            _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void WhenPageIsLessThanOne_PageHasValidationError(int page)
        {
            // Arrange
            var request = new GetPublicationReleasesRequest
            {
                PublicationSlug = PublicationSlug,
                Page = page,
                PageSize = 10,
            };

            // Act & Assert
            _validator
                .TestValidate(request)
                .ShouldHaveValidationErrorFor(r => r.Page)
                .WithErrorCode(FluentValidationKeys.GreaterThanOrEqualValidator)
                .Only();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void WhenPageSizeIsWithinRange_PageSizeIsValid(int pageSize)
        {
            // Arrange
            var request = new GetPublicationReleasesRequest
            {
                PublicationSlug = PublicationSlug,
                Page = 1,
                PageSize = pageSize,
            };

            // Act & Assert
            _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(101)]
        public void WhenPageSizeIsOutOfRange_PageSizeHasValidationError(int pageSize)
        {
            // Arrange
            var request = new GetPublicationReleasesRequest
            {
                PublicationSlug = PublicationSlug,
                Page = 1,
                PageSize = pageSize,
            };

            // Act & Assert
            _validator
                .TestValidate(request)
                .ShouldHaveValidationErrorFor(r => r.PageSize)
                .WithErrorCode(FluentValidationKeys.InclusiveBetweenValidator)
                .Only();
        }

        [Fact]
        public void WhenPageIsNotNullAndPageSizeIsNull_PageSizeHasValidationError()
        {
            // Arrange
            var request = new GetPublicationReleasesRequest
            {
                PublicationSlug = PublicationSlug,
                Page = 2,
                PageSize = null,
            };

            // Act & Assert
            _validator
                .TestValidate(request)
                .ShouldHaveValidationErrorFor(r => r.PageSize)
                .WithErrorMessage("'PageSize' must also be provided when 'Page' is set.")
                .Only();
        }

        [Fact]
        public void WhenPageSizeIsNotNullAndPageIsNull_PageHasValidationError()
        {
            // Arrange
            var request = new GetPublicationReleasesRequest
            {
                PublicationSlug = PublicationSlug,
                Page = null,
                PageSize = 10,
            };

            // Act & Assert
            _validator
                .TestValidate(request)
                .ShouldHaveValidationErrorFor(r => r.Page)
                .WithErrorMessage("'Page' must also be provided when 'PageSize' is set.")
                .Only();
        }
    }
}
