using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class NationalCharacteristicModel : DataModel<NationalCharacteristicModel>
    {
        public NationalCharacteristicModel()
        {
        }

        public NationalCharacteristicModel(int year, string level, Country country, string schoolType,
            Dictionary<string, string> attributes, Characteristic characteristic) :
            base(year, level, country, schoolType, attributes)
        {
            Characteristic = characteristic;
        }

        public Characteristic Characteristic { get; set; }
    }
}