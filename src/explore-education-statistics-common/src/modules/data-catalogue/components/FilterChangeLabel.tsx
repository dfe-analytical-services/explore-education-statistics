import { Filter } from '@common/services/types/apiDataSetMeta';
import React from 'react';

interface Props {
  currentState?: Filter;
  previousState?: Filter;
}

export default function FilterChangeLabel({
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
          {previousState.hint !== currentState.hint && (
            <li>hint changed to: {currentState.hint}</li>
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
      {state.label} (id: <code>{state.id}</code>, column:{' '}
      <code>{state.column}</code>)
    </>
  );
}
