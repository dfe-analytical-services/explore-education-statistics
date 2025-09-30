using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

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
                Filters = new DataSetGetQueryFilters { Eq = "abc" },
            };

            _validator.TestValidate(facets).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                Filters = new DataSetGetQueryFilters { Eq = "" },
            };

            _validator
                .TestValidate(facets)
                .ShouldHaveValidationErrorFor(f => f.Filters!.Eq)
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
                GeographicLevels = new DataSetGetQueryGeographicLevels { Eq = "NAT" },
            };

            _validator.TestValidate(facets).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                GeographicLevels = new DataSetGetQueryGeographicLevels { Eq = "Invalid" },
            };

            _validator
                .TestValidate(facets)
                .ShouldHaveValidationErrorFor(f => f.GeographicLevels!.Eq)
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

            _validator
                .TestValidate(facets)
                .ShouldHaveValidationErrorFor(f => f.Locations!.Eq!.Level)
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
                    Eq = new DataSetQueryTimePeriod { Period = "2020/2021", Code = "Invalid" },
                },
            };

            _validator
                .TestValidate(facets)
                .ShouldHaveValidationErrorFor(f => f.TimePeriods!.Eq!.Code)
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
                Filters = new DataSetGetQueryFilters { Eq = "abc" },
                Locations = new DataSetQueryCriteriaLocations
                {
                    Eq = new DataSetQueryLocationId { Level = "NAT", Id = "12345" },
                },
                GeographicLevels = new DataSetGetQueryGeographicLevels { Eq = "NAT" },
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
                Filters = new DataSetGetQueryFilters { Eq = "" },
                GeographicLevels = new DataSetGetQueryGeographicLevels { Eq = "Invalid" },
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

            result
                .ShouldHaveValidationErrorFor(f => f.Filters!.Eq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);

            result
                .ShouldHaveValidationErrorFor(f => f.GeographicLevels!.Eq)
                .WithErrorCode(Common.Validators.ValidationMessages.AllowedValue.Code);

            result
                .ShouldHaveValidationErrorFor("Locations.Eq.Id")
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator);

            result
                .ShouldHaveValidationErrorFor(f => f.TimePeriods!.Eq!.Period)
                .WithErrorCode(ValidationMessages.TimePeriodInvalidYearRange.Code);
        }
    }
}
