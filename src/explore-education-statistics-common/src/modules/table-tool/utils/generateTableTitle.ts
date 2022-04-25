import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import commaList from '@common/utils/string/commaList';

const MAX_LOCATIONS = 5;
const MAX_FILTERS = 5;

export default function generateTableTitle({
  indicators,
  timePeriodRange,
  locations,
  subjectName,
  filters,
}: FullTableMeta): string {
  let title =
    indicators.length === 1
      ? `${indicators[0].label} for '${subjectName}'`
      : `'${subjectName}'`;

  // Filters

  const filterLabels = Object.values(filters)
    .flatMap(filterGroup => filterGroup.options)
    .map(filter => filter.label)
    .filter(label => label !== 'Total')
    .sort();

  if (filterLabels.length > 0) {
    const remaining = filterLabels.length - MAX_FILTERS;

    title += ` for ${commaList(
      remaining > 0
        ? filterLabels
            .slice(0, MAX_FILTERS)
            .concat(
              `${remaining} other ${remaining === 1 ? 'filter' : 'filters'}`,
            )
        : filterLabels,
    )}`;
  }

  // Locations

  const locationLabels = locations.map(location => location.label).sort();

  if (locationLabels.length > 0) {
    const remaining = locationLabels.length - MAX_LOCATIONS;

    title += ` in ${commaList(
      remaining > 0
        ? locationLabels
            .slice(0, MAX_LOCATIONS)
            .concat(
              `${remaining} other ${
                remaining === 1 ? 'location' : 'locations'
              }`,
            )
        : locationLabels,
    )}`;
  }

  // Time periods

  if (timePeriodRange.length > 0) {
    const startLabel = timePeriodRange[0].label;
    const endLabel = timePeriodRange[timePeriodRange.length - 1].label;

    const timePeriodString =
      startLabel === endLabel
        ? ` for ${startLabel}`
        : ` between ${startLabel} and ${endLabel}`;

    title += timePeriodString;
  }

  return title;
}
