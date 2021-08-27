import ButtonText from '@common/components/ButtonText';
import useToggle from '@common/hooks/useToggle';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import commaList from '@common/utils/string/commaList';
import classNames from 'classnames';
import React from 'react';

export function generateTableTitle({
  indicators,
  timePeriodRange,
  locations,
  subjectName,
  filters,
  expanded,
}: FullTableMeta & {
  expanded?: boolean;
}) {
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

  const locationsString = ` in ${commaList(
    locations.length > 10 && !expanded
      ? locations
          .map(location => location.label)
          .sort()
          .slice(0, 10)
          .concat(`${locations.length - 10} other locations...`)
      : locations.map(location => location.label).sort(),
  )}`;

  const filterList = Object.values(filters)
    .flatMap(filterGroup => filterGroup.options)
    .map(filter => filter.label)
    .filter(label => label !== 'Total');
  const filterString = filterList.length ? ` for ${commaList(filterList)}` : '';

  return `${indicatorString}'${subjectName}'${filterString}${locationsString}${timePeriodString}`;
}

interface Props extends FullTableMeta {
  id?: string;
  title?: string;
}

const DataTableCaption = ({ id, title, ...props }: Props) => {
  const { locations } = props;

  const [expanded, toggleExpanded] = useToggle(false);

  const caption = generateTableTitle({
    ...props,
    expanded,
  });

  return (
    <>
      <strong id={id} data-testid="dataTableCaption">
        {title || caption}
      </strong>
      {locations.length > 10 && (
        <ButtonText
          className={classNames('govuk-!-display-block govuk-!-margin-top-2')}
          onClick={toggleExpanded}
        >
          {`${expanded ? 'Hide' : 'View'} full table title`}
        </ButtonText>
      )}
    </>
  );
};

export default DataTableCaption;
