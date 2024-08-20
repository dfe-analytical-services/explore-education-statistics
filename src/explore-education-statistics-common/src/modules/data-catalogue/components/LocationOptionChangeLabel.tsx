import { LocationOption } from '@common/services/types/apiDataSetMeta';
import intersperse from '@common/utils/array/intersperse';
import getLocationCodeEntries, {
  LocationCodeLabels,
} from '@common/utils/getLocationCodeEntries';
import React, { Fragment } from 'react';

const locationCodeLabels: Partial<LocationCodeLabels> = {
  code: 'code',
  id: 'id',
  oldCode: 'old code',
};

interface Props {
  currentState?: LocationOption;
  previousState?: LocationOption;
}

export default function LocationOptionChangeLabel({
  currentState,
  previousState,
}: Props) {
  if (previousState && currentState) {
    const codeEntries = getLocationCodeEntries(
      previousState,
      locationCodeLabels,
    );

    const codeLabels = codeEntries.map(entry => (
      <Fragment key={entry.key}>
        {entry.label}: <code>{entry.value}</code>
      </Fragment>
    ));

    return (
      <>
        {previousState.label} ({intersperse(codeLabels, () => ', ')}):
        <ul>
          {previousState.label !== currentState.label && (
            <li>label changed to: {currentState.label}</li>
          )}
          {codeEntries
            .filter(entry => entry.value !== currentState[entry.key])
            .map(entry => (
              <li key={entry.key}>
                {entry.label} changed to: <code>{currentState[entry.key]}</code>
              </li>
            ))}
        </ul>
      </>
    );
  }

  const state = currentState ?? previousState;

  if (!state) {
    throw new Error('No change state provided');
  }

  const codeLabels = getLocationCodeEntries(state, locationCodeLabels).map(
    entry => (
      <Fragment key={entry.key}>
        {entry.label}: <code>{entry.value}</code>
      </Fragment>
    ),
  );

  return (
    <>
      {state.label} ({intersperse(codeLabels, () => ', ')})
    </>
  );
}
