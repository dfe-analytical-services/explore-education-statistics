#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    /// <summary>
    /// Marker interface allowing two different types of LocationsMetaViewModels to exist while we are implementing
    /// SOW8 changes.
    /// </summary>
    public interface ILocationsMetaViewModel
    {
    }

    public record LocationsMetaViewModel : ILocationsMetaViewModel
    {
        public string Legend { get; init; } = string.Empty;
        public List<LocationAttributeViewModel> Options { get; init; } = new();
    }

    public record LocationAttributeViewModel
    {
        public string Label { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public string? Level { get; init; }
        public List<LocationAttributeViewModel>? Options { get; init; }
    }

    /// <summary>
    /// Legacy locations view model returned in Subject meta data - to be removed
    /// </summary>
    [ObsoleteAttribute("TODO EES-2902 - Remove with SOW8 after EES-2773", false)]
    public class ObservationalUnitsMetaViewModel :
        LegendOptionsMetaValueModel<IEnumerable<LabelValue>>, ILocationsMetaViewModel
    {
    }
}
