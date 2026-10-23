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
