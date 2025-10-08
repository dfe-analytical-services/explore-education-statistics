using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class TimePeriodMetaGeneratorExtensions
{
    private const int DefaultStartYear = 2000;
    private const TimeIdentifier DefaultCode = TimeIdentifier.AcademicYear;

    public static Generator<TimePeriodMeta> DefaultTimePeriodMeta(this DataFixture fixture) =>
        fixture.Generator<TimePeriodMeta>().WithDefaults();

    public static Generator<TimePeriodMeta> WithDefaults(
        this Generator<TimePeriodMeta> generator,
        TimeIdentifier code = DefaultCode,
        int startYear = DefaultStartYear
    ) => generator.ForInstance(s => s.SetDefaults(code: code, startYear: startYear));

    public static Generator<TimePeriodMeta> WithDataSetVersion(
        this Generator<TimePeriodMeta> generator,
        DataSetVersion dataSetVersion
    ) => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<TimePeriodMeta> WithDataSetVersionId(
        this Generator<TimePeriodMeta> generator,
        Guid dataSetVersionId
    ) => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<TimePeriodMeta> WithCode(this Generator<TimePeriodMeta> generator, TimeIdentifier code) =>
        generator.ForInstance(s => s.SetCode(code));

    public static Generator<TimePeriodMeta> WithPeriod(this Generator<TimePeriodMeta> generator, string period) =>
        generator.ForInstance(s => s.SetPeriod(period));

    public static InstanceSetters<TimePeriodMeta> SetDefaults(
        this InstanceSetters<TimePeriodMeta> setters,
        TimeIdentifier code = DefaultCode,
        int startYear = DefaultStartYear
    ) => setters.Set(m => m.Code, code).Set(m => m.Period, (_, _, context) => (startYear + context.Index).ToString());

    public static InstanceSetters<TimePeriodMeta> SetDataSetVersion(
        this InstanceSetters<TimePeriodMeta> setters,
        DataSetVersion dataSetVersion
    ) => setters.Set(m => m.DataSetVersion, dataSetVersion).SetDataSetVersionId(dataSetVersion.Id);

    public static InstanceSetters<TimePeriodMeta> SetDataSetVersionId(
        this InstanceSetters<TimePeriodMeta> setters,
        Guid dataSetVersionId
    ) => setters.Set(m => m.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<TimePeriodMeta> SetCode(
        this InstanceSetters<TimePeriodMeta> setters,
        TimeIdentifier code
    ) => setters.Set(m => m.Code, code);

    public static InstanceSetters<TimePeriodMeta> SetPeriod(
        this InstanceSetters<TimePeriodMeta> setters,
        string period
    ) => setters.Set(m => m.Period, period);
}
