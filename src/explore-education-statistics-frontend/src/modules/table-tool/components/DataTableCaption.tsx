import commaList from '@common/lib/utils/string/commaList';
import { LocationFilter } from '@frontend/modules/table-tool/components/types/filters';
import TimePeriod from '@frontend/modules/table-tool/components/types/TimePeriod';
import React from 'react';

interface Props {
  id: string;
  timePeriods: TimePeriod[];
  locations: LocationFilter[];
  subjectName: string;
  publicationName: string;
}

const DataTableCaption = ({
  id = 'dataTableCaption',
  timePeriods,
  locations,
  subjectName,
  publicationName,
}: Props) => {
  let timePeriodString = '';

  if (timePeriods.length > 0) {
    const startLabel = timePeriods[0].label;
    const endLabel = timePeriods[timePeriods.length - 1].label;

    timePeriodString =
      startLabel === endLabel
        ? ` for ${startLabel}`
        : ` between ${startLabel} and ${endLabel}`;
  }

  const locationsString = ` in ${commaList(
    locations.map(location => location.label),
  )}`;

  const caption = `Table showing '${subjectName}' from '${publicationName}'${locationsString}${timePeriodString}`;

  return (
    <>
      <strong id={id}>{caption}</strong>

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
