#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests;

// TODO EES-3755 Remove after Permalink snapshot migration work is complete
internal static class ObservationViewModelBuilderTestUtils
{
    public static ObservationViewModel BuildObservationViewModel(
        Observation observation,
        IEnumerable<Indicator> indicators,
        ObservationViewModelTestBuildStrategy buildStrategy)
    {
        // Build the view model with location id as normal
        var viewModel = ObservationViewModelBuilder.BuildObservation(observation, indicators.Select(i => i.Id));

        // Depending on the build strategy to test permalinks built with different states of location data,
        // we might want to add or remove the location id from the view model and/or add a location object
        switch (buildStrategy)
        {
            case ObservationViewModelTestBuildStrategy.WithLocationIdOnly:
                break;
            case ObservationViewModelTestBuildStrategy.WithLocationIdAndLocationObject:
                viewModel.Location = observation.Location.AsLocationViewModel();
                break;
            case ObservationViewModelTestBuildStrategy.WithLocationObjectOnly:
                viewModel.Location = observation.Location.AsLocationViewModel();

                // Remove the location id
                // Ideally location id should have been made nullable to match the possibility that it can be null in permalink data
                // but we'll leave it for now given that this possibility is about to disappear with the snapshot migration.
                viewModel.LocationId = Guid.Empty;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(buildStrategy), buildStrategy, null);
        }

        return viewModel;
    }
}

internal enum ObservationViewModelTestBuildStrategy
{
    /// <summary>
    /// Build the observation view model with a location id as normal.
    /// </summary>
    WithLocationIdOnly,

    /// <summary>
    /// Build the observation view model without a location id and include the location object instead.
    /// </summary>
    WithLocationObjectOnly,

    /// <summary>
    /// Build the observation view model with a location id and also include the location object.
    /// </summary>
    WithLocationIdAndLocationObject
}
