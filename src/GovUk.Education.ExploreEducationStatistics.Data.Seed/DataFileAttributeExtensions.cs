using System;
using System.Linq;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    public static class DataFileAttributeExtensions
    {
        public static ImportFileType GetImportFileTypeFromDataFileAttributeOfEnumType(this Enum enumValue,
            Type enumType)
        {
            return enumType.GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DataFileAttribute>().ImportFileType;
        }
    }
}