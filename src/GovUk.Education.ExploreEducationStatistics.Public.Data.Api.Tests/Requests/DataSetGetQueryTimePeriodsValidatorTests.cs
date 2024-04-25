using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public abstract class DataSetGetQueryTimePeriodsValidatorTests
{
    private readonly DataSetGetQueryTimePeriods.Validator _validator = new();

    public static IEnumerable<object[]> ValidTimePeriodStringsSingle()
    {
        return
        [
            ["2020|AY"],
            ["2020/2021|AY"],
            ["2020|FY"],
            ["2021|M1"],
            ["2021|W40"],
            ["2021|T3"],
            ["2021/2022|T3"],
            ["2019|FYQ4"],
            ["2019/2020|FYQ4"],
        ];
    }

    public static IEnumerable<object[]> ValidTimePeriodStringsMultiple()
    {
        return
        [
            ["2020|AY", "2020/2021|AY", "2020|FY"],
            ["2021|M1", "2021|W40", "2021|T3"],
            ["2021/2022|T3", "2019|FYQ4", "2019/2020|FYQ4"],
        ];
    }

    public static IEnumerable<object[]> InvalidTimePeriodStringsSingle()
    {
        return
        [
            [""],
            ["Invalid"],
            ["2022"],
            ["2022/2023"],
            ["20222"],
            ["20222|AY"],
            ["2022|ay"],
            ["2022/2020|AY"],
            ["2000/1999|AY"],
            ["2022|YY"],
            ["2022|WEEK12"],
        ];
    }

    public static IEnumerable<object[]> InvalidTimePeriodStringsMultiple()
    {
        return
        [
            [],
            ["", ""],
            ["Invalid", "2022", "2022/2023"],
            ["20222", "20222|AY", "2022|ay"],
            ["2022/2020|AY", "2000/1999|AY"],
            ["2022|YY", "2022|WEEK12", "2022/2023|ZZ"]
        ];
    }

    public class EqTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodStringsSingle))]
        public void Success(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Eq = timePeriod };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodStringsSingle))]
        public void Failure_InvalidString(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Eq = timePeriod };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Eq);
        }
    }

    public class NotEqTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodStringsSingle))]
        public void Success(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { NotEq = timePeriod };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodStringsSingle))]
        public void Failure_InvalidString(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { NotEq = timePeriod };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.NotEq);
        }
    }

    public class InTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodStringsMultiple))]
        public void Success(params string[] timePeriods)
        {
            var testObj = new DataSetGetQueryTimePeriods { In = timePeriods };

            _validator.TestValidate(testObj).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodStringsMultiple))]
        public void Failure_InvalidStrings(params string[] timePeriods)
        {
            var query = new DataSetGetQueryTimePeriods { In = timePeriods };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.In);

            timePeriods.ForEach((_, index) => result.ShouldHaveValidationErrorFor($"In[{index}]"));
        }
    }

    public class NotInTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodStringsMultiple))]
        public void Success(params string[] timePeriods)
        {
            var testObj = new DataSetGetQueryTimePeriods { NotIn = timePeriods };

            _validator.TestValidate(testObj).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodStringsMultiple))]
        public void Failure_InvalidStrings(params string[] timePeriods)
        {
            var query = new DataSetGetQueryTimePeriods { NotIn = timePeriods };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.NotIn);

            timePeriods.ForEach((_, index) => result.ShouldHaveValidationErrorFor($"NotIn[{index}]"));
        }
    }

    public class GtTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodStringsSingle))]
        public void Success(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Gt = timePeriod };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodStringsSingle))]
        public void Failure_InvalidString(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Gt = timePeriod };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Gt);
        }
    }

    public class GteTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodStringsSingle))]
        public void Success(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Gte = timePeriod };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodStringsSingle))]
        public void Failure_InvalidString(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Gte = timePeriod };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Gte);
        }
    }

    public class LtTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodStringsSingle))]
        public void Success(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Lt = timePeriod };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodStringsSingle))]
        public void Failure_InvalidString(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Lt = timePeriod };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Lt);
        }
    }

    public class LteTests : DataSetGetQueryTimePeriodsValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidTimePeriodStringsSingle))]
        public void Success(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Lte = timePeriod };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(InvalidTimePeriodStringsSingle))]
        public void Failure_InvalidString(string timePeriod)
        {
            var query = new DataSetGetQueryTimePeriods { Lte = timePeriod };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Lte);
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
