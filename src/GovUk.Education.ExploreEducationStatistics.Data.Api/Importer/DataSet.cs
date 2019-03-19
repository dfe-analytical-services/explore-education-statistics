using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
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