using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class TimePeriodMetaGeneratorExtensions
{
    private const int DefaultStartYear = 2015;
    private const TimeIdentifier DefaultCode = TimeIdentifier.AcademicYear;

    public static Generator<TimePeriodMeta> DefaultTimePeriodMeta(this DataFixture fixture)
        => fixture.Generator<TimePeriodMeta>().WithDefaults();

    public static Generator<TimePeriodMeta> WithDefaults(
        this Generator<TimePeriodMeta> generator,
        TimeIdentifier code = DefaultCode,
        int startYear = DefaultStartYear)
        => generator.ForInstance(s => s.SetDefaults(code: code, startYear: startYear));

    public static Generator<TimePeriodMeta> WithCode(this Generator<TimePeriodMeta> generator, TimeIdentifier code)
        => generator.ForInstance(s => s.SetCode(code));

    public static Generator<TimePeriodMeta> WithYear(this Generator<TimePeriodMeta> generator, int year)
        => generator.ForInstance(s => s.SetYear(year));

    public static InstanceSetters<TimePeriodMeta> SetDefaults(
        this InstanceSetters<TimePeriodMeta> setters,
        TimeIdentifier code = DefaultCode,
        int startYear = DefaultStartYear)
        => setters
            .Set(m => m.Code, code)
            .Set(m => m.Year, (_, _, context) => startYear + context.Index);

    public static InstanceSetters<TimePeriodMeta> SetCode(
        this InstanceSetters<TimePeriodMeta> setters,
        TimeIdentifier code)
        => setters.Set(m => m.Code, code);

    public static InstanceSetters<TimePeriodMeta> SetYear(this InstanceSetters<TimePeriodMeta> setters, int year)
        => setters.Set(m => m.Year, year);
}
