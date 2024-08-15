import FilterChangeLabel from '@common/modules/data-catalogue/components/FilterChangeLabel';
import FilterOptionChangeLabel from '@common/modules/data-catalogue/components/FilterOptionChangeLabel';
import GeographicLevelChangeLabel from '@common/modules/data-catalogue/components/GeographicLevelChangeLabel';
import IndicatorChangeLabel from '@common/modules/data-catalogue/components/IndicatorChangeLabel';
import LocationGroupChangeLabel from '@common/modules/data-catalogue/components/LocationGroupChangeLabel';
import LocationOptionChangeLabel from '@common/modules/data-catalogue/components/LocationOptionChangeLabel';
import TimePeriodOptionChangeLabel from '@common/modules/data-catalogue/components/TimePeriodOptionChangeLabel';
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
import React, { useMemo } from 'react';

const metaTypeLabels = {
  filters: 'filters',
  filterOptions: 'filter options',
  geographicLevels: 'geographic level options',
  indicators: 'indicators',
  locationGroups: 'location groups',
  locationOptions: 'location options',
  timePeriods: 'time periods',
} satisfies Record<keyof ChangeSet, string>;

export interface ChangeListProps<TState> {
  changes?: Change<TState>[];
  metaType: keyof ChangeSet;
  metaTypeLabel?: string;
  testId?: string;
}

export default function ChangeList<TState>({
  changes = [],
  metaType,
  metaTypeLabel: customMetaTypeLabel,
  testId,
}: ChangeListProps<TState>) {
  const { additions, deletions, updates } = useMemo(() => {
    return changes.reduce<{
      additions: Change<TState>[];
      updates: Change<TState>[];
      deletions: Change<TState>[];
    }>(
      (acc, change) => {
        if (change.currentState && change.previousState) {
          acc.updates.push(change);
          return acc;
        }

        if (change.currentState) {
          acc.additions.push(change);
        } else if (change.previousState) {
          acc.deletions.push(change);
        }

        return acc;
      },
      {
        additions: [],
        deletions: [],
        updates: [],
      },
    );
  }, [changes]);

  if (!changes.length) {
    return null;
  }

  const metaTypeLabel = customMetaTypeLabel ?? metaTypeLabels[metaType];

  const hasSingleChangeType =
    [additions.length, deletions.length, updates.length].filter(
      length => length > 0,
    ).length === 1;

  return (
    <>
      {!hasSingleChangeType && <h4>Changes to {metaTypeLabel}</h4>}

      {deletions.length > 0 && (
        <div data-testid={`deleted-${testId ?? metaType}`}>
          {hasSingleChangeType ? (
            <h4>Deleted {metaTypeLabel}</h4>
          ) : (
            <h5>Deleted</h5>
          )}

          <ul>
            {deletions.map((change, index) => (
              <li
                // eslint-disable-next-line react/no-array-index-key
                key={index}
                data-testid="deleted-item"
              >
                <ChangeItem change={change} metaType={metaType} />
              </li>
            ))}
          </ul>
        </div>
      )}

      {updates.length > 0 && (
        <div data-testid={`updated-${testId ?? metaType}`}>
          {hasSingleChangeType ? (
            <h4>Updated {metaTypeLabel}</h4>
          ) : (
            <h5>Updated</h5>
          )}

          <ul>
            {updates.map((change, index) => (
              <li
                // eslint-disable-next-line react/no-array-index-key
                key={index}
                data-testid="updated-item"
              >
                <ChangeItem change={change} metaType={metaType} />
              </li>
            ))}
          </ul>
        </div>
      )}

      {additions.length > 0 && (
        <div data-testid={`added-${testId ?? metaType}`}>
          {hasSingleChangeType ? <h4>New {metaTypeLabel}</h4> : <h5>New</h5>}

          <ul>
            {additions.map((change, index) => (
              <li
                // eslint-disable-next-line react/no-array-index-key
                key={index}
                data-testid="added-item"
              >
                <ChangeItem change={change} metaType={metaType} />
              </li>
            ))}
          </ul>
        </div>
      )}
    </>
  );
}

interface ChangeItemProps<TState> {
  change: Change<TState>;
  metaType: keyof ChangeSet;
}

function ChangeItem<TState>({ change, metaType }: ChangeItemProps<TState>) {
  const { previousState, currentState } = change;

  switch (metaType) {
    case 'filters':
      return (
        <FilterChangeLabel
          currentState={currentState as Filter}
          previousState={previousState as Filter}
        />
      );
    case 'filterOptions':
      return (
        <FilterOptionChangeLabel
          currentState={currentState as FilterOption}
          previousState={previousState as FilterOption}
        />
      );
    case 'geographicLevels':
      return (
        <GeographicLevelChangeLabel
          currentState={currentState as GeographicLevel}
          previousState={previousState as GeographicLevel}
        />
      );
    case 'indicators':
      return (
        <IndicatorChangeLabel
          currentState={currentState as IndicatorOption}
          previousState={previousState as IndicatorOption}
        />
      );
    case 'locationGroups':
      return (
        <LocationGroupChangeLabel
          currentState={currentState as LocationGroup}
          previousState={previousState as LocationGroup}
        />
      );
    case 'locationOptions':
      return (
        <LocationOptionChangeLabel
          currentState={currentState as LocationOption}
          previousState={previousState as LocationOption}
        />
      );
    case 'timePeriods':
      return (
        <TimePeriodOptionChangeLabel
          currentState={currentState as TimePeriodOption}
          previousState={previousState as TimePeriodOption}
        />
      );
    default:
      throw new Error(`Unrecognised meta type '${metaType}'`);
  }
}
