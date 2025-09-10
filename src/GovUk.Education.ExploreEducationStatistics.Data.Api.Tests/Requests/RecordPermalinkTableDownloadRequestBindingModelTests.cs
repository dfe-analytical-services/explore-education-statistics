using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
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
    
    [Fact]
    public void GivenAFullyPopulatedBindingModel_WhenConvertedToModel_ThenShouldSucceed()
    {
        // ARRANGE
        var bindingModel = new RecordPermalinkTableDownloadRequestBindingModel
        {
            PermalinkTitle = "perma link title",
            PermalinkId = Guid.Parse("4e3f5743-fb44-4183-950c-39f4032dd174"),
            DownloadFormat = TableDownloadFormat.ODS
        };
        
        // ACT
        var actual = bindingModel.ToModel();
        
        // ASSERT
        var expected = new CapturePermaLinkTableDownloadCall
        {
            PermalinkTitle = "perma link title",
            PermalinkId = Guid.Parse("4e3f5743-fb44-4183-950c-39f4032dd174"),
            DownloadFormat = TableDownloadFormat.ODS
        };
        Assert.Equal(expected, actual);
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
