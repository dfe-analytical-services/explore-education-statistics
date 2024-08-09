import { Change } from '@common/services/types/apiDataSetChanges';

export interface ChangesByType<TChange extends Change<unknown>> {
  additions: TChange[];
  deletions: TChange[];
  updates: TChange[];
}

export default function groupByChangeType<TChange extends Change<unknown>>(
  changes: TChange[],
): ChangesByType<TChange> {
  return changes.reduce<ChangesByType<TChange>>(
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
}
