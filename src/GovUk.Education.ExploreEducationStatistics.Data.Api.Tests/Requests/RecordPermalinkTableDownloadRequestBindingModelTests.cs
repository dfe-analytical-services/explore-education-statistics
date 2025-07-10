using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics;
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
    
    public static RecordTableToolDownloadRequestBindingModel FullyPopulatedModel() =>
        new()
        {
           DataSetName = "data set name",
           DownloadFormat = TableDownloadFormat.ODS,
           PublicationName = "Publication Name",
           ReleasePeriodAndLabel = "release period label",
           ReleaseVersionId = Guid.NewGuid(),
           SubjectId = Guid.NewGuid(),
           Query = new FullTableQueryRequest
           {
               SubjectId = Guid.NewGuid(),
               Filters = [ Guid.NewGuid(), Guid.NewGuid() ],
               Indicators = [ Guid.NewGuid(), Guid.NewGuid() ],
               LocationIds = [ Guid.NewGuid(), Guid.NewGuid() ],
               TimePeriod = new TimePeriodQuery
               {
                   StartYear = 2025,
                   StartCode = TimeIdentifier.July,
                   EndYear = 2026,
                   EndCode = TimeIdentifier.November
               },
               FilterHierarchiesOptions = new Dictionary<Guid, List<FilterHierarchyOption>>
               {
                   { Guid.NewGuid(), [ new FilterHierarchyOption([Guid.NewGuid(), Guid.NewGuid()])]},
                   { Guid.NewGuid(), [ new FilterHierarchyOption([Guid.NewGuid(), Guid.NewGuid()])]},
               }
           }
        };
}

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
