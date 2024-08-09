import { Change, ChangeSet } from '@common/services/types/apiDataSetChanges';
import {
  Filter,
  FilterOption,
  GeographicLevel,
  IndicatorOption,
  LocationGroup,
  LocationOption,
  TimePeriodOption,
} from '@common/services/types/apiDataSetMeta';
import intersperse from '@common/utils/array/intersperse';
import getLocationCodeEntries, {
  LocationCodeLabels,
} from '@common/utils/getLocationCodeEntries';
import React from 'react';

const locationCodeLabels: Partial<LocationCodeLabels> = {
  code: 'code',
  id: 'id',
  oldCode: 'old code',
};

export interface ChangeItemProps<TState> {
  change: Change<TState>;
  metaType: keyof ChangeSet;
}

export default function ChangeItem<TState>({
  change,
  metaType,
}: ChangeItemProps<TState>) {
  const { previousState, currentState } = change;

  if (previousState && currentState) {
    return (
      <li>
        <StateUpdateLabel
          currentState={currentState}
          metaType={metaType}
          previousState={previousState}
        />
      </li>
    );
  }

  if (currentState) {
    return (
      <li>
        <StateLabel state={currentState} metaType={metaType} />
      </li>
    );
  }

  if (previousState) {
    return (
      <li>
        <StateLabel state={previousState} metaType={metaType} />
      </li>
    );
  }

  throw new Error('Change did not contain previous or current state');
}

function StateLabel<TState>({
  state,
  metaType,
}: {
  state: TState;
  metaType: keyof ChangeSet;
}) {
  switch (metaType) {
    case 'filters': {
      const filter = state as Filter;

      return (
        <>
          {filter.label} (id: <code>{filter.id}</code>)
        </>
      );
    }
    case 'filterOptions': {
      const option = state as FilterOption;

      return (
        <>
          {option.label} (id: <code>{option.id}</code>)
        </>
      );
    }
    case 'geographicLevels': {
      const level = state as GeographicLevel;

      return (
        <>
          {level.label} (code: <code>{level.code}</code>)
        </>
      );
    }
    case 'indicators': {
      const indicator = state as IndicatorOption;

      return (
        <>
          {indicator.label} (id: <code>{indicator.id}</code>)
        </>
      );
    }
    case 'locationGroups': {
      const group = state as LocationGroup;

      return (
        <>
          {group.level.label} (code: <code>{group.level.code}</code>)
        </>
      );
    }
    case 'locationOptions': {
      const option = state as LocationOption;

      const codeEntries = getLocationCodeEntries(
        option,
        locationCodeLabels,
      ).map(entry => (
        <>
          {`${entry.label}: `}
          <code>{entry.value}</code>
        </>
      ));

      return (
        <>
          {option.label} ({intersperse(codeEntries, () => ', ')})
        </>
      );
    }
    case 'timePeriods': {
      const option = state as TimePeriodOption;

      return option.label;
    }
    default:
      throw new Error(`Unrecognised meta type '${metaType}'`);
  }
}

function StateUpdateLabel<TState>({
  currentState,
  metaType,
  previousState,
}: {
  currentState: TState;
  metaType: keyof ChangeSet;
  previousState: TState;
}) {
  switch (metaType) {
    case 'filters': {
      const previous = previousState as Filter;
      const current = currentState as Filter;

      return (
        <>
          {previous.label} (id: <code>{previous.id}</code>):
          <ul>
            {previous.label !== current.label && (
              <li>New label: {current.label}</li>
            )}
            {previous.hint !== current.hint && (
              <li>New hint: {current.hint}</li>
            )}
          </ul>
        </>
      );
    }
    case 'filterOptions': {
      const option = currentState as FilterOption;

      return (
        <>
          {option.label} (id: <code>{option.id}</code>)
        </>
      );
    }
    case 'geographicLevels': {
      const level = currentState as GeographicLevel;

      return (
        <>
          {level.label} (code: <code>{level.code}</code>)
        </>
      );
    }
    case 'indicators': {
      const indicator = currentState as IndicatorOption;

      return (
        <>
          {indicator.label} (id: <code>{indicator.id}</code>)
        </>
      );
    }
    case 'locationGroups': {
      const group = currentState as LocationGroup;

      return (
        <>
          {group.level.label} (code: <code>{group.level.code}</code>)
        </>
      );
    }
    case 'locationOptions': {
      const option = currentState as LocationOption;

      const codeEntries = getLocationCodeEntries(
        option,
        locationCodeLabels,
      ).map(entry => (
        <>
          {`${entry.label}: `}
          <code>{entry.value}</code>
        </>
      ));

      return (
        <>
          {option.label} ({intersperse(codeEntries, () => ', ')})
        </>
      );
    }
    case 'timePeriods': {
      const option = currentState as TimePeriodOption;

      return option.label;
    }
    default:
      throw new Error(`Unrecognised meta type '${metaType}'`);
  }
}
