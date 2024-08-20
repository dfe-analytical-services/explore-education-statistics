using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

[Collection("Set FluentValidation property name camel case configuration")]
public abstract class DataSetQueryCriteriaFacetsValidatorTests
{
    private readonly DataSetQueryCriteriaFacets.Validator _validator = new();

    public class FiltersTests : DataSetQueryCriteriaFacetsValidatorTests
    {
        [Fact]
        public void Success()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                Filters = new DataSetGetQueryFilters
                {
                    Eq = "abc"
                },
            };

            _validator.TestValidate(facets).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                Filters = new DataSetGetQueryFilters
                {
                    Eq = ""
                },
            };

            _validator.TestValidate(facets)
                .ShouldHaveValidationErrorFor("filters.eq")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }

    public class GeographicLevelsTests : DataSetQueryCriteriaFacetsValidatorTests
    {
        [Fact]
        public void Success()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                GeographicLevels = new DataSetGetQueryGeographicLevels
                {
                    Eq = "NAT"
                },
            };

            _validator.TestValidate(facets).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                GeographicLevels = new DataSetGetQueryGeographicLevels
                {
                    Eq = "Invalid"
                },
            };

            _validator.TestValidate(facets)
                .ShouldHaveValidationErrorFor("geographicLevels.eq")
                .WithErrorCode(Common.Validators.ValidationMessages.AllowedValue.Code);
        }
    }

    public class LocationsTests : DataSetQueryCriteriaFacetsValidatorTests
    {
        [Fact]
        public void Success()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                Locations = new DataSetQueryCriteriaLocations
                {
                    Eq = new DataSetQueryLocationId { Level = "NAT", Id = "12345" },
                },
            };

            _validator.TestValidate(facets).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                Locations = new DataSetQueryCriteriaLocations
                {
                    Eq = new DataSetQueryLocationId { Level = "Invalid", Id = "12345" },
                },
            };

            _validator.TestValidate(facets)
                .ShouldHaveValidationErrorFor("locations.eq.level")
                .WithErrorCode(Common.Validators.ValidationMessages.AllowedValue.Code);
        }
    }

    public class TimePeriodsTests : DataSetQueryCriteriaFacetsValidatorTests
    {
        [Fact]
        public void Success()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                TimePeriods = new DataSetQueryCriteriaTimePeriods
                {
                    Eq = new DataSetQueryTimePeriod { Period = "2020/2021", Code = "AY" },
                },
            };

            _validator.TestValidate(facets).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                TimePeriods = new DataSetQueryCriteriaTimePeriods
                {
                    Eq = new DataSetQueryTimePeriod { Period = "2020/2021", Code = "Invalid" }
                },
            };

            _validator.TestValidate(facets)
                .ShouldHaveValidationErrorFor("timePeriods.eq.code")
                .WithErrorCode(ValidationMessages.TimePeriodAllowedCode.Code);
        }
    }

    public class MixtureTests : DataSetQueryCriteriaFacetsValidatorTests
    {
        [Fact]
        public void Success()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                Filters = new DataSetGetQueryFilters
                {
                    Eq = "abc"
                },
                Locations = new DataSetQueryCriteriaLocations
                {
                    Eq = new DataSetQueryLocationId { Level = "NAT", Id = "12345" },
                },
                GeographicLevels = new DataSetGetQueryGeographicLevels
                {
                    Eq = "NAT"
                },
                TimePeriods = new DataSetQueryCriteriaTimePeriods
                {
                    Eq = new DataSetQueryTimePeriod { Period = "2020/2021", Code = "AY" },
                },
            };

            _validator.TestValidate(facets).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                Filters = new DataSetGetQueryFilters
                {
                    Eq = ""
                },
                GeographicLevels = new DataSetGetQueryGeographicLevels
                {
                    Eq = "Invalid"
                },
                Locations = new DataSetQueryCriteriaLocations
                {
                    Eq = new DataSetQueryLocationId { Level = "REG", Id = new string('x', 11) },
                },
                TimePeriods = new DataSetQueryCriteriaTimePeriods
                {
                    Eq = new DataSetQueryTimePeriod { Period = "2020/2018", Code = "AY" },
                },
            };

            var result = _validator.TestValidate(facets);

            result.ShouldHaveValidationErrorFor("filters.eq")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);

            result.ShouldHaveValidationErrorFor("geographicLevels.eq")
                .WithErrorCode(Common.Validators.ValidationMessages.AllowedValue.Code);

            result.ShouldHaveValidationErrorFor("locations.eq.id")
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator);

            result.ShouldHaveValidationErrorFor("timePeriods.eq.period")
                .WithErrorCode(ValidationMessages.TimePeriodInvalidYearRange.Code);
        }
    }
}
