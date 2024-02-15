using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class TimePeriodOptionMetaGeneratorExtensions
{
    private const int DefaultStartYear = 2000;
    private const TimeIdentifier DefaultCode = TimeIdentifier.AcademicYear;

    public static Generator<TimePeriodOptionMeta> DefaultTimePeriodOptionMeta(this DataFixture fixture)
        => fixture.Generator<TimePeriodOptionMeta>().WithDefaults();

    public static Generator<TimePeriodOptionMeta> WithDefaults(
        this Generator<TimePeriodOptionMeta> generator,
        TimeIdentifier code = DefaultCode,
        int startYear = DefaultStartYear)
        => generator.ForInstance(s => s.SetDefaults(code: code, startYear: startYear));

    public static Generator<TimePeriodOptionMeta> WithCode(this Generator<TimePeriodOptionMeta> generator, TimeIdentifier code)
        => generator.ForInstance(s => s.SetCode(code));

    public static Generator<TimePeriodOptionMeta> WithYear(this Generator<TimePeriodOptionMeta> generator, int year)
        => generator.ForInstance(s => s.SetYear(year));

    public static InstanceSetters<TimePeriodOptionMeta> SetDefaults(
        this InstanceSetters<TimePeriodOptionMeta> setters,
        TimeIdentifier code = DefaultCode,
        int startYear = DefaultStartYear)
        => setters
            .Set(m => m.Code, code)
            .Set(m => m.Year, (_, _, context) => startYear + context.Index);

    public static InstanceSetters<TimePeriodOptionMeta> SetCode(
        this InstanceSetters<TimePeriodOptionMeta> setters,
        TimeIdentifier code)
        => setters.Set(m => m.Code, code);

    public static InstanceSetters<TimePeriodOptionMeta> SetYear(this InstanceSetters<TimePeriodOptionMeta> setters, int year)
        => setters.Set(m => m.Year, year);
}
