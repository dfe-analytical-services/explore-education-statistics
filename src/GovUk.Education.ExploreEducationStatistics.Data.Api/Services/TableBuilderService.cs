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

        public TableToolResult Get(Guid publicationId,
            SchoolType schoolType,
            Level level = Level.National)
        {
            var data = _dataService.GetCollectionForPublication(publicationId)
                .Find(x => x.Level == level.ToString() && x.SchoolType == schoolType.ToString())
                .ToList();
            
            var first = data.FirstOrDefault(); 
            
            return new TableToolResult
            { 
                PublicationId = first.PublicationId,
                ReleaseId = first.ReleaseId,
                ReleaseDate = first.ReleaseDate,
                Result = data.Select(DataToTableToolData)
            };
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