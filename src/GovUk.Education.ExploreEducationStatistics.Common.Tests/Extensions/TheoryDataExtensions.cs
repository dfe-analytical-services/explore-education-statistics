using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class TheoryDataExtensions
{
    public static TheoryData<T1, T2> Cross<T1, T2>(
        this TheoryData<T1, T2> theoryData,
        IEnumerable<T1> testDataCollection1,
        IEnumerable<T2> testDataCollection2)
    {
        foreach (var testDataParameter1 in testDataCollection1)
        {
            foreach (var testDataParameter2 in testDataCollection2)
            {
                theoryData.Add(testDataParameter1, testDataParameter2);
            }
        }

        return theoryData;
    }
}
