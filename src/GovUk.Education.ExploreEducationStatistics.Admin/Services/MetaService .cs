using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class MetaService : IMetaService
    {
        private readonly ApplicationDbContext _context;

        public MetaService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public List<TimeIdentifierCategoryModel> GetTimeIdentifiersByCategory()
        {
            var all = ((TimeIdentifierCategory[]) Enum.GetValues(typeof(TimeIdentifierCategory)));
            return all.Select(category => new TimeIdentifierCategoryModel
            {
                Category = category,
                TimeIdentifiers = category.GetTimeIdentifiers()
                    .Select(identifier => new TimeIdentifierModel {Identifier = identifier}).ToList()
            }).ToList();
        }

        public List<ReleaseType> GetReleaseTypes()
        {
            return _context.ReleaseTypes.ToList();
        }
    }
}