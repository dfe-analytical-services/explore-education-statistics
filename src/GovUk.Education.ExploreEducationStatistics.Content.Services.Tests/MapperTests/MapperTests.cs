using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Mappings;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.MapperTests;

public class MapperTests
{
    [Fact]
    public void CanMapContactsToContactViewModels()
    {
        var mockContact = new Contact()
        {
            ContactName = "Mock Contact Name",
            ContactTelNo = "Mock Contact Tel No.",
            TeamName = "Mock Contact Team Name",
            TeamEmail = "Mock Contact Team Email"
        };

        var services = new ServiceCollection();
        services.AddAutoMapper(typeof(MappingProfiles));

        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetService<IMapper>();

        var contactViewModel = mapper.Map<ContactViewModel>(mockContact);

        Assert.NotNull(contactViewModel);
        Assert.Equal(contactViewModel.ContactName, mockContact.ContactName);
        Assert.Equal(contactViewModel.TeamEmail, mockContact.TeamEmail);
        Assert.Equal(contactViewModel.TeamName, mockContact.TeamName);
        Assert.Equal(contactViewModel.ContactTelNo, mockContact.ContactTelNo);
    }
}
