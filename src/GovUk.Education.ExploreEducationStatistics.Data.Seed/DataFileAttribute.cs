using System;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DataFileAttribute : Attribute
    {
        public ImportFileType ImportFileType { get; }

        public DataFileAttribute(ImportFileType importFileType)
        {
            ImportFileType = importFileType;
        }
    }
}