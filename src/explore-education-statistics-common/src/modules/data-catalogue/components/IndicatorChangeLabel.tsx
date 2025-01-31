import { IndicatorOption } from '@common/services/types/apiDataSetMeta';
import React from 'react';

interface Props {
  currentState?: IndicatorOption;
  previousState?: IndicatorOption;
}

export default function IndicatorChangeLabel({
  currentState,
  previousState,
}: Props) {
  if (previousState && currentState) {
    return (
      <>
        {previousState.label} (id: <code>{previousState.id}</code>, column:{' '}
        <code>{previousState.column}</code>):
        <ul>
          {previousState.label !== currentState.label && (
            <li>label changed to: {currentState.label}</li>
          )}
          {previousState.id !== currentState.id && (
            <li>
              id changed to: <code>{currentState.id}</code>
            </li>
          )}
          {previousState.column !== currentState.column && (
            <li>
              column changed to: <code>{currentState.column}</code>
            </li>
          )}
          {previousState.unit !== currentState.unit && (
            <li>
              unit changed to: <code>{currentState.unit}</code>
            </li>
          )}
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
      {state.label} (id: <code>{state.id}</code>)
    </>
  );
}
