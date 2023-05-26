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
    public static ObservationViewModel BuildObservationViewModelWithoutLocationId(
        Observation observation,
        IEnumerable<Indicator> indicators)
    {
        // Build the view model with location id as normal
        var viewModel = ObservationViewModelBuilder.BuildObservation(observation, indicators.Select(i => i.Id));

        // Swap out the location id for a location view model to represent how permalink data would be like
        // with observations that had a location object prior to location id being added and location being removed.

        // Ideally location id should have been made nullable to match the possibility that it can be null in permalink data
        // but we'll leave it for now given that this possibility is about to disappear with the snapshot migration.
        viewModel.LocationId = Guid.Empty;

        viewModel.Location = observation.Location.AsLocationViewModel();
        return viewModel;
    }

    public static ObservationViewModel BuildObservationViewModelWithLocationIdAndLocation(
        Observation observation,
        IEnumerable<Indicator> indicators)
    {
        // Build the view model with location id as normal
        var viewModel = ObservationViewModelBuilder.BuildObservation(observation, indicators.Select(i => i.Id));

        // Add the location view model object to represent how the permalink data would be like with observations
        // when location id was added but before location was removed.
        viewModel.Location = observation.Location.AsLocationViewModel();

        return viewModel;
    }
}
