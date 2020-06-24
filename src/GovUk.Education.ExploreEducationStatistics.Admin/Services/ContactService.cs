﻿using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ContactService : IContactService
    {
        private readonly ContentDbContext _context;

        public ContactService(ContentDbContext context)
        {
            _context = context;
        }

        public List<Contact> GetContacts()
        {
            return _context.Contacts.OrderBy(c => c.TeamName).ThenBy(c => c.ContactName).ToList();
        }
    }
}