import {
  FilterItemReplacement,
  FootnoteReplacementPlan,
  TargetReplacement,
} from '@admin/services/dataReplacementService';
import { Dictionary } from '@common/types';
import deepmerge from 'deepmerge';

export interface MergedFootnoteFilterReplacement extends TargetReplacement {
  groups: Dictionary<MergedFootnoteFilterGroupReplacement>;
  isAllSelected: boolean;
}

export interface MergedFootnoteFilterGroupReplacement
  extends TargetReplacement {
  filters: FilterItemReplacement[];
  isAllSelected: boolean;
}

/**
 * Merge all of the filter, filter group and filter item
 * properties of a {@param footnoteReplacementPlan} into a
 * single dictionary with nested groupings.
 */
export default function mergeReplacementFootnoteFilters(
  footnoteReplacementPlan: FootnoteReplacementPlan,
): Dictionary<MergedFootnoteFilterReplacement> {
  let mergedFilters: Dictionary<MergedFootnoteFilterReplacement> = {};

  footnoteReplacementPlan.filterItems.forEach(filterItem => {
    const {
      filterId,
      filterLabel,
      filterGroupId,
      filterGroupLabel,
      ...item
    } = filterItem;

    mergedFilters = deepmerge(mergedFilters, {
      [filterId]: {
        id: filterId,
        label: filterLabel,
        valid: true,
        groups: {
          [filterGroupId]: {
            id: filterGroupId,
            label: filterGroupLabel,
            valid: true,
            isAllSelected: false,
            filters: [{ ...item }],
          },
        },
        isAllSelected: false,
      },
    });
  });

  footnoteReplacementPlan.filterGroups.forEach(filterGroup => {
    const { filterId, filterLabel, ...group } = filterGroup;

    mergedFilters = deepmerge(mergedFilters, {
      [filterId]: {
        id: filterId,
        label: filterLabel,
        valid: true,
        groups: {
          [group.id]: {
            ...group,
            filters: [],
            isAllSelected: true,
          },
        },
        isAllSelected: false,
      },
    });
  });

  footnoteReplacementPlan.filters.forEach(filter => {
    mergedFilters = deepmerge(mergedFilters, {
      [filter.id]: {
        ...filter,
        groups: {},
        isAllSelected: true,
      },
    });
  });

  return mergedFilters;
}
