using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public class DataSetQueryCriteriaLocationsValidatorTests
{
    private readonly DataSetQueryCriteriaLocations.Validator _validator = new();

    public static readonly TheoryData<IDataSetQueryLocation> ValidLocationsSingle = new()
    {
        new DataSetQueryLocationId { Level = "NAT", Id = "12345" },
        new DataSetQueryLocationId { Level = "REG", Id = "12345" },
        new DataSetQueryLocationId { Level = "LA", Id = "12345" },
        new DataSetQueryLocationId { Level = "SCH", Id = "12345" },
        new DataSetQueryLocationId { Level = "PROV", Id = "12345" },
        new DataSetQueryLocationCode { Level = "NAT", Code = "E92000001" },
        new DataSetQueryLocationCode { Level = "REG", Code = "E12000003" },
        new DataSetQueryLocationLocalAuthorityCode { Code = "E08000019" },
        new DataSetQueryLocationLocalAuthorityCode { Code = "E09000021 / E09000027" },
        new DataSetQueryLocationLocalAuthorityOldCode { OldCode = "373" },
        new DataSetQueryLocationLocalAuthorityOldCode { OldCode = "314 / 318" },
        new DataSetQueryLocationSchoolUrn { Urn = "107029" },
        new DataSetQueryLocationSchoolLaEstab { LaEstab = "3732060" },
        new DataSetQueryLocationProviderUkprn { Ukprn = "10066874" },
    };

    public static readonly TheoryData<IDataSetQueryLocation[]> ValidLocationsMultiple = new()
    {
        new IDataSetQueryLocation[]
        {
            new DataSetQueryLocationId { Level = "NAT", Id = "12345" },
            new DataSetQueryLocationCode { Level = "NAT", Code = "E92000001" },
        },
        new IDataSetQueryLocation[]
        {
            new DataSetQueryLocationId { Level = "REG", Id = "12345" },
            new DataSetQueryLocationCode { Level = "REG", Code = "E12000003" },
        },
        new IDataSetQueryLocation[]
        {
            new DataSetQueryLocationId { Level = "LA", Id = "12345" },
            new DataSetQueryLocationLocalAuthorityCode { Code = "E08000019" },
            new DataSetQueryLocationLocalAuthorityOldCode { OldCode = "373" },
        },
        new IDataSetQueryLocation[]
        {
            new DataSetQueryLocationId { Level = "SCH", Id = "12345" },
            new DataSetQueryLocationSchoolUrn { Urn = "107029" },
            new DataSetQueryLocationSchoolLaEstab { LaEstab = "3732060" },
        },
        new IDataSetQueryLocation[]
        {
            new DataSetQueryLocationId { Level = "PROV", Id = "12345" },
            new DataSetQueryLocationProviderUkprn { Ukprn = "10066874" },
        },
    };

    public static readonly TheoryData<IDataSetQueryLocation> InvalidLocationsSingle = new()
    {
        new DataSetQueryLocationId { Level = "", Id = "" },
        new DataSetQueryLocationId { Level = "NAT", Id = "" },
        new DataSetQueryLocationCode { Level = "REG", Code = "" },
        new DataSetQueryLocationLocalAuthorityCode { Code = "" },
        new DataSetQueryLocationId { Level = "NA", Id = "12345" },
        new DataSetQueryLocationId { Level = "la", Id = "12345" },
        new DataSetQueryLocationId { Level = "", Id = "12345" },
        new DataSetQueryLocationId { Level = "NAT", Id = new string('x', 11) },
        new DataSetQueryLocationCode { Level = "REG", Code = new string('x', 31) },
        new DataSetQueryLocationLocalAuthorityOldCode { OldCode = new string('x', 21) },
        new DataSetQueryLocationProviderUkprn { Ukprn = new string('x', 21) },
        new DataSetQueryLocationSchoolUrn { Urn = new string('x', 21) },
    };

    public static readonly TheoryData<IDataSetQueryLocation[]> InvalidLocationsMultiple = new()
    {
        new IDataSetQueryLocation[]
        {
            new DataSetQueryLocationId { Level = "", Id = "" },
            new DataSetQueryLocationId { Level = "NAT", Id = "" },
            new DataSetQueryLocationCode { Level = "REG", Code = "" },
            new DataSetQueryLocationLocalAuthorityCode { Code = "" },
        },
        new IDataSetQueryLocation[]
        {
            new DataSetQueryLocationId { Level = "NA", Id = "12345" },
            new DataSetQueryLocationId { Level = "la", Id = "12345" },
            new DataSetQueryLocationId { Level = "", Id = "12345" },
        },
        new IDataSetQueryLocation[]
        {
            new DataSetQueryLocationId { Level = "NAT", Id = new string('x', 11) },
            new DataSetQueryLocationCode { Level = "REG", Code = new string('x', 31) },
            new DataSetQueryLocationLocalAuthorityOldCode { OldCode = new string('x', 21) },
            new DataSetQueryLocationProviderUkprn { Ukprn = new string('x', 21) },
            new DataSetQueryLocationSchoolUrn { Urn = new string('x', 21) },
        },
    };

    public class EqTests : DataSetQueryCriteriaLocationsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidLocationsSingle))]
        public void Success(IDataSetQueryLocation location)
        {
            var query = new DataSetQueryCriteriaLocations { Eq = location };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidLocationsSingle))]
        public void Failure(IDataSetQueryLocation location)
        {
            var query = new DataSetQueryCriteriaLocations { Eq = location };

            var result = _validator.TestValidate(query);

            Assert.NotEmpty(result.Errors);

            Assert.All(result.Errors, error => Assert.StartsWith("Eq", error.PropertyName));
        }
    }

    public class NotEqTests : DataSetQueryCriteriaLocationsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidLocationsSingle))]
        public void Success(IDataSetQueryLocation location)
        {
            var query = new DataSetQueryCriteriaLocations { NotEq = location };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidLocationsSingle))]
        public void Failure(IDataSetQueryLocation location)
        {
            var query = new DataSetQueryCriteriaLocations { NotEq = location };

            var result = _validator.TestValidate(query);

            Assert.NotEmpty(result.Errors);

            Assert.All(result.Errors, error => Assert.StartsWith("NotEq", error.PropertyName));
        }
    }

    public class InTests : DataSetQueryCriteriaLocationsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidLocationsMultiple))]
        public void Success(params IDataSetQueryLocation[] locations)
        {
            var query = new DataSetQueryCriteriaLocations { In = locations };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidLocationsMultiple))]
        public void Failure(IDataSetQueryLocation[] locations)
        {
            var query = new DataSetQueryCriteriaLocations { In = locations };

            var result = _validator.TestValidate(query);

            Assert.True(result.Errors.Count >= locations.Length, "Must have at least as many errors as locations");

            Assert.All(result.Errors, error => Assert.StartsWith("In", error.PropertyName));
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetQueryCriteriaLocations { In = [] };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.In).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }

    public class NotInTests : DataSetQueryCriteriaLocationsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidLocationsMultiple))]
        public void Success(params IDataSetQueryLocation[] locations)
        {
            var query = new DataSetQueryCriteriaLocations { NotIn = locations };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidLocationsMultiple))]
        public void Failure(params IDataSetQueryLocation[] locations)
        {
            var query = new DataSetQueryCriteriaLocations { NotIn = locations };

            var result = _validator.TestValidate(query);

            Assert.True(result.Errors.Count >= locations.Length, "Must have at least as many errors as locations");

            Assert.All(result.Errors, error => Assert.StartsWith("NotIn", error.PropertyName));
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetQueryCriteriaLocations { NotIn = [] };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.NotIn).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }

    public class EmptyTests : DataSetQueryCriteriaLocationsValidatorTests
    {
        [Fact]
        public void AllNull_Success()
        {
            var query = new DataSetQueryCriteriaLocations();

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void SomeEmpty_Failure()
        {
            var query = new DataSetQueryCriteriaLocations
            {
                Eq = new DataSetQueryLocationId { Level = "NAT", Id = "12345" },
                NotEq = null,
                In = [new DataSetQueryLocationCode { Level = "REG", Code = "12345" }],
                NotIn = [],
            };

            var result = _validator.TestValidate(query);

            Assert.Single(result.Errors);

            result.ShouldNotHaveValidationErrorFor(q => q.Eq);
            result.ShouldNotHaveValidationErrorFor(q => q.In);
            result.ShouldNotHaveValidationErrorFor(q => q.NotEq);

            result.ShouldHaveValidationErrorFor(q => q.NotIn).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }
}
