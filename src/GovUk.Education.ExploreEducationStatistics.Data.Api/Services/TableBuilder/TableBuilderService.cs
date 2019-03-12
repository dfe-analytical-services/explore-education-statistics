using System;
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
            return BuildResult(_geographicDataService, query, _geographicResultBuilder);
        }

        public TableBuilderResult GetLocalAuthority(LaQueryContext query)
        {
            return BuildResult(_laCharacteristicDataService, query, _laCharacteristicResultBuilder);
        }

        public TableBuilderResult GetNational(NationalQueryContext query)
        {
            return BuildResult(_nationalCharacteristicDataService, query, _characteristicResultBuilder);
        }

        private static int GetLatestRelease<T>(IDataService<T> dataService, Guid publicationId) where T : TidyData
        {
            return dataService.TopWithPredicate(data => data.ReleaseId, data => data.PublicationId == publicationId);
        }

        private static TableBuilderResult BuildResult<T>(IDataService<T> dataService,
            IQueryContext<T> queryContext,
            IResultBuilder<T, ITableBuilderData> resultBuilder) where T : TidyData
        {
            var releaseId = GetLatestRelease(dataService, queryContext.PublicationId);
            var data = dataService.FindMany(queryContext.FindExpression(releaseId));

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
                Result = data.Select(tidyData => resultBuilder.BuildResult(tidyData, queryContext.Attributes))
            };
        }
    }
}