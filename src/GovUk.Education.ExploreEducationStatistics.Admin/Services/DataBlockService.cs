using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockService : IDataBlockService
    {
        public Task<List<DataBlockViewModel>> ListAsync(Guid releaseId)
        {
            var dataBlockList = new List<DataBlockViewModel>();

            if (releaseId.ToString() == "e7774a74-1f62-4b76-b9b5-84f14dac7278")
            {
                dataBlockList.Add(new DataBlockViewModel
                    {
                        Id = new Guid("17774a74-1f62-4b76-b9b5-84f14dac7278"),
                        Heading = "",
                        DataBlockRequest = new DataBlockRequest
                        {
                            SubjectId = 12,
                            GeographicLevel = "Local_Authority",
                            Indicators = new List<string> {"153", "154", "155"},
                            Filters = new List<string> {"423"},
                            TimePeriod = new TimePeriod
                            {
                                StartYear = "2014",
                                StartCode = TimeIdentifier.AcademicYear,
                                EndYear = "2016",
                                EndCode = TimeIdentifier.AcademicYear
                            }
                        }
                    }
                );
            }

            return Task.FromResult(dataBlockList);
        }
    }
}