import ButtonText from '@common/components/ButtonText';
import useToggle from '@common/hooks/useToggle';
import commaList from '@common/lib/utils/string/commaList';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import classNames from 'classnames';
import React from 'react';

export function generateTableTitle({
  indicators,
  timePeriodRange,
  locations,
  subjectName,
  publicationName,
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

  return `Table showing ${indicatorString}'${subjectName}' from '${publicationName}'${locationsString}${timePeriodString}`;
}

interface Props extends FullTableMeta {
  id: string;
  title?: string;
}

const DataTableCaption = ({
  id = 'dataTableCaption',
  title,
  ...props
}: Props) => {
  const { locations } = props;

  const [expanded, toggleExpanded] = useToggle(false);

  const caption = generateTableTitle({
    ...props,
    expanded,
  });

  return (
    <>
      <strong id={id}>{title || caption}</strong>
      {locations.length > 10 && (
        <ButtonText
          className={classNames('govuk-!-display-block govuk-!-margin-top-2')}
          onClick={toggleExpanded}
        >
          {`${expanded ? 'Hide' : 'View'} full table title`}
        </ButtonText>
      )}
      <ul>
        <li>
          <strong>n/a</strong> - value matching this criteria could not be
          found.
        </li>
        <li>
          <strong>x</strong> - value matching this criteria is suppressed.
        </li>
      </ul>
    </>
  );
};

export default DataTableCaption;
