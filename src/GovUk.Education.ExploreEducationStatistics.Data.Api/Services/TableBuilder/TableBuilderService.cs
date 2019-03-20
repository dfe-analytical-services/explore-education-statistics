using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class TableBuilderService : ITableBuilderService
    {
        private readonly IReleaseService _releaseService;
        private readonly IGeographicDataService _geographicDataService;
        private readonly ILaCharacteristicDataService _laCharacteristicDataService;
        private readonly INationalCharacteristicDataService _nationalCharacteristicDataService;
        private readonly GeographicResultBuilder _geographicResultBuilder;
        private readonly CharacteristicResultBuilder _characteristicResultBuilder;
        private readonly LaCharacteristicResultBuilder _laCharacteristicResultBuilder;

        public TableBuilderService(IReleaseService releaseService,
            IGeographicDataService geographicDataService,
            ILaCharacteristicDataService laCharacteristicDataService,
            INationalCharacteristicDataService nationalCharacteristicDataService,
            GeographicResultBuilder geographicResultBuilder,
            CharacteristicResultBuilder characteristicResultBuilder,
            LaCharacteristicResultBuilder laCharacteristicResultBuilder)
        {
            _releaseService = releaseService;
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

        private IEnumerable<T> GetData<T>(IDataService<T> dataService, IQueryContext<T> queryContext) where T : TidyData
        {
            var releaseId = _releaseService.GetLatestRelease(queryContext.PublicationId);
            return dataService.FindMany(queryContext.FindExpression(releaseId),
                new List<Expression<Func<T, object>>> {data => data.Release}
            );
        }

        private TableBuilderResult BuildResult<T>(IDataService<T> dataService,
            IQueryContext<T> queryContext,
            IResultBuilder<T, ITableBuilderData> resultBuilder) where T : TidyData
        {
            var data = GetData(dataService, queryContext);

            if (!data.Any())
            {
                return new TableBuilderResult();
            }

            var first = data.FirstOrDefault();
            return new TableBuilderResult
            {
                PublicationId = first.PublicationId,
                ReleaseId = first.Release.Id,
                ReleaseDate = first.Release.ReleaseDate,
                Level = first.Level,
                Result = data.Select(tidyData => resultBuilder.BuildResult(tidyData, queryContext.Indicators))
            };
        }
    }
}