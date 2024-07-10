using System.Reflection;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests;

public abstract class LocationOptionMetaRowTests
{
    public class GetRowKeyTests
    {
        [Fact]
        public void ContainsKeyProperties()
        {
            HashSet<string> excludedProperties =
            [
                nameof(LocationOptionMetaRow.Id),
            ];

            var expectedProperties = typeof(LocationOptionMetaRow)
                .GetProperties()
                .Select(p => p.Name)
                .Except(excludedProperties)
                .ToHashSet();

            var optionRow = new LocationOptionMetaRow
            {
                Id = 1,
                Type = nameof(LocationOptionMetaRow.Type),
                Label = nameof(LocationOptionMetaRow.Label),
                Code = nameof(LocationOptionMetaRow.Code),
                OldCode = nameof(LocationOptionMetaRow.OldCode),
                Urn = nameof(LocationOptionMetaRow.Urn),
                LaEstab = nameof(LocationOptionMetaRow.LaEstab),
                Ukprn = nameof(LocationOptionMetaRow.Ukprn),
            };

            var rowKey = optionRow.GetRowKey();
            var properties = rowKey.Split(',').ToHashSet();

            Assert.Equal(expectedProperties, properties);
        }
    }

    public class PropertyTests
    {
        [Fact]
        public void PropertiesMatchLocationOptionMeta()
        {
            HashSet<string> excludedProperties =
            [
                nameof(LocationOptionMeta.Metas),
                nameof(LocationOptionMeta.MetaLinks),
            ];

            var optionProperties = typeof(LocationOptionMeta)
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .Except(excludedProperties)
                .ToHashSet();

            var optionRowProperties = typeof(LocationOptionMetaRow)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .ToHashSet();

            // Check that properties are matching between these models as it's easy
            // to forget to add new option model properties to the row model.
            Assert.Equal(optionRowProperties, optionProperties);
        }
    }
}
