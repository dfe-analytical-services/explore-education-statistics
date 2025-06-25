namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.TheoryDataHelpers;

public static class TheoryDatas
{
    public static class Blank
    {
        public static TheoryData<string?> Strings => [null, string.Empty];
        public static TheoryData<Guid?> Guids => [null, Guid.Empty];
        public static TheoryData<bool?> Bools => [null, false];
    }
}
