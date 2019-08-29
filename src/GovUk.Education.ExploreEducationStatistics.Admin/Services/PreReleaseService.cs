using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PreReleaseService : IPreReleaseService
    {
        /*
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PreReleaseService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        */

        public async Task<List<UserDetailsViewModel>> GetPreReleaseContactsAsync()
        {
            // TODO return a list of candidate Users once we have a concept of Users in the database
            return await Task.FromResult(new List<UserDetailsViewModel>()
            {
                new UserDetailsViewModel()
                {
                    Id = new Guid("9b057c20-687a-4734-9e0d-419ada88cde0"),
                    Name = "TODO Pre Release User 1",
                },
                new UserDetailsViewModel()
                {
                    Id = new Guid("4606b997-cf60-4564-86d7-bfd6cd38e879"),
                    Name = "TODO Pre Release User 2",
                },
                new UserDetailsViewModel()
                {
                    Id = new Guid("513e2657-d1a8-4d30-afb3-8d382cc4ddee"),
                    Name = "TODO Pre Release User 3",
                },
            });
        }
    }
}