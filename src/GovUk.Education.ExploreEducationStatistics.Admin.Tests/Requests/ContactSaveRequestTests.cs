#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests
{
    public class ContactSaveRequestTests
    {
        [Fact]
        public void ContactSaveRequest_ContactTelNo()
        {
            var request = new ContactSaveRequest
            {
                ContactTelNo = "12345678",
            };

            var validationContext = new ValidationContext(request);

            var results = request.Validate(validationContext);

            Assert.Empty(results);
        }

        [Fact]
        public void ContactSaveRequest_ContactTelNo_Null()
        {
            var request = new ContactSaveRequest
            {
                ContactTelNo = null,
            };

            var validationContext = new ValidationContext(request);

            var results = request.Validate(validationContext);

            Assert.Empty(results);
        }

        [Fact]
        public void ContactSaveRequest_ContactTelNo_TooShort()
        {
            var request = new ContactSaveRequest
            {
                ContactTelNo = "1234567",
            };

            var validationContext = new ValidationContext(request);

            var results = request.Validate(validationContext);

            var validationResult = Assert.Single(results);
            Assert.Equal("Contact telephone must have a minimum length of '8'.", validationResult.ErrorMessage);
        }

        [Theory]
        [InlineData("abcdefghijk")]
        [InlineData(@"!&$\*^!\*&)%!")]
        [InlineData("a12345678")]
        [InlineData("12345678a")]
        public void ContactSaveRequest_ContactTelNo_NotNumeric(string contactTelNo)
        {
            var request = new ContactSaveRequest
            {
                ContactTelNo = contactTelNo,
            };

            var validationContext = new ValidationContext(request);

            var results = request.Validate(validationContext);

            var validationResult = Assert.Single(results);
            Assert.Equal("Contact telephone must only contain numbers and spaces", validationResult.ErrorMessage);
        }

        [Theory]
        [InlineData(" 0 3 7 0 0 0 0 2 2 8 8 ")]
        [InlineData("03700002288")]
        [InlineData("    0370 000 2288")]
        [InlineData("037 0000 2288    ")]
        public void ContactSaveRequest_ContactTelNo_NotDfEEnquiriesNum(string contactTelNo)
        {
            var request = new ContactSaveRequest
            {
                ContactTelNo = contactTelNo,
            };

            var validationContext = new ValidationContext(request);

            var results = request.Validate(validationContext);

            var validationResult = Assert.Single(results);
            Assert.Equal("Contact telephone cannot be DfE Enquiries number", validationResult.ErrorMessage);
        }
    }
}
