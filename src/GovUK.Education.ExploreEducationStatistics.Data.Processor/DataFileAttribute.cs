using System;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
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