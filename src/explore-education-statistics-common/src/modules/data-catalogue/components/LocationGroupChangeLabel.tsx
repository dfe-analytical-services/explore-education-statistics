import { LocationGroup } from '@common/services/types/apiDataSetMeta';
import React from 'react';

interface Props {
  currentState?: LocationGroup;
  previousState?: LocationGroup;
}

export default function LocationGroupChangeLabel({
  currentState,
  previousState,
}: Props) {
  if (previousState && currentState) {
    return (
      <>
        {previousState.level.label} (code:{' '}
        <code>{previousState.level.code}</code>):
        <ul>
          <li>
            changed to: {currentState.level.label} (code:{' '}
            <code>{currentState.level.code}</code>)
          </li>
        </ul>
      </>
    );
  }

  const state = currentState ?? previousState;

  if (!state) {
    throw new Error('No change state provided');
  }

  return (
    <>
      {state.level.label} (code: <code>{state.level.code}</code>)
    </>
  );
}
