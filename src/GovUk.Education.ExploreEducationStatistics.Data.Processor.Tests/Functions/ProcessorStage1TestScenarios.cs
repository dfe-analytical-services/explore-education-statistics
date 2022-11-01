using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Functions;

public class SmallCsvStage1Scenario : IProcessorStage1TestScenario
{
    private readonly Guid _subjectId;

    public SmallCsvStage1Scenario(Guid? subjectId = null)
    {
        _subjectId = subjectId ?? Guid.NewGuid();
    }

    public string GetFilenameUnderTest()
    {
        return "small-csv.csv";
    }

    public Guid GetSubjectId()
    {
        return _subjectId;
    }

    public int GetExpectedTotalRows()
    {
        return 16;
    }
}

public interface IProcessorStage1TestScenario
{
    string GetFilenameUnderTest();

    Guid GetSubjectId();

    int GetExpectedTotalRows();
}