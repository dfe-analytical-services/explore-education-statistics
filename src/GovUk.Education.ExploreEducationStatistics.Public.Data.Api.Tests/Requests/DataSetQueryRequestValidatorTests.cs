using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using ValidationErrorMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public class DataSetGetQueryRequestValidatorTests
{
    private readonly DataSetGetQueryRequest.Validator _validator = new();

    public class FiltersTests : DataSetGetQueryRequestValidatorTests
    {
        [Fact]
        public void Success()
        {
            var query = new DataSetGetQueryRequest
            {
                Filters = new DataSetGetQueryFilters
                {
                    Eq = "abc"
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var query = new DataSetGetQueryRequest
            {
                Filters = new DataSetGetQueryFilters
                {
                    Eq = ""
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query)
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
                GeographicLevels = new DataSetGetQueryGeographicLevels
                {
                    Eq = "NAT"
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var query = new DataSetGetQueryRequest
            {
                GeographicLevels = new DataSetGetQueryGeographicLevels
                {
                    Eq = "invalid"
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.GeographicLevels!.Eq)
                .WithErrorCode(Common.Validators.ValidationErrorMessages.AllowedValue.Code);
        }
    }

    public class LocationsTests : DataSetGetQueryRequestValidatorTests
    {
        [Fact]
        public void Success()
        {
            var query = new DataSetGetQueryRequest
            {
                Locations = new DataSetGetQueryLocations
                {
                    Eq = "NAT|id|12345"
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var query = new DataSetGetQueryRequest
            {
                Locations = new DataSetGetQueryLocations
                {
                    Eq = "invalid"
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Locations!.Eq)
                .WithErrorCode(ValidationErrorMessages.LocationFormat.Code);
        }
    }

    public class TimePeriodsTests : DataSetGetQueryRequestValidatorTests
    {
        [Fact]
        public void Success()
        {
            var query = new DataSetGetQueryRequest
            {
                TimePeriods = new DataSetGetQueryTimePeriods
                {
                    Eq = "2020|AY"
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var query = new DataSetGetQueryRequest
            {
                TimePeriods = new DataSetGetQueryTimePeriods
                {
                    Eq = "invalid"
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.TimePeriods!.Eq)
                .WithErrorCode(ValidationErrorMessages.TimePeriodFormat.Code);
        }
    }

    public class IndicatorsTests : DataSetGetQueryRequestValidatorTests
    {
        [Fact]
        public void Success()
        {
            var query = new DataSetGetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetGetQueryRequest
            {
                Indicators = []
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Indicators)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_EmptyStrings()
        {
            var query = new DataSetGetQueryRequest
            {
                Indicators = ["", ""]
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Indicators)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_MaxLength()
        {
            var query = new DataSetGetQueryRequest
            {
                Indicators = [new string('a', 101), new string('a', 200)]
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
            var query = new DataSetGetQueryRequest
            {
                Indicators = [new string('a', 101), ""]
            };

            var result = _validator.TestValidate(query);

            result
                .ShouldHaveValidationErrorFor("Indicators[0]")
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator);

            result
                .ShouldHaveValidationErrorFor("Indicators[1]")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }
}
