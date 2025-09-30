using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public abstract class DataSetGetQueryTimePeriodsValidatorTests
{
    private readonly DataSetGetQueryTimePeriods.Validator _validator = new();

    public static readonly TheoryData<DataSetQueryTimePeriod> ValidTimePeriodQueriesSingle =
        DataSetQueryCriteriaTimePeriodsValidatorTests.ValidTimePeriodsSingle;

    public static readonly TheoryData<DataSetQueryTimePeriod[]> ValidTimePeriodQueriesMultiple =
        DataSetQueryCriteriaTimePeriodsValidatorTests.ValidTimePeriodsMultiple;

    public static readonly TheoryData<string> InvalidTimePeriodFormatsSingle = new()
    {
        "",
        "Invalid",
        "2022",
        "2022/2023",
        "20222",
        "|"
    };

    public static readonly TheoryData<string[]> InvalidTimePeriodFormatsMultiple = new()
    {
        new[] { "", " ", null! },
        new[] { "2022", "2022/2023" },
        new[] { "Invalid", "|" },
    };

    public static readonly TheoryData<DataSetQueryTimePeriod> InvalidTimePeriodQueriesSingle =
        DataSetQueryCriteriaTimePeriodsValidatorTests.InvalidTimePeriodsSingle;

    public static readonly TheoryData<DataSetQueryTimePeriod[]> InvalidTimePeriodQueriesMultiple =
        DataSetQueryCriteriaTimePeriodsValidatorTests.InvalidTimePeriodsMultiple;

    public class EqTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodQueriesSingle))]
        public void Success(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Eq = timePeriod.ToTimePeriodString() };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodFormatsSingle))]
        public void Failure_InvalidFormat(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Eq = timePeriod };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Eq)
                .Only();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodQueriesSingle))]
        public void Failure_InvalidQuery(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Eq = timePeriod.ToTimePeriodString() };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Eq)
                .Only();
        }
    }

    public class NotEqTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodQueriesSingle))]
        public void Success(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { NotEq = timePeriod.ToTimePeriodString() };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodFormatsSingle))]
        public void Failure_InvalidFormat(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { NotEq = timePeriod };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.NotEq)
                .Only();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodQueriesSingle))]
        public void Failure_InvalidQuery(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { NotEq = timePeriod.ToTimePeriodString() };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.NotEq)
                .Only();
        }
    }

    public class InTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodQueriesMultiple))]
        public void Success(DataSetQueryTimePeriod[] timePeriods)
        {
            var query = new DataSetGetQueryTimePeriods
            {
                In = [.. timePeriods.Select(t => t.ToTimePeriodString())]
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetGetQueryTimePeriods { In = [] };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.In)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodFormatsMultiple))]
        public void Failure_InvalidFormats(string[] timePeriods)
        {
            var query = new DataSetGetQueryTimePeriods { In = timePeriods };

            var result = _validator.TestValidate(query);

            Assert.Equal(timePeriods.Length, result.Errors.Count);

            timePeriods.ForEach((_, index) => result.ShouldHaveValidationErrorFor($"In[{index}]"));
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodQueriesMultiple))]
        public void Failure_InvalidQueries(DataSetQueryTimePeriod[] timePeriods)
        {
            var query = new DataSetGetQueryTimePeriods
            {
                In = [.. timePeriods.Select(t => t.ToTimePeriodString())]
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(timePeriods.Length, result.Errors.Count);

            timePeriods.ForEach((_, index) => result.ShouldHaveValidationErrorFor($"In[{index}]"));
        }
    }

    public class NotInTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodQueriesMultiple))]
        public void Success(DataSetQueryTimePeriod[] timePeriods)
        {
            var query = new DataSetGetQueryTimePeriods { NotIn = [.. timePeriods.Select(t => t.ToTimePeriodString())] };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetGetQueryTimePeriods { NotIn = [] };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodFormatsMultiple))]
        public void Failure_InvalidFormats(string[] timePeriods)
        {
            var query = new DataSetGetQueryTimePeriods { NotIn = timePeriods };

            var result = _validator.TestValidate(query);

            Assert.Equal(timePeriods.Length, result.Errors.Count);

            timePeriods.ForEach((_, index) => result.ShouldHaveValidationErrorFor($"NotIn[{index}]"));
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodQueriesMultiple))]
        public void Failure_InvalidQueries(DataSetQueryTimePeriod[] timePeriods)
        {
            var query = new DataSetGetQueryTimePeriods { NotIn = [.. timePeriods.Select(t => t.ToTimePeriodString())] };

            var result = _validator.TestValidate(query);

            Assert.Equal(timePeriods.Length, result.Errors.Count);

            timePeriods.ForEach((_, index) => result.ShouldHaveValidationErrorFor($"NotIn[{index}]"));
        }
    }

    public class GtTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodQueriesSingle))]
        public void Success(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Gt = timePeriod.ToTimePeriodString() };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodFormatsSingle))]
        public void Failure_InvalidFormat(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Gt = timePeriod };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Gt)
                .Only();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodQueriesSingle))]
        public void Failure_InvalidQuery(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Gt = timePeriod.ToTimePeriodString() };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Gt)
                .Only();
        }
    }

    public class GteTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodQueriesSingle))]
        public void Success(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Gte = timePeriod.ToTimePeriodString() };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodFormatsSingle))]
        public void Failure_InvalidFormat(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Gte = timePeriod };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Gte)
                .Only();
        }


        [Theory]
        [MemberData(nameof(InvalidTimePeriodQueriesSingle))]
        public void Failure_InvalidQuery(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Gte = timePeriod.ToTimePeriodString() };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Gte)
                .Only();
        }
    }

    public class LtTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodQueriesSingle))]
        public void Success(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Lt = timePeriod.ToTimePeriodString() };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodFormatsSingle))]
        public void Failure_InvalidFormat(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Lt = timePeriod };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Lt)
                .Only();
        }


        [Theory]
        [MemberData(nameof(InvalidTimePeriodQueriesSingle))]
        public void Failure_InvalidQuery(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Lt = timePeriod.ToTimePeriodString() };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Lt)
                .Only();
        }
    }

    public class LteTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodQueriesSingle))]
        public void Success(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Lte = timePeriod.ToTimePeriodString() };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodFormatsSingle))]
        public void Failure_InvalidFormat(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Lte = timePeriod };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Lte)
                .Only();
        }


        [Theory]
        [MemberData(nameof(InvalidTimePeriodQueriesSingle))]
        public void Failure_InvalidQuery(DataSetQueryTimePeriod timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Lte = timePeriod.ToTimePeriodString() };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Lte)
                .Only();
        }
    }

    public class EmptyTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Fact]
        public void AllEmpty_Failure()
        {
            var query = new DataSetGetQueryTimePeriods
            {
                Eq = "",
                NotEq = "",
                In = [],
                NotIn = [],
                Gt = "",
                Gte = "",
                Lt = "",
                Lte = ""
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(8, result.Errors.Count);

            result.ShouldHaveValidationErrorFor(q => q.Eq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.In)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.Gt)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.Gte)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.Lt)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.Lte)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void SomeEmpty_Failure()
        {
            var query = new DataSetGetQueryTimePeriods
            {
                Eq = "2020|AY",
                NotEq = "",
                In = ["2021/2022|T3"],
                NotIn = [],
                Gt = "2019|M4",
                Gte = "",
                Lt = "",
                Lte = "2030|M7"
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(4, result.Errors.Count);

            result.ShouldNotHaveValidationErrorFor(q => q.Eq);
            result.ShouldNotHaveValidationErrorFor(q => q.In);
            result.ShouldNotHaveValidationErrorFor(q => q.Gt);
            result.ShouldNotHaveValidationErrorFor(q => q.Lte);

            result.ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.NotIn)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.Gte)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.Lt)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }
}
