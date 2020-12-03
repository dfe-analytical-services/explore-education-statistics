using System;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ImportStatusBau
    {
        public Guid SubjectId { get; set; }
        public string DataFileName { get; set; }
        public string MetaFileName { get; set; }
        public Release Release { get; set; }
        public string Errors { get; set; }
        public int NumberOfRows { get; set; }
        public IStatus Status { get; set; }
        public int StagePercentageComplete { get; set; }
    }
}