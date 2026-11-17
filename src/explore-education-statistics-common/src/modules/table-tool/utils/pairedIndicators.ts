import { IndicatorOption } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';

/**
 * Get the child indicator id for each parent indicator id,
 * using the {@link IndicatorOption.parentIndicatorId} of child indicators.
 */
export function getPairedIndicatorChildIds(
  indicatorGroups: { options: IndicatorOption[] }[],
): Dictionary<string> {
  return indicatorGroups
    .flatMap(group => group.options)
    .reduce<Dictionary<string>>((acc, option) => {
      if (option.parentIndicatorId) {
        acc[option.parentIndicatorId] = option.value;
      }
      return acc;
    }, {});
}

/**
 * Add the parent indicator of any child indicator selected without it,
 * keeping the child indicator. Use this before {@link normalisePairedIndicators}
 * for queries that weren't built with the filters form (e.g. from table tool
 * search), where a child indicator can be requested on its own.
 */
export function addMissingParentIndicators(
  selectedIndicators: string[],
  childIdsByParentId: Dictionary<string>,
): string[] {
  const parentIdsByChildId = Object.fromEntries(
    Object.entries(childIdsByParentId).map(([parentId, childId]) => [
      childId,
      parentId,
    ]),
  );

  const result = new Set<string>();

  selectedIndicators.forEach(indicator => {
    const parentId = parentIdsByChildId[indicator];

    if (parentId) {
      result.add(parentId);
    }

    result.add(indicator);
  });

  return [...result];
}

/**
 * Ensure the child indicator of every selected parent indicator is selected,
 * and that no child indicator is selected without its parent indicator.
 */
export function normalisePairedIndicators(
  selectedIndicators: string[],
  childIdsByParentId: Dictionary<string>,
): string[] {
  const childIds = new Set(Object.values(childIdsByParentId));

  if (childIds.size === 0) {
    return selectedIndicators;
  }

  const parentIds = selectedIndicators.filter(
    indicator => !childIds.has(indicator),
  );

  const result = new Set<string>();

  parentIds.forEach(indicator => {
    result.add(indicator);

    const childId = childIdsByParentId[indicator];

    if (childId) {
      result.add(childId);
    }
  });

  return [...result];
}
