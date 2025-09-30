using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public class DataSetQueryCriteriaTimePeriodsValidatorTests
{
    private readonly DataSetQueryCriteriaTimePeriods.Validator _validator = new();

    public static readonly TheoryData<DataSetQueryTimePeriod> ValidTimePeriodsSingle = new()
    {
        new DataSetQueryTimePeriod { Period = "2020", Code = "AY" },
        new DataSetQueryTimePeriod { Period = "2020/2021", Code = "AY" },
        new DataSetQueryTimePeriod { Period = "2020", Code = "FY" },
        new DataSetQueryTimePeriod { Period = "2021", Code = "M1" },
        new DataSetQueryTimePeriod { Period = "2021", Code = "W40" },
        new DataSetQueryTimePeriod { Period = "2021", Code = "T3" },
        new DataSetQueryTimePeriod { Period = "2021/2022", Code = "T3" },
        new DataSetQueryTimePeriod { Period = "2019", Code = "FYQ4" },
        new DataSetQueryTimePeriod { Period = "2019/2020", Code = "FYQ4" },
    };

    public static readonly TheoryData<DataSetQueryTimePeriod[]> ValidTimePeriodsMultiple = new()
    {
        new DataSetQueryTimePeriod[]
        {
            new() { Period = "2020", Code = "AY" },
            new() { Period = "2020/2021", Code = "AY" },
            new() { Period = "2020", Code = "FY" },
        },
        new DataSetQueryTimePeriod[]
        {
            new() { Period = "2021", Code = "M1" },
            new() { Period = "2021", Code = "W40" },
            new() { Period = "2021", Code = "T3" },
        },
        new DataSetQueryTimePeriod[]
        {
            new() { Period = "2021/2022", Code = "T3" },
            new() { Period = "2019", Code = "FYQ4" },
            new() { Period = "2019/2020", Code = "FYQ4" },
        },
    };

    public static readonly TheoryData<DataSetQueryTimePeriod> InvalidTimePeriodsSingle = new()
    {
        new DataSetQueryTimePeriod { Period = "", Code = "AY" },
        new DataSetQueryTimePeriod { Period = "Invalid", Code = "AY" },
        new DataSetQueryTimePeriod { Period = "2020/2022", Code = "AY" },
        new DataSetQueryTimePeriod { Period = "2022/2020", Code = "AY" },
        new DataSetQueryTimePeriod { Period = "2000/1999", Code = "AY" },
        new DataSetQueryTimePeriod { Period = "2022", Code = "" },
        new DataSetQueryTimePeriod { Period = "2022/2023", Code = "" },
        new DataSetQueryTimePeriod { Period = "2022", Code = "ay" },
        new DataSetQueryTimePeriod { Period = "2022", Code = "Invalid" },
        new DataSetQueryTimePeriod { Period = "2022", Code = "1" },
        new DataSetQueryTimePeriod { Period = "2022", Code = "WEEK12" },
        new DataSetQueryTimePeriod { Period = "2022/2023", Code = "JANUARY" },
    };

    public static readonly TheoryData<DataSetQueryTimePeriod[]> InvalidTimePeriodsMultiple = new()
    {
        new DataSetQueryTimePeriod[]
        {
            new() { Period = "", Code = "AY" },
            new() { Period = "Invalid", Code = "AY" },
            new() { Period = "2020/2022", Code = "AY" },
            new() { Period = "2022/2020", Code = "AY" },
            new() { Period = "2000/1999", Code = "AY" },
        },
        new DataSetQueryTimePeriod[]
        {
            new() { Period = "2022", Code = "" },
            new() { Period = "2022/2023", Code = "" },
            new() { Period = "2022", Code = "ay" },
            new() { Period = "2022", Code = "Invalid" },
            new() { Period = "2022", Code = "1" },
            new() { Period = "2022", Code = "WEEK12" },
            new() { Period = "2022/2023", Code = "JANUARY" },
        },
    };

    public class EqTests : DataSetQueryCriteriaTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodsSingle))]
        public void Success(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetQueryCriteriaTimePeriods { Eq = timePeriod };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodsSingle))]
        public void Failure(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetQueryCriteriaTimePeriods { Eq = timePeriod };

            var result = _validator.TestValidate(query);

            Assert.NotEmpty(result.Errors);

            Assert.All(result.Errors, error => Assert.StartsWith("Eq", error.PropertyName));
        }
    }

    public class NotEqTests : DataSetQueryCriteriaTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodsSingle))]
        public void Success(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetQueryCriteriaTimePeriods { NotEq = timePeriod };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodsSingle))]
        public void Failure(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetQueryCriteriaTimePeriods { NotEq = timePeriod };

            var result = _validator.TestValidate(query);

            Assert.NotEmpty(result.Errors);

            Assert.All(result.Errors, error => Assert.StartsWith("NotEq", error.PropertyName));
        }
    }

    public class InTests : DataSetQueryCriteriaTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodsMultiple))]
        public void Success(DataSetQueryTimePeriod[] timePeriods)
        {
            var query = new DataSetQueryCriteriaTimePeriods { In = timePeriods };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodsMultiple))]
        public void Failure(DataSetQueryTimePeriod[] timePeriods)
        {
            var query = new DataSetQueryCriteriaTimePeriods { In = timePeriods };

            var result = _validator.TestValidate(query);

            Assert.Equal(timePeriods.Length, result.Errors.Count);

            Assert.All(result.Errors, error => Assert.StartsWith("In", error.PropertyName));
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetQueryCriteriaTimePeriods { In = [] };

            var result = _validator.TestValidate(query);

            result
                .ShouldHaveValidationErrorFor(q => q.In)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }

    public class NotInTests : DataSetQueryCriteriaTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodsMultiple))]
        public void Success(DataSetQueryTimePeriod[] timePeriods)
        {
            var query = new DataSetQueryCriteriaTimePeriods { NotIn = timePeriods };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodsMultiple))]
        public void Failure(DataSetQueryTimePeriod[] timePeriods)
        {
            var query = new DataSetQueryCriteriaTimePeriods { NotIn = timePeriods };

            var result = _validator.TestValidate(query);

            Assert.Equal(timePeriods.Length, result.Errors.Count);

            Assert.All(result.Errors, error => Assert.StartsWith("NotIn", error.PropertyName));
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetQueryCriteriaTimePeriods { NotIn = [] };

            var result = _validator.TestValidate(query);

            result
                .ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }

    public class GtTests : DataSetQueryCriteriaTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodsSingle))]
        public void Success(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetQueryCriteriaTimePeriods { Gt = timePeriod };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodsSingle))]
        public void Failure(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetQueryCriteriaTimePeriods { Gt = timePeriod };

            var result = _validator.TestValidate(query);

            Assert.NotEmpty(result.Errors);

            Assert.All(result.Errors, error => Assert.StartsWith("Gt", error.PropertyName));
        }
    }

    public class GteTests : DataSetQueryCriteriaTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodsSingle))]
        public void Success(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetQueryCriteriaTimePeriods { Gte = timePeriod };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodsSingle))]
        public void Failure(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetQueryCriteriaTimePeriods { Gte = timePeriod };

            var result = _validator.TestValidate(query);

            Assert.NotEmpty(result.Errors);

            Assert.All(result.Errors, error => Assert.StartsWith("Gte", error.PropertyName));
        }
    }

    public class LtTests : DataSetQueryCriteriaTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodsSingle))]
        public void Success(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetQueryCriteriaTimePeriods { Lt = timePeriod };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodsSingle))]
        public void Failure(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetQueryCriteriaTimePeriods { Lt = timePeriod };

            var result = _validator.TestValidate(query);

            Assert.NotEmpty(result.Errors);

            Assert.All(result.Errors, error => Assert.StartsWith("Lt", error.PropertyName));
        }
    }

    public class LteTests : DataSetQueryCriteriaTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodsSingle))]
        public void Success(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetQueryCriteriaTimePeriods { Lte = timePeriod };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodsSingle))]
        public void Failure(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetQueryCriteriaTimePeriods { Lte = timePeriod };

            var result = _validator.TestValidate(query);

            Assert.NotEmpty(result.Errors);

            Assert.All(result.Errors, error => Assert.StartsWith("Lte", error.PropertyName));
        }
    }

    public class EmptyTests : DataSetQueryCriteriaTimePeriodsValidatorTests
    {
        [Fact]
        public void AllNull_Success()
        {
            var query = new DataSetQueryCriteriaTimePeriods();

            var result = _validator.TestValidate(query);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void SomeEmpty_Failure()
        {
            var query = new DataSetQueryCriteriaTimePeriods
            {
                Eq = new DataSetQueryTimePeriod { Period = "2020", Code = "AY" },
                NotEq = null,
                In = [new DataSetQueryTimePeriod { Period = "2021/2022", Code = "T3" }],
                NotIn = [],
                Gt = new DataSetQueryTimePeriod { Period = "2019", Code = "M4" },
                Gte = null,
                Lt = null,
                Lte = new DataSetQueryTimePeriod { Period = "2030", Code = "M7" },
            };

            var result = _validator.TestValidate(query);

            Assert.Single(result.Errors);

            result.ShouldNotHaveValidationErrorFor(q => q.Eq);
            result.ShouldNotHaveValidationErrorFor(q => q.NotEq);
            result.ShouldNotHaveValidationErrorFor(q => q.In);
            result.ShouldNotHaveValidationErrorFor(q => q.Gt);
            result.ShouldNotHaveValidationErrorFor(q => q.Gte);
            result.ShouldNotHaveValidationErrorFor(q => q.Lt);
            result.ShouldNotHaveValidationErrorFor(q => q.Lte);

            result
                .ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }
}
