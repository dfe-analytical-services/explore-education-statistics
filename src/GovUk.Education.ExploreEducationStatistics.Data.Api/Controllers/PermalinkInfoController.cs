#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    public class PermalinkInfoController : ControllerBase
    {
        private readonly StatisticsDbContext _statisticsDbContext;

        public PermalinkInfoController(StatisticsDbContext statisticsDbContext)
        {
            _statisticsDbContext = statisticsDbContext;
        }

        [HttpGet("api/import")]
        public async Task<ActionResult> Import()
        {
            var d = new DirectoryInfo(@"C:\Users\ben\Downloads\Permalinks\");
            foreach (var fi in d.GetFiles())
            {
                var text = System.IO.File.ReadAllText(fi.FullName);

                //UpdateCreated(text);

                var permalink = JsonConvert.DeserializeObject<Permalink>(
                    value: text,
                    settings: BuildJsonSerializerSettings());

                if (permalink == null)
                {
                    return BadRequest("Null permalink for file:" + fi.Name);
                }

                if (permalink.Id.ToString() != fi.Name)
                {
                    return BadRequest("Permalink filename doesn't match id:" + fi.Name);
                }

                if (permalink.Query.SubjectId == null || permalink.Query.SubjectId == Guid.Empty)
                {
                    return BadRequest("Permalink query subject id is null for file" + fi.Name);
                }

                await _statisticsDbContext.PermalinkInfo.AddAsync(new PermlinkInfo
                {
                    Id = permalink.Id,
                    SubjectId = permalink.Query.SubjectId,
                    TableRows = permalink.FullTable.Results.Count(),
                    QueryLocationCodes = permalink.Query.Locations.CountItems(),
                    QueryLocationProviderCodes = permalink.Query.Locations.Provider?.Count ?? 0,
                    QueryLocationGeographicLevel = permalink.Query.Locations.GeographicLevel,
                    Content = text,
                    Created = permalink.Created
                });

                await _statisticsDbContext.SaveChangesAsync();
            }

            return NoContent();
        }

        private async Task UpdateCreated(string text)
        {
            JToken token = JToken.Parse(text);
            var idToken = token.SelectToken("Id");
            var createdToken = token.SelectToken("Created");

            _statisticsDbContext.Database.ExecuteSqlRaw(
                $"UPDATE PermalinkInfo SET Created = '{createdToken.ToObject<string>()}' WHERE Id = '{idToken.ToObject<string>()}'"
            );

            await _statisticsDbContext.SaveChangesAsync();
        }

        private static JsonSerializerSettings BuildJsonSerializerSettings()
        {
            return new()
            {
                ContractResolver = new PermalinkContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
        }
    }
}
