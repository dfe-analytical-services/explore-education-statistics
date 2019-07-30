using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ContactServiceTests
    {
        [Fact]
        public void GetContacts()
        {
            using (var context = DbUtils.InMemoryApplicationDbContext("Find"))
            {
                var contactsToSave = new List<Contact>
                    {
                        new Contact()
                        {
                            Id = Guid.NewGuid(),
                            ContactName = "Contact A",
                            TeamEmail = "ContactA@example.com",
                            ContactTelNo = "123456789"
                        },
                        new Contact()
                        {
                            Id = Guid.NewGuid(),
                            ContactName = "Contact B",
                            TeamEmail = "ContactB@example.com",
                            ContactTelNo = "123456789"
                        }
                    };


                context.AddRange(contactsToSave);
                context.SaveChanges();
                
                var service = new ContactService(context);
                // Method under test
                var retrievedContacts = service.GetContacts();
                Assert.True(retrievedContacts.Exists(c => c.ContactName == "Contact A"));
                Assert.True(retrievedContacts.Exists(c => c.ContactName == "Contact B"));
            }
        }
    }
}