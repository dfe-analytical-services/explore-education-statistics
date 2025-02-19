import { FilterOption } from '@common/services/types/apiDataSetMeta';
import React from 'react';

interface Props {
  currentState?: FilterOption;
  previousState?: FilterOption;
}

export default function FilterOptionChangeLabel({
  currentState,
  previousState,
}: Props) {
  if (previousState && currentState) {
    return (
      <>
        {previousState.label} (id: <code>{previousState.id}</code>):
        <ul>
          {previousState.label !== currentState.label && (
            <li>label changed to: {currentState.label}</li>
          )}
          {previousState.id !== currentState.id && (
            <li>
              id changed to: <code>{currentState.id}</code>
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
