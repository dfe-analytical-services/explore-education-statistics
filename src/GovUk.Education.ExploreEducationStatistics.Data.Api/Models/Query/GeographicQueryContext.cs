using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;
using MongoDB.Driver.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class GeographicQueryContext : IQueryContext<GeographicData>
    {
        public Guid PublicationId { get; set; }
        public Level Level { get; set; }
        public ICollection<SchoolType> SchoolTypes { get; set; }
        public ICollection<int> Years { get; set; }
        public ICollection<string> Regions { get; set; }
        public ICollection<string> LocalAuthorities { get; set; }
        public ICollection<string> Schools { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public ICollection<string> Attributes { get; set; }

        public IMongoQueryable<GeographicData> FindExpression(IMongoQueryable<GeographicData> queryable)
        {
            queryable = queryable.Where(x => x.Level == Level);

            if (SchoolTypes != null && SchoolTypes.Count > 0)
            {
                queryable = queryable.Where(x => SchoolTypes.Contains(x.SchoolType));
            }

            var yearsQuery = QueryUtil.YearsQuery(Years, StartYear, EndYear);
            if (yearsQuery.Any())
            {
                queryable = queryable.Where(x => yearsQuery.Contains(x.Year));
            }

            if (Regions != null && Regions.Count > 0)
            {
                queryable = queryable.Where(x => Regions.Contains(x.Region.Code));
            }

            if (LocalAuthorities != null && LocalAuthorities.Count > 0)
            {
                queryable = queryable.Where(x => LocalAuthorities.Contains(x.LocalAuthority.Code));
            }

            if (Schools != null && Schools.Count > 0)
            {
                queryable = queryable.Where(x => Schools.Contains(x.School.LaEstab));
            }

            return queryable;
        }
    }
}