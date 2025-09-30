using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public abstract class DataSetQueryCriteriaGeographicLevelsValidatorTests
{
    private readonly DataSetQueryCriteriaGeographicLevels.Validator _validator = new();

    public static readonly TheoryData<string?> ValidGeographicLevelsSingle = new()
    {
        "NAT",
        "LA",
        null,
    };

    public static readonly TheoryData<string[]> ValidGeographicLevelsMultiple = new()
    {
        new[] { "NAT", "LA", "REG" },
        new[] { "SCH", "NAT" },
        new[] { "PROV" },
    };

    public static readonly TheoryData<string> InvalidGeographicLevelsSingle = new()
    {
        "",
        " ",
        "Invalid",
        "National",
        "nat",
        "la",
        "1",
    };

    public static readonly TheoryData<string[]> InvalidGeographicLevelsMultiple = new()
    {
        new[] { "", " ", null! },
        new[] { "Invalid1", "Invalid2" },
        new[] { "National", "LocalAuthority" },
        new[] { "nat", "la" },
        new[] { "Local authority" },
        new[] { "National" },
    };

    public class EqTests : DataSetQueryCriteriaGeographicLevelsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidGeographicLevelsSingle))]
        public void Success(string? geographicLevel)
        {
            var query = new DataSetQueryCriteriaGeographicLevels { Eq = geographicLevel };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidGeographicLevelsSingle))]
        public void Failure_NotAllowed(string geographicLevel)
        {
            var query = new DataSetQueryCriteriaGeographicLevels { Eq = geographicLevel };

            var result = _validator.TestValidate(query);

            result
                .ShouldHaveValidationErrorFor(g => g.Eq)
                .WithErrorCode(ValidationMessages.AllowedValue.Code)
                .WithErrorMessage(ValidationMessages.AllowedValue.Message)
                .WithCustomState<AllowedValueValidator.AllowedErrorDetail<string>>(s =>
                    s.Value == geographicLevel
                )
                .WithCustomState<AllowedValueValidator.AllowedErrorDetail<string>>(s =>
                    s.Allowed.SequenceEqual(GeographicLevelUtils.OrderedCodes)
                )
                .Only();
        }
    }

    public class NotEqTests : DataSetQueryCriteriaGeographicLevelsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidGeographicLevelsSingle))]
        public void Success(string? geographicLevel)
        {
            var query = new DataSetQueryCriteriaGeographicLevels { NotEq = geographicLevel };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidGeographicLevelsSingle))]
        public void Failure_NotAllowed(string geographicLevel)
        {
            var query = new DataSetQueryCriteriaGeographicLevels { NotEq = geographicLevel };

            var result = _validator.TestValidate(query);

            result
                .ShouldHaveValidationErrorFor(g => g.NotEq)
                .WithErrorCode(ValidationMessages.AllowedValue.Code)
                .WithErrorMessage(ValidationMessages.AllowedValue.Message)
                .WithCustomState<AllowedValueValidator.AllowedErrorDetail<string>>(s =>
                    s.Value == geographicLevel
                )
                .WithCustomState<AllowedValueValidator.AllowedErrorDetail<string>>(s =>
                    s.Allowed.SequenceEqual(GeographicLevelUtils.OrderedCodes)
                )
                .Only();
        }
    }

    public class InTests : DataSetQueryCriteriaGeographicLevelsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidGeographicLevelsMultiple))]
        public void Success(params string[] geographicLevels)
        {
            var query = new DataSetQueryCriteriaGeographicLevels { In = geographicLevels };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Success_Null()
        {
            var query = new DataSetQueryCriteriaGeographicLevels { In = null };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidGeographicLevelsMultiple))]
        public void Failure_NotAllowed(params string[] geographicLevels)
        {
            var query = new DataSetQueryCriteriaGeographicLevels { In = geographicLevels };

            var result = _validator.TestValidate(query);

            Assert.Equal(geographicLevels.Length, result.Errors.Count);

            foreach (var (error, index) in result.Errors.WithIndex())
            {
                result
                    .ShouldHaveValidationErrorFor($"In[{index}]")
                    .WithErrorCode(ValidationMessages.AllowedValue.Code)
                    .WithErrorMessage(ValidationMessages.AllowedValue.Message)
                    .WithCustomState<AllowedValueValidator.AllowedErrorDetail<string>>(s =>
                        s.Value == (string)error.AttemptedValue
                    )
                    .WithCustomState<AllowedValueValidator.AllowedErrorDetail<string>>(s =>
                        s.Allowed.SequenceEqual(GeographicLevelUtils.OrderedCodes)
                    );
            }
        }
    }

    public class NotInTests : DataSetQueryCriteriaGeographicLevelsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidGeographicLevelsMultiple))]
        public void Success(params string[] geographicLevels)
        {
            var query = new DataSetQueryCriteriaGeographicLevels { NotIn = geographicLevels };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Success_Null()
        {
            var query = new DataSetQueryCriteriaGeographicLevels { NotIn = null };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidGeographicLevelsMultiple))]
        public void Failure_NotAllowed(params string[] geographicLevels)
        {
            var query = new DataSetQueryCriteriaGeographicLevels { NotIn = geographicLevels };

            var result = _validator.TestValidate(query);

            Assert.Equal(geographicLevels.Length, result.Errors.Count);

            foreach (var (error, index) in result.Errors.WithIndex())
            {
                result
                    .ShouldHaveValidationErrorFor($"NotIn[{index}]")
                    .WithErrorCode(ValidationMessages.AllowedValue.Code)
                    .WithErrorMessage(ValidationMessages.AllowedValue.Message)
                    .WithCustomState<AllowedValueValidator.AllowedErrorDetail<string>>(s =>
                        s.Value == (string)error.AttemptedValue
                    )
                    .WithCustomState<AllowedValueValidator.AllowedErrorDetail<string>>(s =>
                        s.Allowed.SequenceEqual(GeographicLevelUtils.OrderedCodes)
                    );
            }
        }
    }

    public class EmptyTests : DataSetQueryCriteriaGeographicLevelsValidatorTests
    {
        [Fact]
        public void AllEmpty_Failure()
        {
            var query = new DataSetQueryCriteriaGeographicLevels
            {
                Eq = "",
                NotEq = "",
                In = [],
                NotIn = [],
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(4, result.Errors.Count);

            result
                .ShouldHaveValidationErrorFor(q => q.Eq)
                .WithErrorCode(ValidationMessages.AllowedValue.Code);
            result
                .ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(ValidationMessages.AllowedValue.Code);
            result
                .ShouldHaveValidationErrorFor(q => q.In)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result
                .ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void SomeEmpty_Failure()
        {
            var query = new DataSetQueryCriteriaGeographicLevels
            {
                Eq = "NAT",
                NotEq = "",
                In = ["LA"],
                NotIn = [],
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(2, result.Errors.Count);

            result.ShouldNotHaveValidationErrorFor(q => q.Eq);
            result.ShouldNotHaveValidationErrorFor(q => q.In);

            result
                .ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(ValidationMessages.AllowedValue.Code);
            result
                .ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }
}
