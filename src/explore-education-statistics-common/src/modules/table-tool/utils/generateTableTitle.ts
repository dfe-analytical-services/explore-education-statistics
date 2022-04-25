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

  const locationsString = ` in ${commaList(
    locations.length > 10 && !expanded
      ? locationLabels
          .slice(0, 10)
          .concat(
            `${locations.length - 10} other ${
              locations.length - 10 === 1 ? 'location' : 'locations'
            }...`,
          )
      : locationLabels,
  )}`;

  const filterLabels = Object.values(filters)
    .flatMap(filterGroup => filterGroup.options)
    .map(filter => filter.label)
    .filter(label => label !== 'Total');

  const filterString = filterLabels.length
    ? ` for ${commaList(filterLabels)}`
    : '';

  return `${indicatorString}'${subjectName}'${filterString}${locationsString}${timePeriodString}`;
}
