#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public abstract class AbstractSubjectMetaService
    {
        protected static IEnumerable<LocationAttributeViewModel> DeduplicateLocationViewModels(
            IEnumerable<LocationAttributeViewModel> viewModels)
        {
            var list = viewModels.ToList();

            // TODO EES-2954 SOW8 Review if these cases are still applicable and enhance if necessary

            /*
             The list of Location attributes should in theory already be unique.
             If they are not, there's three possibilities:
              * Duplicates exist where the label-value pairs are distinct but the Level attribute is different
                i.e. where the same Location attribute is reused across multiple Geographic Levels e.g. LA and LAD.
                These need transforming to give them distinct labels.
              * Duplicates where the labels are the same but the values are different.
                These need transforming to give them distinct labels.
              * Duplicates where the values are the same but the labels are different.
                These don't need any action.
            */

            var case1 = list
                .GroupBy(model => (model.Value, model.Label))
                .Where(grouping => grouping.Count() > 1)
                .SelectMany(grouping => grouping)
                .ToList();

            var case2 = list.Except(case1)
                .GroupBy(model => model.Label)
                .Where(grouping => grouping.Count() > 1)
                .SelectMany(grouping => grouping)
                .ToList();

            if (!(case1.Any() || case2.Any()))
            {
                return list;
            }

            return list.Select(value =>
            {
                if (case1.Contains(value))
                {
                    value.Label += $" ({value.Level})";
                }

                if (case2.Contains(value))
                {
                    value.Label += $" ({value.Value})";
                }

                return value;
            });
        }
    }
}
