import ChangeItem from '@common/modules/data-catalogue/components/ChangeItem';
import groupByChangeType from '@common/modules/data-catalogue/utils/groupByChangeType';
import { Change, ChangeSet } from '@common/services/types/apiDataSetChanges';
import React from 'react';

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
}

export default function ChangeList<TState>({
  changes = [],
  metaType,
  metaTypeLabel: customMetaTypeLabel,
}: ChangeListProps<TState>) {
  if (!changes.length) {
    return null;
  }

  const { additions, deletions, updates } = groupByChangeType(changes);

  const metaTypeLabel = customMetaTypeLabel ?? metaTypeLabels[metaType];

  return (
    <>
      {deletions.length > 0 && (
        <>
          <h4>Deleted {metaTypeLabel}</h4>

          <ul>
            {deletions.map((change, index) => (
              // eslint-disable-next-line react/no-array-index-key
              <ChangeItem key={index} change={change} metaType={metaType} />
            ))}
          </ul>
        </>
      )}

      {updates.length > 0 && (
        <>
          <h4>Updated {metaTypeLabel}</h4>
          <ul>
            {updates.map((change, index) => (
              // eslint-disable-next-line react/no-array-index-key
              <ChangeItem key={index} change={change} metaType={metaType} />
            ))}
          </ul>
        </>
      )}

      {additions.length > 0 && (
        <>
          <h4>New {metaTypeLabel}</h4>
          <ul>
            {additions.map((change, index) => (
              // eslint-disable-next-line react/no-array-index-key
              <ChangeItem key={index} change={change} metaType={metaType} />
            ))}
          </ul>
        </>
      )}
    </>
  );
}
