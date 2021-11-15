using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    /// <summary>
    /// Marker interface allowing two different types of SubjecMetaViewModels to exist while we are implementing
    /// SOW8 changes.
    /// </summary>
    public interface ISubjectMetaViewModel
    {
    }

    public abstract record AbstractSubjectMetaViewModel<TLocations> 
        : ISubjectMetaViewModel where TLocations : ILocationsMetaViewModel
    {
        public Dictionary<string, FilterMetaViewModel> Filters { get; set; }

        public Dictionary<string, IndicatorsMetaViewModel> Indicators { get; set; }

        public Dictionary<string, TLocations> Locations { get; set; }

        public TimePeriodsMetaViewModel TimePeriod { get; set; }
    }

    /// <summary>
    /// Legacy SubjectMetaViewModel to be removed
    /// </summary>
    [ObsoleteAttribute("TODO EES-2902 - Remove with SOW8 after EES-2773", false)]
    public record LegacySubjectMetaViewModel : AbstractSubjectMetaViewModel<ObservationalUnitsMetaViewModel>;

    /// <summary>
    /// SubjectMetaViewModel supporting hierarchical locations
    /// </summary>
    public record SubjectMetaViewModel : AbstractSubjectMetaViewModel<LocationsMetaViewModel>;
}
