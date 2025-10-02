using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.ViewModels;

public static class LocationOptionViewModelsTests
{
    public class HasMajorChangeTests
    {
        public class LocationOptionViewModelTypeTheoryData : TheoryData<Type>
        {
            public LocationOptionViewModelTypeTheoryData()
            {
                typeof(LocationOptionViewModel).GetSubclasses().ForEach(Add);
            }
        }

        public static LocationOptionViewModelTypeTheoryData OptionTypes = new();

        [Theory]
        [MemberData(nameof(OptionTypes))]
        public void LabelChange_ReturnsFalse(Type type)
        {
            var option = Activator.CreateInstance(type) as LocationOptionViewModel;

            Assert.NotNull(option);

            type.GetProperty(nameof(LocationOptionViewModel.Label))?.SetValue(option, "label");

            var otherOption = option with { Label = "updated label" };

            Assert.False(option.HasMajorChange(otherOption));
        }

        [Theory]
        [MemberData(nameof(OptionTypes))]
        public void NonLabelChange_ReturnsTrue(Type type)
        {
            var nonLabelProperties = type.GetProperties()
                .Where(p => p.Name != nameof(LocationOptionViewModel.Label))
                .ToList();

            var option = Activator.CreateInstance(type) as LocationOptionViewModel;

            Assert.NotNull(option);

            nonLabelProperties.ForEach(p => p.SetValue(option, p.Name.ToLowerFirst()));

            Assert.All(
                nonLabelProperties,
                property =>
                {
                    var otherOption = option with { };

                    Assert.NotNull(otherOption);

                    property.SetValue(otherOption, "updated " + property.Name.ToLowerFirst());

                    Assert.True(option.HasMajorChange(otherOption));
                }
            );
        }
    }
}
