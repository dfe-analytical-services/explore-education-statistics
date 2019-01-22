using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class LaCharacteristicModel : DataModel<LaCharacteristicModel>
    {
        public LaCharacteristicModel()
        {
        }

        public LaCharacteristicModel(int year, string level, Country country, string schoolType,
            Dictionary<string, string> attributes, Region region, LocalAuthority localAuthority, Characteristic characteristic) :
            base(year, level, country, schoolType, attributes)
        {
            Region = region;
            LocalAuthority = localAuthority;
            Characteristic = characteristic;
        }

        public Region Region { get; set; }

        public LocalAuthority LocalAuthority { get; set; }

        public Characteristic Characteristic { get; set; }
    }
}