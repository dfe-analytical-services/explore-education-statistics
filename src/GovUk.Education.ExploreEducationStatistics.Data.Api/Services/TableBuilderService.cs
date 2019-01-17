using System;
using System.Collections.Generic;
using System.Linq;
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

        public TableToolResult Get(Guid publication,
            SchoolType schoolType,
            Level level = Level.National)
        {
            return new TableToolResult
            {
                Publication = publication,
                Release = new Guid(),
                Result = GetTableToolData(publication, level, schoolType)
            };
        }

        private IEnumerable<TableToolData> GetTableToolData(Guid publication, Level level, SchoolType schoolType)
        {
            return _dataService.GetCollectionForPublication(publication)
                .Find(x => x.Level == level.ToString() && x.SchoolType == schoolType.ToString())
                .ToList()
                .Select(DataToTableToolData);
        }

        private static TableToolData DataToTableToolData(TidyData data)
        {
            return new TableToolData
            {
                Domain = data.Year.ToString(),
                //Range = FilterAttributes(data.Attributes)
                Range = data.Attributes
            };
        }

        private Dictionary<string, string> FilterAttributes(Dictionary<string, string> attributes)
        {
            return attributes.Where(pair => pair.Key.Contains("_exact") || pair.Key.Contains("_percent"))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}