#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests;

public class ContactSaveRequestTests
{
    [Fact]
    public void ContactSaveRequest_ContactTelNo()
    {
        var request = new ContactSaveRequest
        {
            ContactTelNo = "01234567",
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
