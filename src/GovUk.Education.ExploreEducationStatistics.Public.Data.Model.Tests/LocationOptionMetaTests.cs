using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests;

public abstract class LocationOptionMetaTests
{
    public class ToRowTests
    {
        [Fact]
        public void RowHasAllProperties()
        {
            var option = new LocationCodedOptionMeta
            {
                Id = 1,
                Label = nameof(LocationCodedOptionMeta.Label),
                Code = nameof(LocationCodedOptionMeta.Code),
            };

            HashSet<string> excludedOptionProperties =
            [
                nameof(LocationOptionMeta.Metas),
                nameof(LocationOptionMeta.MetaLinks),
            ];

            // Use reflection to set all protected properties for the purpose of
            // this test. Otherwise, we would have to write boilerplate tests for
            // each LocationOptionMeta subtype (which is easy to forget to update).
            option
                .GetType()
                .GetProperties(
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty
                )
                .Where(p => !excludedOptionProperties.Contains(p.Name))
                .ForEach(p => p.SetValue(option, p.Name));

            var optionProperties = option
                .ToDictionary(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(kv => !excludedOptionProperties.Contains(kv.Key))
                .ToDictionary();

            var rowProperties = option.ToRow().ToDictionary();

            Assert.Equal(optionProperties, rowProperties);
        }
    }
}
