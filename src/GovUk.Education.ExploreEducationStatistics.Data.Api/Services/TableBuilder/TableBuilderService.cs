using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class TableBuilderService : ITableBuilderService
    {
        private readonly IGeographicDataService _geographicDataService;
        private readonly ILaCharacteristicDataService _laCharacteristicDataService;
        private readonly INationalCharacteristicDataService _nationalCharacteristicDataService;
        private readonly GeographicResultBuilder _geographicResultBuilder;
        private readonly CharacteristicResultBuilder _characteristicResultBuilder;
        private readonly LaCharacteristicResultBuilder _laCharacteristicResultBuilder;

        public TableBuilderService(IGeographicDataService geographicDataService,
            ILaCharacteristicDataService laCharacteristicDataService,
            INationalCharacteristicDataService nationalCharacteristicDataService,
            GeographicResultBuilder geographicResultBuilder,
            CharacteristicResultBuilder characteristicResultBuilder,
            LaCharacteristicResultBuilder laCharacteristicResultBuilder)
        {
            _geographicDataService = geographicDataService;
            _laCharacteristicDataService = laCharacteristicDataService;
            _nationalCharacteristicDataService = nationalCharacteristicDataService;
            _geographicResultBuilder = geographicResultBuilder;
            _characteristicResultBuilder = characteristicResultBuilder;
            _laCharacteristicResultBuilder = laCharacteristicResultBuilder;
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
                _laCharacteristicResultBuilder);
        }

        public TableBuilderResult GetNational(NationalQueryContext query)
        {
            return BuildResult(
                _nationalCharacteristicDataService.FindMany(query.FindExpression()),
                query.Attributes,
                _characteristicResultBuilder);
        }

        private static TableBuilderResult BuildResult(IEnumerable<IGeographicSchoolData> data,
            ICollection<string> attributes,
            IResultBuilder<IGeographicSchoolData, ITableBuilderData> resultBuilder)
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

        private static TableBuilderResult BuildResult(IEnumerable<ICharacteristicGeographicData> data,
            ICollection<string> attributes,
            IResultBuilder<ICharacteristicGeographicData, ITableBuilderData> resultBuilder)
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