import commaList from '@common/lib/utils/string/commaList';
import {
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';
import React, { useState } from 'react';
import { FullTableMeta } from '@common/modules/full-table/types/fullTable';
import ButtonText from '@common/components/ButtonText';
import classNames from 'classnames';

export function generateTableTitle({
  timePeriodRange,
  locations,
  subjectName,
  publicationName,
  expanded,
}: {
  timePeriodRange: FullTableMeta['timePeriodRange'];
  locations: FullTableMeta['locations'];
  subjectName: FullTableMeta['subjectName'];
  publicationName: FullTableMeta['publicationName'];
  expanded?: boolean;
}) {
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
      ? locations.slice(0, 10).map(location => location.label)
      : locations.map(location => location.label),
  )}`;

  return `Table showing '${subjectName}' from '${publicationName}'${
    !expanded && locations.length > 10
      ? `${locationsString} and ${locations.length} other locations...`
      : locationsString
  }${timePeriodString}`;
}

interface Props {
  id: string;
  timePeriodRange: TimePeriodFilter[];
  locations: LocationFilter[];
  subjectName: string;
  publicationName: string;
}

const DataTableCaption = ({
  id = 'dataTableCaption',
  timePeriodRange,
  locations,
  subjectName,
  publicationName,
}: Props) => {
  const [expanded, setExpanded] = useState<boolean>(false);
  const caption = generateTableTitle({
    timePeriodRange,
    locations,
    subjectName,
    publicationName,
    expanded,
  });

  return (
    <>
      <strong id={id}>{caption}</strong>
      {locations.length > 10 && (
        <ButtonText
          className={classNames('govuk-!-display-block govuk-!-margin-top-2')}
          onClick={() => {
            return expanded ? setExpanded(false) : setExpanded(true);
          }}
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
