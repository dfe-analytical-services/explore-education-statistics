import { LocationOptionSource } from '@admin/services/apiDataSetVersionService';
import intersperse from '@common/utils/array/intersperse';
import getLocationCodeEntries from '@common/utils/getLocationCodeEntries';
import classNames from 'classnames';
import React, { Fragment } from 'react';

interface Props {
  block?: boolean;
  location: LocationOptionSource;
}

export default function ApiDataSetLocationCode({
  block = true,
  location,
}: Props) {
  const entries = getLocationCodeEntries(location);

  if (entries.length === 0) {
    return null;
  }

  const labels = entries.map(entry => (
    <Fragment key={entry.key}>
      {entry.label}: <code>{entry.value}</code>
    </Fragment>
  ));

  return (
    <span
      className={classNames('dfe-colour--dark-grey', {
        'govuk-!-display-block': block,
      })}
    >
      {intersperse(labels, () => ', ')}
    </span>
  );
}
