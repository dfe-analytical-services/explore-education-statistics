using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public class TableBuilderCharacteristicData : ITableBuilderData
    {
        public string Year { get; set; }
        public CharacteristicViewModel Characteristic { get; set; }
        public Dictionary<string, string> Range { get; set; }
    }
}