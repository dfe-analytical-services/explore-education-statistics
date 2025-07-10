using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Requests;

public class RecordTableToolDownloadRequestBindingModelTests
{
    [Theory]
    [MemberData(nameof(InvalidBindingModels))]
    public void GivenInvalidBindingModel_WhenValidated_ThenResultIsInvalid(RecordTableToolDownloadRequestBindingModel invalidModel)
    {
        // ARRANGE
        var validator = new RecordTableToolDownloadRequestBindingModel.Validator();
        
        // ACT
        var result = validator.Validate(invalidModel);
        
        // ASSERT
        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void GivenValidBindingModel_WhenValidated_ThenResultIsValid()
    {
        // ARRANGE
        var validator = new RecordTableToolDownloadRequestBindingModel.Validator();
        
        // ACT
        var result = validator.Validate(FullyPopulatedModel());
        
        // ASSERT
        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    
    public static TheoryData<RecordTableToolDownloadRequestBindingModel> InvalidBindingModels() =>
        new(
            new RecordTableToolDownloadRequestBindingModel(),
            FullyPopulatedModel() with { DataSetName = null },
            FullyPopulatedModel() with { DataSetName = string.Empty },
            FullyPopulatedModel() with { DownloadFormat = null },
            FullyPopulatedModel() with { DownloadFormat = (TableDownloadFormat)99 },
            FullyPopulatedModel() with { PublicationName = null },
            FullyPopulatedModel() with { PublicationName = string.Empty },
            FullyPopulatedModel() with { ReleasePeriodAndLabel = null },
            FullyPopulatedModel() with { ReleasePeriodAndLabel = string.Empty },
            FullyPopulatedModel() with { ReleaseVersionId = null },
            FullyPopulatedModel() with { ReleaseVersionId = Guid.Empty },
            FullyPopulatedModel() with { SubjectId = null },
            FullyPopulatedModel() with { SubjectId = Guid.Empty },
            FullyPopulatedModel() with { Query = null }
        );

    private static RecordTableToolDownloadRequestBindingModel FullyPopulatedModel() =>
        new RecordTableToolDownloadRequestBindingModelBuilder().Build();
}
