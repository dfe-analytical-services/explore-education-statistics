using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Extensions
{
    public static class DataFileAttributeExtensions
    {
        public static DataCsvMetaFilename GetMetaFilename(this Enum enumValue)
        {
            return enumValue.GetEnumAttribute<DataFileAttribute>().MetaFilename;
        }
    }
}