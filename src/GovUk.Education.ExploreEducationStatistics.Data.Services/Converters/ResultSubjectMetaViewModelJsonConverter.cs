#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Converters
{
    /// <summary>
    /// JsonConverter which transforms the legacy 'Locations' field of ResultSubjectMetaViewModel to 'LocationsHierarchical'.
    ///
    /// Intended to be temporary until this transformation is made permanent by migrating old Permalinks in blob storage (EES-2943).
    ///
    /// Note that this doesn't infer any hierarchy so the location attributes will continue to be single level
    /// meaning there's no overall change to the data of Permalinks except for moving it between fields and transformation of format.
    ///
    /// Permalinks created prior to the Table Result Subject Metadata changes of EES-2881 (BE) and EES-2777 (FE) have a
    /// flat 'Locations' field in their JSON serialization of type <see cref="List{ObservationalUnitMetaViewModel}"/>.
    ///
    /// Permalinks created afterwards, plus any created while the dedicated release toggle for that feature was turned on,
    /// have locations in field <see cref="ResultSubjectMetaViewModel.LocationsHierarchical">ResultSubjectMetaViewModel.LocationsHierarchical</see>.
    ///
    /// Until old Permalinks are migrated, the translation provided by this converter ensures that consumers
    /// are aware of legacy locations when accessing <see cref="ResultSubjectMetaViewModel.LocationsHierarchical" />.
    /// </summary>
    public class ResultSubjectMetaViewModelJsonConverter : JsonConverter<ResultSubjectMetaViewModel>
    {
        public override bool CanWrite => false;

        public override ResultSubjectMetaViewModel ReadJson(JsonReader reader,
            Type objectType,
            ResultSubjectMetaViewModel? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            var tableSubjectMeta = token.ToObject<ResultSubjectMetaViewModel>();

            if (tableSubjectMeta == null)
            {
                return null!;
            }

            if (!tableSubjectMeta.LocationsHierarchical.Any())
            {
                // Get the legacy locations field which exists as a property in Json of older Permalinks
                var locationsToken = token.SelectToken("Locations");
                if (locationsToken is {Type: JTokenType.Array})
                {
                    var locations = locationsToken.ToObject<List<ObservationalUnitMetaViewModel>>();
                    if (locations != null && locations.Any())
                    {
                        tableSubjectMeta.LocationsHierarchical = TransformLocations(locations);
                    }
                }
            }

            return tableSubjectMeta;
        }

        public override void WriteJson(JsonWriter writer,
            ResultSubjectMetaViewModel? value,
            JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }

        private static Dictionary<string, List<LocationAttributeViewModel>> TransformLocations(
            List<ObservationalUnitMetaViewModel> locations)
        {
            var result = locations
                .GroupBy(location => location.Level)
                .ToDictionary(pair => pair.Key.ToString().CamelCase(),
                    pair => pair.ToList().Select(
                        attribute => new LocationAttributeViewModel
                        {
                            GeoJson = attribute.GeoJson,
                            Label = attribute.Label,
                            Value = attribute.Value
                        }).ToList());
            return result;
        }
    }
}
