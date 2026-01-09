using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Requests;

public class RecordTableToolDownloadRequestBindingModelTests
{
    [Theory]
    [MemberData(nameof(InvalidBindingModels))]
    public void GivenInvalidBindingModel_WhenValidated_ThenResultIsInvalid(
        RecordTableToolDownloadRequestBindingModel invalidModel
    )
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

    [Fact]
    public void GivenValidBindingModel_WhenConvertedToModel_ThenResultIsCorrect()
    {
        // ARRANGE
        var bindingModel = new RecordTableToolDownloadRequestBindingModel()
        {
            DataSetName = "data set name",
            DownloadFormat = TableDownloadFormat.ODS,
            PublicationName = "Publication Name",
            ReleasePeriodAndLabel = "release period label",
            ReleaseVersionId = Guid.Parse("5c015e8a-772f-4450-9fe0-8e83f73808ee"),
            SubjectId = Guid.Parse("cbdb1ea4-9ff1-4f45-86be-c5918b28735c"),
            Query = new FullTableQueryRequestBuilder().Build(),
        };

        // ACT
        var actual = bindingModel.ToModel();

        // ASSERT
        var expected = new CaptureTableToolDownloadCall
        {
            DataSetName = "data set name",
            DownloadFormat = TableDownloadFormat.ODS,
            PublicationName = "Publication Name",
            ReleasePeriodAndLabel = "release period label",
            ReleaseVersionId = Guid.Parse("5c015e8a-772f-4450-9fe0-8e83f73808ee"),
            SubjectId = Guid.Parse("cbdb1ea4-9ff1-4f45-86be-c5918b28735c"),
            Query = bindingModel.Query.AsFullTableQuery(),
        };
        Assert.Equivalent(expected, actual);
    }

    public static TheoryData<RecordTableToolDownloadRequestBindingModel> InvalidBindingModels() =>
        new(
            new RecordTableToolDownloadRequestBindingModel(),
            FullyPopulatedModel() with
            {
                DataSetName = null,
            },
            FullyPopulatedModel() with
            {
                DataSetName = string.Empty,
            },
            FullyPopulatedModel() with
            {
                DownloadFormat = null,
            },
            FullyPopulatedModel() with
            {
                DownloadFormat = (TableDownloadFormat)99,
            },
            FullyPopulatedModel() with
            {
                PublicationName = null,
            },
            FullyPopulatedModel() with
            {
                PublicationName = string.Empty,
            },
            FullyPopulatedModel() with
            {
                ReleasePeriodAndLabel = null,
            },
            FullyPopulatedModel() with
            {
                ReleasePeriodAndLabel = string.Empty,
            },
            FullyPopulatedModel() with
            {
                ReleaseVersionId = null,
            },
            FullyPopulatedModel() with
            {
                ReleaseVersionId = Guid.Empty,
            },
            FullyPopulatedModel() with
            {
                SubjectId = null,
            },
            FullyPopulatedModel() with
            {
                SubjectId = Guid.Empty,
            },
            FullyPopulatedModel() with
            {
                Query = null,
            }
        );

    private static RecordTableToolDownloadRequestBindingModel FullyPopulatedModel() =>
        new RecordTableToolDownloadRequestBindingModelBuilder().Build();
}
