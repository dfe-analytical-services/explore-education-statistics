using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public abstract class DataSetGetQueryRequestValidatorTests
{
    private readonly DataSetGetQueryRequest.Validator _validator = new();

    public class FiltersTests : DataSetGetQueryRequestValidatorTests
    {
        [Fact]
        public void Success()
        {
            var query = new DataSetGetQueryRequest
            {
                Filters = new DataSetGetQueryFilters { Eq = "abc" },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var query = new DataSetGetQueryRequest
            {
                Filters = new DataSetGetQueryFilters { Eq = "" },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Filters!.Eq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }

    public class GeographicLevelsTests : DataSetGetQueryRequestValidatorTests
    {
        [Fact]
        public void Success()
        {
            var query = new DataSetGetQueryRequest
            {
                GeographicLevels = new DataSetGetQueryGeographicLevels { Eq = "NAT" },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var query = new DataSetGetQueryRequest
            {
                GeographicLevels = new DataSetGetQueryGeographicLevels { Eq = "invalid" },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.GeographicLevels!.Eq)
                .WithErrorCode(Common.Validators.ValidationMessages.AllowedValue.Code);
        }
    }

    public class LocationsTests : DataSetGetQueryRequestValidatorTests
    {
        [Fact]
        public void Success()
        {
            var query = new DataSetGetQueryRequest
            {
                Locations = new DataSetGetQueryLocations { Eq = "NAT|id|12345" },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var query = new DataSetGetQueryRequest
            {
                Locations = new DataSetGetQueryLocations { Eq = "invalid" },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Locations!.Eq)
                .WithErrorCode(ValidationMessages.LocationFormat.Code);
        }
    }

    public class TimePeriodsTests : DataSetGetQueryRequestValidatorTests
    {
        [Fact]
        public void Success()
        {
            var query = new DataSetGetQueryRequest
            {
                TimePeriods = new DataSetGetQueryTimePeriods { Eq = "2020|AY" },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var query = new DataSetGetQueryRequest
            {
                TimePeriods = new DataSetGetQueryTimePeriods { Eq = "invalid" },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.TimePeriods!.Eq)
                .WithErrorCode(ValidationMessages.TimePeriodFormat.Code);
        }
    }

    public class IndicatorsTests : DataSetGetQueryRequestValidatorTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("indicator1")]
        [InlineData("indicator1111111111111111111111111111111")]
        [InlineData("indicator1", "indicator2")]
        public void Success(params string[]? indicators)
        {
            var query = new DataSetGetQueryRequest { Indicators = indicators };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("", " ")]
        public void Failure_EmptyStrings(params string[] indicators)
        {
            var query = new DataSetGetQueryRequest { Indicators = indicators };

            var result = _validator.TestValidate(query);

            foreach (var (_, index) in indicators.WithIndex())
            {
                result
                    .ShouldHaveValidationErrorFor($"Indicators[{index}]")
                    .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            }
        }

        [Fact]
        public void Failure_MaxLength()
        {
            var query = new DataSetGetQueryRequest
            {
                Indicators = [new string('a', 101), new string('a', 200)],
            };

            var result = _validator.TestValidate(query);

            result
                .ShouldHaveValidationErrorFor("Indicators[0]")
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator);

            result
                .ShouldHaveValidationErrorFor("Indicators[1]")
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
        }

        [Fact]
        public void Failure_Mixture()
        {
            var query = new DataSetGetQueryRequest { Indicators = [new string('a', 101), ""] };

            var result = _validator.TestValidate(query);

            result
                .ShouldHaveValidationErrorFor("Indicators[0]")
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator);

            result
                .ShouldHaveValidationErrorFor("Indicators[1]")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }

    public class SortsTests : DataSetGetQueryRequestValidatorTests
    {
        [Fact]
        public void Success()
        {
            var query = new DataSetGetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                Sorts = ["TimePeriod|Asc", "GeographicLevel|Asc"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetGetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                Sorts = [],
            };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Sorts)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_InvalidStrings()
        {
            var query = new DataSetGetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                Sorts =
                [
                    "",
                    "Invalid",
                    "TimePeriod|asc",
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa|Asc",
                ],
            };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.Sorts);

            result
                .ShouldHaveValidationErrorFor("Sorts[0]")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);

            result
                .ShouldHaveValidationErrorFor("Sorts[1]")
                .WithErrorCode(ValidationMessages.SortFormat.Code);

            result
                .ShouldHaveValidationErrorFor("Sorts[2]")
                .WithErrorCode(ValidationMessages.SortDirection.Code);

            result
                .ShouldHaveValidationErrorFor("Sorts[3]")
                .WithErrorCode(ValidationMessages.SortFieldMaxLength.Code);
        }
    }

    public class PageTests : DataSetGetQueryRequestValidatorTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        public void Success(int page)
        {
            var query = new DataSetGetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                Page = page,
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(-10)]
        public void Failure_LessThanOne(int page)
        {
            var query = new DataSetGetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                Page = page,
            };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Page)
                .WithErrorCode(FluentValidationKeys.GreaterThanOrEqualValidator);
        }
    }

    public class PageSizeTests : DataSetGetQueryRequestValidatorTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(10000)]
        public void Success(int pageSize)
        {
            var query = new DataSetGetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                PageSize = pageSize,
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(10001)]
        public void Failure_OutOfBounds(int pageSize)
        {
            var query = new DataSetGetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                PageSize = pageSize,
            };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.PageSize)
                .WithErrorCode(FluentValidationKeys.InclusiveBetweenValidator);
        }
    }
}
