import { TimePeriodOption } from '@common/services/types/apiDataSetMeta';
import React from 'react';

interface Props {
  currentState?: TimePeriodOption;
  previousState?: TimePeriodOption;
}

export default function TimePeriodOptionChangeLabel({
  currentState,
  previousState,
}: Props) {
  if (previousState && currentState) {
    return (
      <>
        {previousState.label} (code: <code>{previousState.code}</code>):
        <ul>
          <li>
            changed to: {currentState.label} (code:{' '}
            <code>{currentState.code}</code>)
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
      {state.label} (code: <code>{state.code}</code>)
    </>
  );
}
