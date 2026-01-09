using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.ViewModels;

public static class DataSetQueryPaginatedResultsViewModelTestExtensions
{
    public static WarningViewModel AssertHasFiltersNotFoundWarning(
        this DataSetQueryPaginatedResultsViewModel viewModel,
        string expectedPath,
        IEnumerable<string> notFoundItems
    )
    {
        var warning = viewModel.AssertHasWarning(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.FiltersNotFound.Code
        );

        var warningDetail = warning.GetDetail<NotFoundItemsErrorDetail<string>>();

        Assert.Equal(notFoundItems, warningDetail.Items);

        return warning;
    }

    public static WarningViewModel AssertHasGeographicLevelsNotFoundWarning(
        this DataSetQueryPaginatedResultsViewModel viewModel,
        string expectedPath,
        IEnumerable<string> notFoundItems
    )
    {
        var warning = viewModel.AssertHasWarning(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.GeographicLevelsNotFound.Code
        );

        var warningDetail = warning.GetDetail<NotFoundItemsErrorDetail<string>>();

        Assert.Equal(notFoundItems, warningDetail.Items);

        return warning;
    }

    public static WarningViewModel AssertHasLocationsNotFoundWarning(
        this DataSetQueryPaginatedResultsViewModel viewModel,
        string expectedPath,
        IEnumerable<string> notFoundItems
    )
    {
        var warning = viewModel.AssertHasWarning(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.LocationsNotFound.Code
        );

        var warningDetail = warning.GetDetail<NotFoundItemsErrorDetail<string>>();

        Assert.Equal(notFoundItems, warningDetail.Items);

        return warning;
    }

    public static WarningViewModel AssertHasLocationsNotFoundWarning(
        this DataSetQueryPaginatedResultsViewModel viewModel,
        string expectedPath,
        IEnumerable<IDataSetQueryLocation> notFoundItems
    )
    {
        var warning = viewModel.AssertHasWarning(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.LocationsNotFound.Code
        );

        var warningDetail = warning.GetDetail<NotFoundItemsErrorDetail<IDataSetQueryLocation>>();

        Assert.Equal(notFoundItems, warningDetail.Items);

        return warning;
    }

    public static WarningViewModel AssertHasTimePeriodsNotFoundWarning(
        this DataSetQueryPaginatedResultsViewModel viewModel,
        string expectedPath,
        IEnumerable<string> notFoundItems
    )
    {
        var warning = viewModel.AssertHasWarning(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.TimePeriodsNotFound.Code
        );

        var warningDetail = warning.GetDetail<NotFoundItemsErrorDetail<string>>();

        Assert.Equal(notFoundItems, warningDetail.Items);

        return warning;
    }

    public static WarningViewModel AssertHasTimePeriodsNotFoundWarning(
        this DataSetQueryPaginatedResultsViewModel viewModel,
        string expectedPath,
        IEnumerable<DataSetQueryTimePeriod> notFoundItems
    )
    {
        var warning = viewModel.AssertHasWarning(
            expectedPath: expectedPath,
            expectedCode: ValidationMessages.TimePeriodsNotFound.Code
        );

        var warningDetail = warning.GetDetail<NotFoundItemsErrorDetail<DataSetQueryTimePeriod>>();

        Assert.Equal(notFoundItems, warningDetail.Items);

        return warning;
    }

    public static WarningViewModel AssertHasWarning(
        this DataSetQueryPaginatedResultsViewModel viewModel,
        string expectedPath,
        string expectedCode
    )
    {
        Predicate<WarningViewModel> predicate = warning => warning.Path == expectedPath && warning.Code == expectedCode;

        Assert.Contains(viewModel.Warnings, predicate);

        return viewModel.Warnings.First(new Func<WarningViewModel, bool>(predicate));
    }
}
