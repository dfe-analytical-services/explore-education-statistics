using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Requests;

public class RecordPermalinkTableDownloadRequestBindingModelTests
{
    [Theory]
    [MemberData(nameof(InvalidBindingModels))]
    public void GivenAnIncompleteBindingModel_WhenValidated_ThenShouldFail(
        RecordPermalinkTableDownloadRequestBindingModel incompleteModel)
    {
        // ARRANGE
        var validator = new RecordPermalinkTableDownloadRequestBindingModel.Validator();
        
        // ACT
        var result = validator.Validate(incompleteModel);
        
        // ASSERT
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void GivenAFullyPopulatedBindingModel_WhenValidated_ThenShouldSucceed()
    {
        // ARRANGE
        var validator = new RecordPermalinkTableDownloadRequestBindingModel.Validator();
        
        // ACT
        var result = validator.Validate(FullyPopulatedModel());
        
        // ASSERT
        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }

    public static TheoryData<RecordPermalinkTableDownloadRequestBindingModel> InvalidBindingModels() =>
        new(
            new RecordPermalinkTableDownloadRequestBindingModel(),
            FullyPopulatedModel() with { PermalinkTitle = null },
            FullyPopulatedModel() with { PermalinkTitle = string.Empty },
            FullyPopulatedModel() with { PermalinkId = Guid.Empty },
            FullyPopulatedModel() with { PermalinkId = null },
            FullyPopulatedModel() with { DownloadFormat = null },
            FullyPopulatedModel() with { DownloadFormat = (TableDownloadFormat)99 }
            );

    public static RecordPermalinkTableDownloadRequestBindingModel FullyPopulatedModel() =>
        new()
        {
            PermalinkTitle = "perma link title",
            PermalinkId = Guid.NewGuid(),
            DownloadFormat = TableDownloadFormat.ODS
        };
}
