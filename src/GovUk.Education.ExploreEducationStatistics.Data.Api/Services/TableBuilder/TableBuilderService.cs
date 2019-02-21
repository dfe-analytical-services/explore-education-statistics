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
        private readonly LaCharacteristicDataService _laCharacteristicDataService;
        private readonly NationalCharacteristicDataService _nationalCharacteristicDataService;
        private readonly GeographicResultBuilder _geographicResultBuilder;
        private readonly CharacteristicResultBuilder _characteristicResultBuilder;

        public TableBuilderService(GeographicDataService geographicDataService,
            LaCharacteristicDataService laCharacteristicDataService,
            NationalCharacteristicDataService nationalCharacteristicDataService,
            GeographicResultBuilder geographicResultBuilder,
            CharacteristicResultBuilder characteristicResultBuilder)
        {
            _geographicDataService = geographicDataService;
            _laCharacteristicDataService = laCharacteristicDataService;
            _nationalCharacteristicDataService = nationalCharacteristicDataService;
            _geographicResultBuilder = geographicResultBuilder;
            _characteristicResultBuilder = characteristicResultBuilder;
        }

        public TableBuilderResult GetGeographic(GeographicQueryContext query)
        {
            return BuildResult(
                _geographicDataService.FindMany(query.FindExpression()),
                query.Attributes,
                _geographicResultBuilder);
        }

        public TableBuilderResult GetLocalAuthority(LaQueryContext query)
        {
            return BuildResult(
                _laCharacteristicDataService.FindMany(query.FindExpression()),
                query.Attributes,
                _characteristicResultBuilder);
        }

        public TableBuilderResult GetNational(NationalQueryContext query)
        {
            return BuildResult(
                _nationalCharacteristicDataService.FindMany(query.FindExpression()),
                query.Attributes,
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