using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Models
{
    public class DataSet
    {
        public MetaGroup<IndicatorMeta>[] IndicatorMetas { get; set; }
        public CharacteristicMeta[] CharacteristicMetas { get; set; }
        public DataCsvFilename Filename { get; set; }

        public Type getDataType()
        {
            return Filename.GetDataTypeFromDataFileAttributeOfEnumType(Filename.GetType());
        }
    }
}