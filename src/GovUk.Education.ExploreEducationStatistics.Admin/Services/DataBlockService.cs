using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using DataBlockId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataBlockService : IDataBlockService
    {
        public Task<DataBlockViewModel> Get(DataBlockId id)
        {
            if (id.ToString() == "17774a74-1f62-4b76-b9b5-84f14dac7278")
            {
                return Task.FromResult(new DataBlockViewModel
                {
                    Id = new DataBlockId("17774a74-1f62-4b76-b9b5-84f14dac7278"),
                    Heading =
                        "Local Authority, Total, Number of schools, Number of Pupils, Number of Permanent Exclusions",
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
                });
            }

            return null;
        }

        public Task<List<DataBlockViewModel>> ListAsync(ReleaseId releaseId)
        {
            var dataBlockList = new List<DataBlockViewModel>();

            if (releaseId.ToString() == "e7774a74-1f62-4b76-b9b5-84f14dac7278")
            {
                dataBlockList.Add(new DataBlockViewModel
                    {
                        Id = new DataBlockId("17774a74-1f62-4b76-b9b5-84f14dac7278"),
                        Heading =
                            "Local Authority, Total, Number of schools, Number of Pupils, Number of Permanent Exclusions",
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