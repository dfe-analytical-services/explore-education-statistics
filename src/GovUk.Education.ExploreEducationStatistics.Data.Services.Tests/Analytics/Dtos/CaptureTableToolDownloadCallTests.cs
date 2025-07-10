using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Analytics.Dtos;

public class CaptureTableToolDownloadCallTests
{
    [Fact]
    public void GivenACaptureTableToolDownloadCall_WhenSerialised_ThenAllPropertiesShouldBeCaptured()
    {
        // ARRANGE
        var fullTableQuery = new FullTableQuery
        {
            LocationIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            TimePeriod = new TimePeriodQuery
            {
                StartYear = 2000,
                StartCode = TimeIdentifier.AcademicYear,
                EndYear = 2001,
                EndCode = TimeIdentifier.AcademicYear,
            },
            Indicators = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            Filters = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            SubjectId = Guid.NewGuid(),
            FilterHierarchiesOptions = new List<FilterHierarchyOptions>
            {
                new()
                {
                    LeafFilterId = Guid.NewGuid(), 
                    Options = [[Guid.NewGuid(), Guid.NewGuid()], [Guid.NewGuid(), Guid.NewGuid()]],
                },
                new()
                {
                    LeafFilterId = Guid.NewGuid(), 
                    Options = [[Guid.NewGuid(), Guid.NewGuid()], [Guid.NewGuid(), Guid.NewGuid()]],
                }
            }
        };
        
        var sut = new CaptureTableToolDownloadCall
        {
            ReleaseVersionId = Guid.NewGuid(),
            PublicationName = "the publication name",
            ReleasePeriodAndLabel = "the release period and label",
            SubjectId = Guid.NewGuid(),
            DataSetName = "the data set name",
            DownloadFormat = TableDownloadFormat.ODS,
            Query = fullTableQuery
        };
        
        // ACT
        var actual = AnalyticsRequestSerialiser.SerialiseRequest(sut);

        // ASSERT
        Assert.Contains(sut.ReleaseVersionId.ToString(), actual);
        Assert.Contains(sut.PublicationName, actual);
        Assert.Contains(sut.ReleasePeriodAndLabel, actual);
        Assert.Contains(sut.SubjectId.ToString(), actual);
        Assert.Contains(sut.DataSetName, actual);
        Assert.Contains(sut.DownloadFormat.ToString(), actual);
        
        // Make sure the query is included. The serialisation of that is tested elsewhere.
        var serialisedQuery = AnalyticsRequestSerialiser.SerialiseRequest(fullTableQuery);
        AssertContainsJson(serialisedQuery, actual);
    }

    private void AssertContainsJson(string expectedJson, string actualJson) => 
        Assert.Contains(StripJsonFormatting(expectedJson), StripJsonFormatting(actualJson));

    private string StripJsonFormatting(string json) =>
        JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json));
}
