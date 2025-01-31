#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Validators
{
    public class ContainsOnlyAttributeTests
    {
        [Fact]
        public void AllowedValues_Empty()
        {
            var attribute = new ContainsOnlyAttribute();

            Assert.True(attribute.IsValid(ListOf<string>()));
            Assert.True(attribute.IsValid(ListOf("a")));
            Assert.True(attribute.IsValid(ListOf("a", "b")));
        }

        [Fact]
        public void AllowedValues_InvalidValuesTypeThrows()
        {
            var attribute = new ContainsOnlyAttribute("a");

            Assert.Throws<ArgumentException>(() => attribute.IsValid(1));
            Assert.Throws<ArgumentException>(() => attribute.IsValid(new { }));
            Assert.Throws<ArgumentException>(() => attribute.IsValid(null));
        }

        [Fact]
        public void AllowedValues_Single()
        {
            var attribute = new ContainsOnlyAttribute("a");

            Assert.True(attribute.IsValid(ListOf<string>()));
            Assert.True(attribute.IsValid(ListOf("a")));
            Assert.True(attribute.IsValid(ListOf("a", "a")));
        }

        [Fact]
        public void AllowedValues_Single_Invalid()
        {
            var attribute = new ContainsOnlyAttribute("a");

            Assert.False(attribute.IsValid(ListOf("A")));
            Assert.False(attribute.IsValid(ListOf("A", "a")));
            Assert.False(attribute.IsValid(ListOf("b")));
            Assert.False(attribute.IsValid(ListOf("b", "a")));
            Assert.False(attribute.IsValid(ListOf(1)));
            Assert.False(attribute.IsValid(ListOf(new { })));
            Assert.False(attribute.IsValid(ListOf<object>("a", new { }, 1)));
        }

        [Fact]
        public void AllowedValues_Multiple()
        {
            var attribute = new ContainsOnlyAttribute("a", "b", "c");

            Assert.True(attribute.IsValid(ListOf<string>()));
            Assert.True(attribute.IsValid(ListOf("a")));
            Assert.True(attribute.IsValid(ListOf("b")));
            Assert.True(attribute.IsValid(ListOf("c")));
            Assert.True(attribute.IsValid(ListOf("a", "b")));
            Assert.True(attribute.IsValid(ListOf("a", "b", "c")));
            Assert.True(attribute.IsValid(ListOf("a", "a", "b")));
            Assert.True(attribute.IsValid(ListOf("a", "a", "b", "b")));
            Assert.True(attribute.IsValid(ListOf("a", "a", "b", "b", "c")));
            Assert.True(attribute.IsValid(ListOf("a", "a", "b", "b", "c", "c")));
        }

        [Fact]
        public void AllowedValues_Multiple_Invalid()
        {
            var attribute = new ContainsOnlyAttribute("a", "b", "c");

            Assert.False(attribute.IsValid(ListOf("A")));
            Assert.False(attribute.IsValid(ListOf("A", "a")));
            Assert.False(attribute.IsValid(ListOf("d")));
            Assert.False(attribute.IsValid(ListOf("d", "a")));
            Assert.False(attribute.IsValid(ListOf("d", "a", "a")));
            Assert.False(attribute.IsValid(ListOf("d", "a", "b", "c")));
            Assert.False(attribute.IsValid(ListOf("d", "d")));
            Assert.False(attribute.IsValid(ListOf("d", "d", "a")));
            Assert.False(attribute.IsValid(ListOf("d", "d", "a", "a")));
            Assert.False(attribute.IsValid(ListOf("d", "d", "a", "b", "c")));
            Assert.False(attribute.IsValid(ListOf(1)));
            Assert.False(attribute.IsValid(ListOf(new { })));
            Assert.False(attribute.IsValid(ListOf<object>("a", new { }, 1)));
        }

        [Fact]
        public void AllowedValuesProvider_Empty()
        {
            var model = new AllowedValuesProviderModel
            {
                AllowedValues = new List<string>(),
            };

            Assert.True(ValidateModelValues(model, new List<string>()));
            Assert.True(ValidateModelValues(model, ListOf("a")));
            Assert.True(ValidateModelValues(model, ListOf("a", "b")));
        }

        [Fact]
        public void AllowedValuesProvider_InvalidValuesTypeThrows()
        {
            var model = new AllowedValuesProviderModel
            {
                AllowedValues = ListOf("a")
            };

            Assert.Throws<ArgumentException>(() => ValidateModelValues(model, 1));
            Assert.Throws<ArgumentException>(() => ValidateModelValues(model, new { }));
            Assert.Throws<ArgumentException>(() => ValidateModelValues(model, null));
        }

        [Fact]
        public void AllowedValuesProvider_InvalidAllowedValuesTypeThrows()
        {
            var model = new AllowedValuesProviderModel
            {
                Values = ListOf("a")
            };

            Assert.Throws<ArgumentException>(() => ValidateModelAllowedValues(model, "not allowed"));
            Assert.Throws<ArgumentException>(() => ValidateModelAllowedValues(model, 1));
            Assert.Throws<ArgumentException>(() => ValidateModelAllowedValues(model, new { }));
            Assert.Throws<ArgumentException>(() => ValidateModelAllowedValues(model, null));
        }

        [Fact]
        public void AllowedValuesProvider_Single()
        {
            var model = new AllowedValuesProviderModel
            {
                AllowedValues = ListOf("a")
            };

            Assert.True(ValidateModelValues(model, ListOf("a")));
            Assert.True(ValidateModelValues(model, ListOf("a", "a")));
        }

        [Fact]
        public void AllowedValuesProvider_Single_Invalid()
        {
            var model = new AllowedValuesProviderModel
            {
                AllowedValues = ListOf("a")
            };

            Assert.False(ValidateModelValues(model, ListOf("A")));
            Assert.False(ValidateModelValues(model, ListOf("A", "a")));
            Assert.False(ValidateModelValues(model, ListOf("b")));
            Assert.False(ValidateModelValues(model, ListOf("b", "a")));
        }

        [Fact]
        public void AllowedValuesProvider_Multiple()
        {
            var model = new AllowedValuesProviderModel
            {
                AllowedValues = ListOf("a", "b", "c")
            };

            Assert.True(ValidateModelValues(model, ListOf<string>()));
            Assert.True(ValidateModelValues(model, ListOf("a")));
            Assert.True(ValidateModelValues(model, ListOf("b")));
            Assert.True(ValidateModelValues(model, ListOf("c")));
            Assert.True(ValidateModelValues(model, ListOf("a", "b")));
            Assert.True(ValidateModelValues(model, ListOf("a", "b", "c")));
            Assert.True(ValidateModelValues(model, ListOf("a", "a", "b")));
            Assert.True(ValidateModelValues(model, ListOf("a", "a", "b", "b")));
            Assert.True(ValidateModelValues(model, ListOf("a", "a", "b", "b", "c")));
            Assert.True(ValidateModelValues(model, ListOf("a", "a", "b", "b", "c", "c")));
        }

        [Fact]
        public void AllowedValuesProvider_Multiple_Invalid()
        {
            var model = new AllowedValuesProviderModel
            {
                AllowedValues = ListOf("a", "b", "c")
            };

            Assert.False(ValidateModelValues(model, ListOf("A")));
            Assert.False(ValidateModelValues(model, ListOf("A", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d")));
            Assert.False(ValidateModelValues(model, ListOf("d", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d", "a", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d", "a", "b", "c")));
            Assert.False(ValidateModelValues(model, ListOf("d", "d")));
            Assert.False(ValidateModelValues(model, ListOf("d", "d", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d", "d", "a", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d", "d", "a", "b", "c")));
        }

        [Fact]
        public void StaticAllowedValuesProvider()
        {
            var model = new StaticAllowedValuesProviderModel();
            StaticAllowedValuesProviderModel.AllowedValues = ListOf("a", "b");

            Assert.True(ValidateModelValues(model, ListOf("a")));
            Assert.True(ValidateModelValues(model, ListOf("a", "a")));
            Assert.True(ValidateModelValues(model, ListOf("a", "b")));
            Assert.True(ValidateModelValues(model, ListOf("b", "b")));
        }

        [Fact]
        public void StaticAllowedValuesProvider_Invalid()
        {
            var model = new StaticAllowedValuesProviderModel();
            StaticAllowedValuesProviderModel.AllowedValues = ListOf("a", "b");

            Assert.False(ValidateModelValues(model, ListOf("A")));
            Assert.False(ValidateModelValues(model, ListOf("A", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d")));
            Assert.False(ValidateModelValues(model, ListOf("d", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d", "a", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d", "a", "b", "c")));
            Assert.False(ValidateModelValues(model, ListOf("d", "d")));
            Assert.False(ValidateModelValues(model, ListOf("d", "d", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d", "d", "a", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d", "d", "a", "b", "c")));
            Assert.False(ValidateModelValues(model, ListOf(1)));
            Assert.False(ValidateModelValues(model, ListOf(new { })));
            Assert.False(ValidateModelValues(model, ListOf<object>("a", 1, new { })));
        }

        [Fact]
        public void AllowedValuesAndProviderAreCombined()
        {
            var model = new CombinedAllowedValuesProviderModel();

            Assert.True(ValidateModelValues(model, ListOf<string>()));
            Assert.True(ValidateModelValues(model, ListOf("a")));
            Assert.True(ValidateModelValues(model, ListOf("b")));
            Assert.True(ValidateModelValues(model, ListOf("c")));
            Assert.True(ValidateModelValues(model, ListOf("a", "b")));
            Assert.True(ValidateModelValues(model, ListOf("a", "b", "c")));
            Assert.True(ValidateModelValues(model, ListOf("a", "a", "b")));
            Assert.True(ValidateModelValues(model, ListOf("a", "a", "b", "b")));
            Assert.True(ValidateModelValues(model, ListOf("a", "a", "b", "b", "c")));
            Assert.True(ValidateModelValues(model, ListOf("a", "a", "b", "b", "c", "c")));
        }

        [Fact]
        public void AllowedValuesAndProviderAreCombined_Invalid()
        {
            var model = new CombinedAllowedValuesProviderModel();

            Assert.False(ValidateModelValues(model, ListOf("A")));
            Assert.False(ValidateModelValues(model, ListOf("A", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d")));
            Assert.False(ValidateModelValues(model, ListOf("d", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d", "a", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d", "a", "b", "c")));
            Assert.False(ValidateModelValues(model, ListOf("d", "d")));
            Assert.False(ValidateModelValues(model, ListOf("d", "d", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d", "d", "a", "a")));
            Assert.False(ValidateModelValues(model, ListOf("d", "d", "a", "b", "c")));
            Assert.False(ValidateModelValues(model, ListOf(1)));
            Assert.False(ValidateModelValues(model, ListOf(new { })));
            Assert.False(ValidateModelValues(model, ListOf<object>("a", 1, new { })));
        }

        private bool ValidateModelValues(AllowedValuesProviderModel model, object? values)
        {
            model.Values = values;

            var context = new ValidationContext(model)
            {
                MemberName = nameof(model.Values)
            };

            return Validator.TryValidateProperty(model.Values, context, new List<ValidationResult>());
        }

        private bool ValidateModelValues(StaticAllowedValuesProviderModel model, object? values)
        {
            model.Values = values;

            var context = new ValidationContext(model)
            {
                MemberName = nameof(model.Values)
            };

            return Validator.TryValidateProperty(model.Values, context, new List<ValidationResult>());
        }


        private bool ValidateModelValues(CombinedAllowedValuesProviderModel model, object values)
        {
            model.Values = values;

            var context = new ValidationContext(model)
            {
                MemberName = nameof(model.Values)
            };

            return Validator.TryValidateProperty(model.Values, context, new List<ValidationResult>());
        }

        private bool ValidateModelAllowedValues(AllowedValuesProviderModel model, object? allowedValues)
        {
            model.AllowedValues = allowedValues;

            var context = new ValidationContext(model)
            {
                MemberName = nameof(model.Values)
            };

            return Validator.TryValidateProperty(model.Values, context, new List<ValidationResult>());
        }

        private class AllowedValuesProviderModel
        {
            public object? AllowedValues { get; set; }

            [ContainsOnly(AllowedValuesProvider = nameof(AllowedValues))]
            public object? Values { get; set; }
        }

        private class StaticAllowedValuesProviderModel
        {
            public static object? AllowedValues { get; set; }

            [ContainsOnly(AllowedValuesProvider = nameof(AllowedValues))]
            public object? Values { get; set; }
        }

        private class CombinedAllowedValuesProviderModel
        {
            public object AllowedValues = ListOf("c");

            [ContainsOnly("a", "b", AllowedValuesProvider = nameof(AllowedValues))]
            public object Values { get; set; } = new();
        }
    }
}