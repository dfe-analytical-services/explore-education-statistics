using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;
using MongoDB.Driver.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class NationalQueryContext : IQueryContext<CharacteristicDataNational>
    {
        public Guid PublicationId { get; set; }
        public ICollection<SchoolType> SchoolTypes { get; set; }
        public ICollection<int> Years { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public ICollection<string> Attributes { get; set; }
        public ICollection<string> Characteristics { get; set; }

        public IMongoQueryable<CharacteristicDataNational> FindExpression(
            IMongoQueryable<CharacteristicDataNational> queryable)
        {
            queryable = queryable.Where(x => x.Level == Level.National.ToString());

            if (SchoolTypes.Count > 0)
            {
                queryable = queryable.Where(x =>
                    SchoolTypes.Select(Query.SchoolTypes.EnumToString).Contains(x.SchoolType));
            }

            var yearsQuery = QueryUtil.YearsQuery(Years, StartYear, EndYear);
            if (yearsQuery.Any())
            {
                queryable = queryable.Where(x => yearsQuery.Contains(x.Year));
            }

            if (Characteristics.Count > 0)
            {
                queryable = queryable.Where(x => Characteristics.Contains(x.Characteristic.Name));
            }

            return queryable;
        }
    }
}