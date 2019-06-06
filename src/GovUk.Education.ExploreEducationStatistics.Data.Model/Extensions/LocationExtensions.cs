using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions
{
    public static class LocationExtensions
    {
        /**
        * Owned Types that are mapped with the owner in the same table provide no support for optional (i.e. nullable).
        * See this discussion https://github.com/aspnet/EntityFramework.Docs/issues/466
        * This method replaces empty Owned Types values that are ObservationalUnit properties of Location with null.
        * We do this so that they are not projected as empty values by AutoMapper.
        */
        public static void ReplaceEmptyOwnedTypeValuesWithNull(this Location location)
        {
            var observationalUnitType = typeof(IObservationalUnit);
            var observationalUnitProperties = from p in typeof(Location).GetProperties()
                where observationalUnitType.IsAssignableFrom(p.PropertyType)
                select p;

            foreach (var property in observationalUnitProperties)
            {
                var x = (IObservationalUnit) property.GetValue(location);
                if (x.Code == null)
                {
                    property.SetValue(location, null);
                }
            }
        }
    }
}