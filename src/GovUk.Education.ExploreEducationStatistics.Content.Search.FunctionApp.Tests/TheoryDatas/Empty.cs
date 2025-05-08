namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDatas;

public class Empty
{
    public static TheoryData<string?> StringValues => [null, string.Empty];
    public static TheoryData<Guid?> GuidValues => [null, Guid.Empty];
}
