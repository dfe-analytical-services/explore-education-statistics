using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;

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

        public TableBuilderResult GetGeographic(GeographicQueryContext query)
        {
            var data = _geographicService.FindMany(query);

            if (!data.Any())
            {
                return new TableBuilderResult();
            }

            var first = data.FirstOrDefault();
            return new TableBuilderResult
            {
                PublicationId = first.PublicationId,
                ReleaseId = first.ReleaseId,
                ReleaseDate = first.ReleaseDate,
                Result = data.Select(tidyData => DataToTableBuilderData(tidyData, query.Attributes))
            };
        }

        public TableBuilderResult GetLocalAuthority(LaQueryContext query)
        {
            var data = _laCharacteristicService.FindMany(query);

            if (!data.Any())
            {
                return new TableBuilderResult();
            }

            var first = data.FirstOrDefault();
            return new TableBuilderResult
            {
                PublicationId = first.PublicationId,
                ReleaseId = first.ReleaseId,
                ReleaseDate = first.ReleaseDate,
                Result = data.Select(tidyData => DataToTableBuilderData(tidyData, query.Attributes))
            };
        }

        public TableBuilderResult GetNational(NationalQueryContext query)
        {
            var data = _nationalCharacteristicService.FindMany(query);

            if (!data.Any())
            {
                return new TableBuilderResult();
            }

            var first = data.FirstOrDefault();
            return new TableBuilderResult
            {
                PublicationId = first.PublicationId,
                ReleaseId = first.ReleaseId,
                ReleaseDate = first.ReleaseDate,
                Result = data.Select(tidyData => DataToTableBuilderData(tidyData, query.Attributes))
            };
        }

        private static TableBuilderData DataToTableBuilderData(TidyData data, ICollection<string> attributeFilter)
        {
            return new TableBuilderData
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