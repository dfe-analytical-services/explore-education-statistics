using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Builders;
using Newtonsoft.Json;
using Snapshooter.Xunit;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Analytics.Dtos;

public class CaptureTableToolDownloadCallTests
{
    [Fact]
    public void GivenACaptureTableToolDownloadCall_WhenSerialised_ThenAllPropertiesShouldBeCaptured()
    {
        // ARRANGE
        var fullTableQuery = new FullTableQueryBuilder().Build();

        var sut = new CaptureTableToolDownloadCallBuilder().WhereQueryIs(fullTableQuery).Build();

        // ACT
        var actual = AnalyticsRequestSerialiser.SerialiseRequest(sut);

        // ASSERT
        Assert.Contains(sut.ReleaseVersionId.ToString(), actual);
        Assert.Contains(sut.PublicationName, actual);
        Assert.Contains(sut.ReleasePeriodAndLabel, actual);
        Assert.Contains(sut.SubjectId.ToString(), actual);
        Assert.Contains(sut.DataSetName, actual);
        Assert.Contains(sut.DownloadFormat.ToString(), actual);

        // Make sure the serialised query is included. The correct/accurate serialisation of that model should be tested elsewhere.
        var serialisedQuery = AnalyticsRequestSerialiser.SerialiseRequest(fullTableQuery);
        AssertContainsJson(serialisedQuery, actual);
    }

    private void AssertContainsJson(string expectedJson, string actualJson) =>
        Assert.Contains(StripJsonFormatting(expectedJson), StripJsonFormatting(actualJson));

    private string StripJsonFormatting(string json) => JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json));

    /// <summary>
    /// This data is processed by the Analytics Consumer.
    /// Any changes to this contract must be reflected in the corresponding calls processor.
    /// </summary>
    [Fact]
    public void GivenACaptureTableToolDownloadCall_WhenSerialised_ThenOutputShouldBeBackwardsCompatible()
    {
        // ARRANGE
        var fullTableQuery = new FullTableQuery
        {
            LocationIds = new List<Guid>
            {
                Guid.Parse("4551768e-a997-4fae-8d9f-0d97b6b7dff0"),
                Guid.Parse("85fce0ab-2775-4300-ba4b-f62c0a7ea0cd"),
            },
            TimePeriod = new TimePeriodQuery
            {
                StartYear = 2000,
                StartCode = TimeIdentifier.AcademicYear,
                EndYear = 2001,
                EndCode = TimeIdentifier.AcademicYear,
            },
            Indicators = new List<Guid>
            {
                Guid.Parse("4510415b-4aca-478a-8c51-8f17977ab033"),
                Guid.Parse("0f235e3b-b8c6-4f7e-9ceb-2ed6538607c0"),
            },
            Filters = new List<Guid>
            {
                Guid.Parse("757dcb8e-ffa4-4bbd-a88f-dcfba8e27bc5"),
                Guid.Parse("c803e8f9-77f5-4d74-b3c6-814c5ec43ba4"),
            },
            SubjectId = Guid.Parse("5385a9db-60a9-49b8-b02c-84ce8b814711"),
            FilterHierarchiesOptions = new List<FilterHierarchyOptions>
            {
                new()
                {
                    LeafFilterId = Guid.Parse("0fc13b3b-8d6d-4ea4-a7a7-6a59c73ab205"),
                    Options =
                    [
                        [
                            Guid.Parse("d021942c-c424-4dac-b81e-0912ccc4fe38"),
                            Guid.Parse("da5134f1-ea1a-40bd-bf00-7761d35bc3ee"),
                        ],
                        [
                            Guid.Parse("70e46cb9-239c-4705-9f34-e4219872fcf9"),
                            Guid.Parse("b191b41d-c77c-4229-b3d8-74398904b52c"),
                        ],
                    ],
                },
                new()
                {
                    LeafFilterId = Guid.Parse("acd78867-3a0b-4f6a-9fc5-7f27bf5360e8"),
                    Options =
                    [
                        [
                            Guid.Parse("2d4921be-a038-45de-bd73-4f74d12bcb92"),
                            Guid.Parse("cd5bebe3-8161-4a86-8685-6ff95d9023f3"),
                        ],
                        [
                            Guid.Parse("d9cc047d-41e2-404a-aea7-8ae653e6cc7b"),
                            Guid.Parse("fc319855-ed56-4ecc-9589-dc7385dee46a"),
                        ],
                    ],
                },
            },
        };

        var sut = new CaptureTableToolDownloadCall
        {
            ReleaseVersionId = Guid.Parse("db5429d3-19a0-4ef9-b24e-02580ccad950"),
            PublicationName = "the publication name",
            ReleasePeriodAndLabel = "the release period and label",
            SubjectId = Guid.Parse("d5917257-9970-44e9-a889-c967a9e35ad6"),
            DataSetName = "the data set name",
            DownloadFormat = TableDownloadFormat.ODS,
            Query = fullTableQuery,
        };

        // ACT
        var actual = AnalyticsRequestSerialiser.SerialiseRequest(sut);

        // ASSERT
        Snapshot.Match(actual);
    }
}
