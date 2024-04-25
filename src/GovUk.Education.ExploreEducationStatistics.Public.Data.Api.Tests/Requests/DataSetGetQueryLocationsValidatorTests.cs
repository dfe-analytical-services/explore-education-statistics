using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public abstract class DataSetGetQueryLocationsValidatorTests
{
    private readonly DataSetGetQueryLocations.Validator _validator = new();

    public static IEnumerable<object[]> ValidLocationStringsSingle()
    {
        return
        [
            ["NAT|id|12345"],
            ["NAT|code|E92000001"],
            ["REG|id|12345"],
            ["REG|code|E12000003"],
            ["LA|id|12345"],
            ["LA|code|E08000019"],
            ["LA|code|E09000021 / E09000027"],
            ["LA|oldCode|373"],
            ["LA|oldCode|314 / 318"],
            ["SCH|id|12345"],
            ["SCH|urn|107029"],
            ["SCH|laEstab|3732060"],
            ["PROV|id|12345"],
            ["PROV|ukprn|10066874"],
        ];
    }

    public static IEnumerable<object[]> ValidLocationStringsMultiple()
    {
        return
        [
            ["NAT|id|12345", "NAT|code|E92000001"],
            ["REG|id|12345", "REG|code|E12000003"],
            ["LA|id|12345", "LA|code|E08000019", "LA|oldCode|373"],
            ["SCH|id|12345", "SCH|urn|107029", "SCH|laEstab|3732060"],
            ["PROV|id|12345", "PROV|ukprn|10066874"],
        ];
    }

    public static IEnumerable<object[]> InvalidLocationStringsSingle()
    {
        return
        [
            [""],
            ["Invalid"],
            ["NA|id|12345"],
            ["NAT|urn|12345"],
            ["NAT|id|99999999999999999999999999"],
        ];
    }

    public static IEnumerable<object[]> InvalidLocationStringsMultiple()
    {
        return
        [
            [],
            ["", ""],
            ["Invalid", "NAT|id", "NAT|code|"],
            ["NA|id|12345", "la|code|12345"],
            ["NAT|urn|12345", "REG|ukprn|12345", "RSC|code|12345"],
            ["NAT|id|99999999999999999999999999", "NAT|code|99999999999999999999999999"],
        ];
    }

    public class EqTests : DataSetGetQueryLocationsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidLocationStringsSingle))]
        public void Success(string location)
        {
            var query = new DataSetGetQueryLocations { Eq = location };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidLocationStringsSingle))]
        public void Failure_InvalidString(string location)
        {
            var query = new DataSetGetQueryLocations { Eq = location };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Eq);
        }
    }

    public class NotEqTests : DataSetGetQueryLocationsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidLocationStringsSingle))]
        public void Success(string location)
        {
            var query = new DataSetGetQueryLocations { NotEq = location };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidLocationStringsSingle))]
        public void Failure_InvalidString(string location)
        {
            var query = new DataSetGetQueryLocations { NotEq = location };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.NotEq);
        }
    }

    public class InTests : DataSetGetQueryLocationsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidLocationStringsMultiple))]
        public void Success(params string[] locations)
        {
            var testObj = new DataSetGetQueryLocations { In = locations };

            _validator.TestValidate(testObj).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidLocationStringsMultiple))]
        public void Failure_InvalidStrings(params string[] locations)
        {
            var query = new DataSetGetQueryLocations { In = locations };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.In);

            locations.ForEach((_, index) => result.ShouldHaveValidationErrorFor($"In[{index}]"));
        }
    }

    public class NotInTests : DataSetGetQueryLocationsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidLocationStringsMultiple))]
        public void Success(params string[] locations)
        {
            var testObj = new DataSetGetQueryLocations { NotIn = locations };

            _validator.TestValidate(testObj).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidLocationStringsMultiple))]
        public void Failure_InvalidStrings(params string[] locations)
        {
            var query = new DataSetGetQueryLocations { NotIn = locations };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.NotIn);

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

            result.ShouldNotHaveValidationErrorFor(q => q.Eq);
            result.ShouldNotHaveValidationErrorFor(q => q.In);

            result.ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }
}
