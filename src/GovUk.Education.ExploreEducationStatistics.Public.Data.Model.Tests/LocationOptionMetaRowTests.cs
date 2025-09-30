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
                $"{nameof(LocationOptionMetaRow.Id)} value",
            ];

            var expectedProperties = typeof(LocationOptionMetaRow)
                .GetProperties()
                .Select(p => $"{p.Name} value")
                .Except(excludedProperties)
                .ToHashSet();

            var optionRow = new LocationOptionMetaRow
            {
                Id = 1,
                Type = nameof(LocationOptionMetaRow.Type) + " value",
                Label = nameof(LocationOptionMetaRow.Label) + " value",
                Code = nameof(LocationOptionMetaRow.Code) + " value",
                OldCode = nameof(LocationOptionMetaRow.OldCode) + " value",
                Urn = nameof(LocationOptionMetaRow.Urn) + " value",
                LaEstab = nameof(LocationOptionMetaRow.LaEstab) + " value",
                Ukprn = nameof(LocationOptionMetaRow.Ukprn) + " value"
            };

            var rowKey = optionRow.GetRowKey();
            var properties = rowKey.Split(',').ToHashSet();

            Assert.Equal(expectedProperties, properties);
        }
    }

    public class GetRowKeyPrettyTests
    {
        [Fact]
        public void ContainsKeyProperties()
        {
            HashSet<string> excludedProperties =
            [
                $"{nameof(LocationOptionMetaRow.Id)}:{nameof(LocationOptionMetaRow.Id)} value",
            ];

            var expectedProperties = typeof(LocationOptionMetaRow)
                .GetProperties()
                .Select(p => $"{p.Name}:{p.Name} value")
                .Except(excludedProperties)
                .ToHashSet();

            var optionRow = new LocationOptionMetaRow
            {
                Id = 1,
                Type = nameof(LocationOptionMetaRow.Type) + " value",
                Label = nameof(LocationOptionMetaRow.Label) + " value",
                Code = nameof(LocationOptionMetaRow.Code) + " value",
                OldCode = nameof(LocationOptionMetaRow.OldCode) + " value",
                Urn = nameof(LocationOptionMetaRow.Urn) + " value",
                LaEstab = nameof(LocationOptionMetaRow.LaEstab) + " value",
                Ukprn = nameof(LocationOptionMetaRow.Ukprn) + " value"
            };

            var rowKey = optionRow.GetRowKeyPretty();
            var properties = rowKey.Split(',').ToHashSet();

            Assert.Equal(expectedProperties, properties);
        }
    }

    [Fact]
    public void OmitsNullProperties()
    {
        HashSet<string> excludedProperties =
        [
            $"{nameof(LocationOptionMetaRow.Id)}:{nameof(LocationOptionMetaRow.Id)} value",
            $"{nameof(LocationOptionMetaRow.OldCode)}:{nameof(LocationOptionMetaRow.OldCode)} value",
            $"{nameof(LocationOptionMetaRow.Ukprn)}:{nameof(LocationOptionMetaRow.Ukprn)} value",
        ];

        var expectedProperties = typeof(LocationOptionMetaRow)
            .GetProperties()
            .Select(p => $"{p.Name}:{p.Name} value")
            .Except(excludedProperties)
            .ToHashSet();

        var optionRow = new LocationOptionMetaRow
        {
            Id = 1,
            Type = nameof(LocationOptionMetaRow.Type) + " value",
            Label = nameof(LocationOptionMetaRow.Label) + " value",
            Code = nameof(LocationOptionMetaRow.Code) + " value",
            OldCode = null,
            Urn = nameof(LocationOptionMetaRow.Urn) + " value",
            LaEstab = nameof(LocationOptionMetaRow.LaEstab) + " value",
            Ukprn = null
        };

        var rowKey = optionRow.GetRowKeyPretty();
        var properties = rowKey.Split(',').ToHashSet();

        Assert.Equal(expectedProperties, properties);
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
