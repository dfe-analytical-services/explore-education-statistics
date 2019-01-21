using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.DataTable;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class TableBuilderService
    {
        private readonly GeographicService _geographicService;
        private readonly LaCharacteristicService _laCharacteristicService;
        private readonly NationalCharacteristicService _nationalCharacteristicService;

        public TableBuilderService(GeographicService geographicService,
            LaCharacteristicService laCharacteristicService,
            NationalCharacteristicService nationalCharacteristicService)
        {
            _geographicService = geographicService;
            _laCharacteristicService = laCharacteristicService;
            _nationalCharacteristicService = nationalCharacteristicService;
        }

        public TableToolResult GetGeographic(GeographicQueryContext query)
        {
            var data = _geographicService.FindMany(query);

            if (!data.Any())
            {
                return new TableToolResult();
            }

            var first = data.FirstOrDefault();
            return new TableToolResult
            {
                PublicationId = first.PublicationId,
                ReleaseId = first.ReleaseId,
                ReleaseDate = first.ReleaseDate,
                Result = data.Select(tidyData => DataToTableToolData(tidyData, query.Attributes))
            };
        }

        private static TableToolData DataToTableToolData(TidyData data, ICollection<string> attributeFilter)
        {
            return new TableToolData
            {
                Domain = data.Year.ToString(),
                Range = attributeFilter.Count > 0 ? FilterAttributes(data.Attributes, attributeFilter) : data.Attributes
            };
        }

        private static Dictionary<string, string> FilterAttributes(
            Dictionary<string, string> attributes,
            ICollection<string> filter)
        {
            return (
                from kvp in attributes
                where filter.Contains(kvp.Key)
                select kvp
            ).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}