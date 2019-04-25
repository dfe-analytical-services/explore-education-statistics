using System;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DataFileAttribute : Attribute
    {
        public DataCsvMetaFilename MetaFilename { get; }

        public DataFileAttribute(DataCsvMetaFilename metaFilename)
        {
            MetaFilename = metaFilename;
        }
    }
}