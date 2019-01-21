using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class TableBuilderService
    {
        private readonly DataService _dataService;

        public TableBuilderService(DataService dataService)
        {
            _dataService = dataService;
        }

        public TableToolResult Get(Guid publicationId,
            SchoolType schoolType,
            Level level,
            ICollection<int> yearFilter,
            ICollection<string> attributeFilter)
        {
            var data = _dataService.GetCollectionForPublication(publicationId)
                .AsQueryable().OfType<TidyDataGeographic>()
                .Where(FindExpression(level, schoolType, yearFilter))
                .ToList();

            var first = data.FirstOrDefault();

            return new TableToolResult
            {
                PublicationId = first.PublicationId,
                ReleaseId = first.ReleaseId,
                ReleaseDate = first.ReleaseDate,
                Result = data.Select(tidyData => DataToTableToolData(tidyData, attributeFilter))
            };
        }

        private static Expression<Func<TidyData, bool>> FindExpression(
            Level level,
            SchoolType schoolType,
            ICollection<int> yearFilter)
        {
            return x =>
                x.Level == level.ToString() &&
                x.SchoolType == schoolType.ToString() &&
                (yearFilter.Count == 0 || yearFilter.Contains(x.Year));
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