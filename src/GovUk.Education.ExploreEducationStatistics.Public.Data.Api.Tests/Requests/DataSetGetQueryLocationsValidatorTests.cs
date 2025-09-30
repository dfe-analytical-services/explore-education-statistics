using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public abstract class DataSetGetQueryLocationsValidatorTests
{
    private readonly DataSetGetQueryLocations.Validator _validator = new();

    public static readonly TheoryData<IDataSetQueryLocation> ValidLocationQueriesSingle =
        DataSetQueryCriteriaLocationsValidatorTests.ValidLocationsSingle;

    public static readonly TheoryData<IDataSetQueryLocation[]> ValidLocationQueriesMultiple =
        DataSetQueryCriteriaLocationsValidatorTests.ValidLocationsMultiple;

    public static readonly TheoryData<string> InvalidLocationFormatsSingle = new()
    {
        "",
        " ",
        "Invalid",
        "NAT|",
        "|",
        "||"
    };

    public static readonly TheoryData<string[]> InvalidLocationFormatsMultiple = new()
    {
        new [] { "", " ", null! },
        new [] { "Invalid", "NAT|", "|", "||" },
    };

    public static readonly TheoryData<IDataSetQueryLocation> InvalidLocationQueriesSingle =
        DataSetQueryCriteriaLocationsValidatorTests.InvalidLocationsSingle;

    public static readonly TheoryData<IDataSetQueryLocation[]> InvalidLocationQueriesMultiple =
        DataSetQueryCriteriaLocationsValidatorTests.InvalidLocationsMultiple;

    public class EqTests : DataSetGetQueryLocationsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidLocationQueriesSingle))]
        public void Success(IDataSetQueryLocation location)
        {
            var query = new DataSetGetQueryLocations { Eq = location.ToLocationString() };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidLocationFormatsSingle))]
        public void Failure_InvalidString(string location)
        {
            var query = new DataSetGetQueryLocations { Eq = location };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Eq)
                .Only();
        }

        [Theory]
        [MemberData(nameof(InvalidLocationQueriesSingle))]
        public void Failure_InvalidQuery(IDataSetQueryLocation location)
        {
            var query = new DataSetGetQueryLocations { Eq = location.ToLocationString() };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Eq)
                .Only();
        }
    }

    public class NotEqTests : DataSetGetQueryLocationsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidLocationQueriesSingle))]
        public void Success(IDataSetQueryLocation location)
        {
            var query = new DataSetGetQueryLocations { NotEq = location.ToLocationString() };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidLocationFormatsSingle))]
        public void Failure_InvalidString(string location)
        {
            var query = new DataSetGetQueryLocations { NotEq = location };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.NotEq);
        }

        [Theory]
        [MemberData(nameof(InvalidLocationQueriesSingle))]
        public void Failure_InvalidQuery(IDataSetQueryLocation location)
        {
            var query = new DataSetGetQueryLocations { NotEq = location.ToLocationString() };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.NotEq);
        }
    }

    public class InTests : DataSetGetQueryLocationsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidLocationQueriesMultiple))]
        public void Success(IDataSetQueryLocation[] locations)
        {
            var query = new DataSetGetQueryLocations
            {
                In = locations.Select(l => l.ToLocationString()).ToList()
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetGetQueryLocations { In = [] };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.In)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Theory]
        [MemberData(nameof(InvalidLocationFormatsMultiple))]
        public void Failure_InvalidFormats(string[] locations)
        {
            var query = new DataSetGetQueryLocations { In = locations };

            var result = _validator.TestValidate(query);

            Assert.Equal(locations.Length, result.Errors.Count);

            locations.ForEach((_, index) => result.ShouldHaveValidationErrorFor($"In[{index}]"));
        }

        [Theory]
        [MemberData(nameof(InvalidLocationQueriesMultiple))]
        public void Failure_InvalidQueries(IDataSetQueryLocation[] locations)
        {
            var query = new DataSetGetQueryLocations
            {
                In = [.. locations.Select(l => l.ToLocationString())]
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(locations.Length, result.Errors.Count);

            locations.ForEach((_, index) => result.ShouldHaveValidationErrorFor($"In[{index}]"));
        }
    }

    public class NotInTests : DataSetGetQueryLocationsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidLocationQueriesMultiple))]
        public void Success(IDataSetQueryLocation[] locations)
        {
            var query = new DataSetGetQueryLocations
            {
                NotIn = locations.Select(l => l.ToLocationString()).ToList()
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetGetQueryLocations { NotIn = [] };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Theory]
        [MemberData(nameof(InvalidLocationFormatsMultiple))]
        public void Failure_InvalidFormats(string[] locations)
        {
            var query = new DataSetGetQueryLocations { NotIn = locations };

            var result = _validator.TestValidate(query);

            Assert.Equal(locations.Length, result.Errors.Count);

            locations.ForEach((_, index) => result.ShouldHaveValidationErrorFor($"NotIn[{index}]"));
        }

        [Theory]
        [MemberData(nameof(InvalidLocationQueriesMultiple))]
        public void Failure_InvalidQueries(IDataSetQueryLocation[] locations)
        {
            var query = new DataSetGetQueryLocations
            {
                NotIn = [.. locations.Select(l => l.ToLocationString())]
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(locations.Length, result.Errors.Count);

            locations.ForEach((_, index) => result.ShouldHaveValidationErrorFor($"NotIn[{index}]"));
        }
    }

    public class EmptyTests : DataSetGetQueryLocationsValidatorTests
    {
        [Fact]
        public void AllEmpty_Failure()
        {
            var query = new DataSetGetQueryLocations
            {
                Eq = "",
                NotEq = "",
                In = [],
                NotIn = []
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(4, result.Errors.Count);

            result.ShouldHaveValidationErrorFor(q => q.Eq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.In)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void SomeEmpty_Failure()
        {
            var query = new DataSetGetQueryLocations
            {
                Eq = "NAT|id|12345",
                NotEq = "",
                In = ["LA|code|12345"],
                NotIn = []
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(2, result.Errors.Count);

            result.ShouldNotHaveValidationErrorFor(q => q.Eq);
            result.ShouldNotHaveValidationErrorFor(q => q.In);

            result.ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }
}
