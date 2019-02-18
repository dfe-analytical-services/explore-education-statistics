using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class TableBuilderService
    {
        private readonly GeographicDataService _geographicDataService;
        private readonly LaCharacteristicService _laCharacteristicService;
        private readonly NationalCharacteristicService _nationalCharacteristicService;
        private readonly GeographicResultBuilder _geographicResultBuilder;
        private readonly CharacteristicResultBuilder _characteristicResultBuilder;

        public TableBuilderService(GeographicDataService geographicDataService,
            LaCharacteristicService laCharacteristicService,
            NationalCharacteristicService nationalCharacteristicService,
            GeographicResultBuilder geographicResultBuilder,
            CharacteristicResultBuilder characteristicResultBuilder)
        {
            _geographicDataService = geographicDataService;
            _laCharacteristicService = laCharacteristicService;
            _nationalCharacteristicService = nationalCharacteristicService;
            _geographicResultBuilder = geographicResultBuilder;
            _characteristicResultBuilder = characteristicResultBuilder;
        }

        public TableBuilderResult GetGeographic(GeographicQueryContext query)
        {
            return BuildResult(_geographicDataService.FindMany(query), query.Attributes, _geographicResultBuilder);
        }

        public TableBuilderResult GetLocalAuthority(LaQueryContext query)
        {
            return BuildResult(_laCharacteristicService.FindMany(query), query.Attributes,
                _characteristicResultBuilder);
        }

        public TableBuilderResult GetNational(NationalQueryContext query)
        {
            return BuildResult(_nationalCharacteristicService.FindMany(query), query.Attributes,
                _characteristicResultBuilder);
        }
        
        private static TableBuilderResult BuildResult(IEnumerable<IGeographicData> data, ICollection<string> attributes,
            IResultBuilder<IGeographicData, ITableBuilderData> resultBuilder)
        {
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
                Level = first.Level,
                Result = data.Select(tidyData => resultBuilder.BuildResult(tidyData, attributes))
            };
        }

        private static TableBuilderResult BuildResult(IEnumerable<ICharacteristicData> data,
            ICollection<string> attributes,
            IResultBuilder<ICharacteristicData, ITableBuilderData> resultBuilder)
        {
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
                Level = first.Level,
                Result = data.Select(tidyData => resultBuilder.BuildResult(tidyData, attributes))
            };
        }
    }
}