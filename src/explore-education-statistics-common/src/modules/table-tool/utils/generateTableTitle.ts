import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import commaList from '@common/utils/string/commaList';

interface GenerateTableTitleOptions extends FullTableMeta {
  expanded?: boolean;
}

export default function generateTableTitle({
  indicators,
  timePeriodRange,
  locations,
  subjectName,
  filters,
  expanded,
}: GenerateTableTitleOptions) {
  const indicatorString =
    indicators.length === 1 ? `${indicators[0].label} for ` : '';

  let timePeriodString = '';

  if (timePeriodRange.length > 0) {
    const startLabel = timePeriodRange[0].label;
    const endLabel = timePeriodRange[timePeriodRange.length - 1].label;

    timePeriodString =
      startLabel === endLabel
        ? ` for ${startLabel}`
        : ` between ${startLabel} and ${endLabel}`;
  }

  const locationLabels = locations.map(location => location.label).sort();

  let locationString = '';

  if (locationLabels.length > 0) {
    const remaining = locations.length - 5;

    locationString = ` in ${commaList(
      locations.length > 5 && !expanded
        ? locationLabels
            .slice(0, 5)
            .concat(
              `${remaining} other ${
                remaining === 1 ? 'location' : 'locations'
              }`,
            )
        : locationLabels,
    )}`;
  }

  const filterLabels = Object.values(filters)
    .flatMap(filterGroup => filterGroup.options)
    .map(filter => filter.label)
    .filter(label => label !== 'Total')
    .sort();

  let filterString = '';

  if (filterLabels.length > 0) {
    const remaining = filterLabels.length - 5;

    filterString = ` for ${commaList(
      filterLabels.length > 5 && !expanded
        ? filterLabels
            .slice(0, 5)
            .concat(
              `${remaining} other ${remaining === 1 ? 'filter' : 'filters'}`,
            )
        : filterLabels,
    )}`;
  }

  return `${indicatorString}'${subjectName}'${filterString}${locationString}${timePeriodString}`;
}
