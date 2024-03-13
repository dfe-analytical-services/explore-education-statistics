using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public abstract class DataSetGetQueryGeographicLevelsValidatorTests
{
    private readonly DataSetGetQueryGeographicLevels.Validator _validator = new();

    public static IEnumerable<object[]> ValidGeographicLevelsSingle()
    {
        return
        [
            ["NAT"],
            ["LA"],
            [null],
        ];
    }

    public static IEnumerable<object[]> ValidGeographicLevelsMultiple()
    {
        return
        [
            ["NAT", "LA", "REG"],
            ["SCH", "NAT"],
            ["PROV"],
        ];
    }

    public static IEnumerable<object[]> InvalidGeographicLevelsSingle()
    {
        return
        [
            [""],
            ["Invalid"],
            ["National"],
            ["nat"],
            ["la"],
            ["1"],
        ];
    }

    public static IEnumerable<object[]> InvalidGeographicLevelsMultiple()
    {
        return
        [
            ["Invalid1", "Invalid2"],
            ["National", "LocalAuthority"],
            ["nat", "la"],
            ["Local authority"],
            ["National"],
        ];
    }

    public class EqTests : DataSetGetQueryGeographicLevelsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidGeographicLevelsSingle))]
        public void Success(string? geographicLevel)
        {
            var query = new DataSetGetQueryGeographicLevels { Eq = geographicLevel };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidGeographicLevelsSingle))]
        public void Failure_NotAllowed(string geographicLevel)
        {
            var query = new DataSetGetQueryGeographicLevels { Eq = geographicLevel };

            var result = _validator.Validate(query);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(DataSetGetQueryGeographicLevels.Eq), error.PropertyName);
            Assert.Equal(ValidationMessages.AllowedValue.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.AllowedValue.Message, error.ErrorMessage);

            var state = Assert.IsType<AllowedValueValidator.AllowedErrorDetail<string>>(error.CustomState);

            Assert.Equal(geographicLevel, state.Value);
            Assert.Equal(EnumUtil.GetEnumValues<GeographicLevel>().Order(), state.Allowed);
        }
    }

    public class NotEqTests : DataSetGetQueryGeographicLevelsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidGeographicLevelsSingle))]
        public void Success(string? geographicLevel)
        {
            var query = new DataSetGetQueryGeographicLevels { NotEq = geographicLevel };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidGeographicLevelsSingle))]
        public void Failure_NotAllowed(string geographicLevel)
        {
            var query = new DataSetGetQueryGeographicLevels { NotEq = geographicLevel };

            var result = _validator.Validate(query);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(DataSetGetQueryGeographicLevels.NotEq), error.PropertyName);
            Assert.Equal(ValidationMessages.AllowedValue.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.AllowedValue.Message, error.ErrorMessage);

            var state = Assert.IsType<AllowedValueValidator.AllowedErrorDetail<string>>(error.CustomState);

            Assert.Equal(geographicLevel, state.Value);
            Assert.Equal(EnumUtil.GetEnumValues<GeographicLevel>().Order(), state.Allowed);
        }
    }

    public class InTests : DataSetGetQueryGeographicLevelsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidGeographicLevelsMultiple))]
        public void Success(params string[] geographicLevels)
        {
            var query = new DataSetGetQueryGeographicLevels { In = geographicLevels };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Success_Null()
        {
            var query = new DataSetGetQueryGeographicLevels { In = null };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidGeographicLevelsMultiple))]
        public void Failure_NotAllowed(params string[] geographicLevels)
        {
            var query = new DataSetGetQueryGeographicLevels { In = geographicLevels };

            var result = _validator.Validate(query);

            Assert.False(result.IsValid);

            Assert.All(result.Errors, (error, index) =>
            {
                Assert.Equal($"In[{index}]", error.PropertyName);
                Assert.Equal(ValidationMessages.AllowedValue.Code, error.ErrorCode);
                Assert.Equal(ValidationMessages.AllowedValue.Message, error.ErrorMessage);

                var state = Assert.IsType<AllowedValueValidator.AllowedErrorDetail<string>>(error.CustomState);

                Assert.Equal(error.AttemptedValue, state.Value);
                Assert.Equal(EnumUtil.GetEnumValues<GeographicLevel>().Order(), state.Allowed);
            });
        }
    }

    public class NotInTests : DataSetGetQueryGeographicLevelsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidGeographicLevelsMultiple))]
        public void Success(params string[] geographicLevels)
        {
            var query = new DataSetGetQueryGeographicLevels { NotIn = geographicLevels };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Success_Null()
        {
            var query = new DataSetGetQueryGeographicLevels { In = null };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }


        [Theory]
        [MemberData(nameof(InvalidGeographicLevelsMultiple))]
        public void Failure_NotAllowed(params string[] geographicLevels)
        {
            var query = new DataSetGetQueryGeographicLevels { In = geographicLevels };

            var result = _validator.Validate(query);

            Assert.False(result.IsValid);

            Assert.All(result.Errors, (error, index) =>
            {
                Assert.Equal($"In[{index}]", error.PropertyName);
                Assert.Equal(ValidationMessages.AllowedValue.Code, error.ErrorCode);
                Assert.Equal(ValidationMessages.AllowedValue.Message, error.ErrorMessage);

                var state = Assert.IsType<AllowedValueValidator.AllowedErrorDetail<string>>(error.CustomState);

                Assert.Equal(error.AttemptedValue, state.Value);
                Assert.Equal(EnumUtil.GetEnumValues<GeographicLevel>().Order(), state.Allowed);
            });
        }
    }

    public class EmptyTests : DataSetGetQueryGeographicLevelsValidatorTests
    {
        [Fact]
        public void AllEmpty_Failure()
        {
            var query = new DataSetGetQueryGeographicLevels
            {
                Eq = "",
                NotEq = "",
                In = [],
                NotIn = []
            };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.Eq)
                .WithErrorCode(ValidationMessages.AllowedValue.Code);
            result.ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(ValidationMessages.AllowedValue.Code);
            result.ShouldHaveValidationErrorFor(q => q.In)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void SomeEmpty_Failure()
        {
            var query = new DataSetGetQueryGeographicLevels
            {
                Eq = "NAT",
                NotEq = "",
                In = ["LA"],
                NotIn = []
            };

            var result = _validator.TestValidate(query);

            result.ShouldNotHaveValidationErrorFor(q => q.Eq);
            result.ShouldNotHaveValidationErrorFor(q => q.In);

            result.ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(ValidationMessages.AllowedValue.Code);
            result.ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }
}
