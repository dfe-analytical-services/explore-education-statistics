using Bogus;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Validators
{// https://docs.fluentvalidation.net/en/latest/testing.html
    public class FileValidatorsTests
    {
        private FileValidators _validator;

        public void Setup()
        {
            _validator = new FileValidators();
        }

        [Fact]
        public void Should_have_error_when_Name_is_null()
        {
            var model = new Person { Name = null };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(person => person.Name);
        }

        [Fact]
        public void Should_not_have_error_when_name_is_specified()
        {
            var model = new Person { Name = "Jeremy" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(person => person.Name);
        }
    }
}
