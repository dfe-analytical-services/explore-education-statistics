#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class MetaService : IMetaService
    {
        public List<TimeIdentifierCategoryModel> GetTimeIdentifiersByCategory()
        {
            var all = (TimeIdentifierCategory[]) Enum.GetValues(typeof(TimeIdentifierCategory));
            return all.Select(category => new TimeIdentifierCategoryModel
            {
                Category = category,
                TimeIdentifiers = category.GetTimeIdentifiers()
                    .Select(identifier => new TimeIdentifierModel {Identifier = identifier}).ToList()
            }).ToList();
        }
    }
}
