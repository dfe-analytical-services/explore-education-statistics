import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import {
  CategoryFilter,
  LocationFilter,
  TimePeriodFilter,
  Filter,
} from '@common/modules/table-tool/types/filters';
import isEqual from 'lodash/isEqual';
import intersectionWith from 'lodash/intersectionWith';
import intersection from 'lodash/intersection';

type GroupsKeys = 'rowGroups' | 'columnGroups';
type LastGroupKeys = 'rows' | 'columns';
type Axes = 'row' | 'column';

interface Keys {
  groupsKey: GroupsKeys;
  lastGroupKey: LastGroupKeys;
  otherAxisGroupsKey: GroupsKeys;
  otherAxisLastGroupKey: LastGroupKeys;
}
const getKeys = (axis: Axes): Keys => {
  return {
    groupsKey: `${axis}Groups`,
    lastGroupKey: `${axis}s`,
    otherAxisGroupsKey: axis === 'row' ? 'columnGroups' : 'rowGroups',
    otherAxisLastGroupKey: axis === 'row' ? 'columns' : 'rows',
  };
};

const getFilterType = (filter: Filter): string => {
  if (filter instanceof CategoryFilter) {
    return (filter as CategoryFilter).category;
  }
  if (filter instanceof LocationFilter) {
    return 'locationFilter';
  }
  if (filter instanceof TimePeriodFilter) {
    return 'timePeriodFilter';
  }
  return 'indicator';
};

const getMatchingGroup = (
  groups: Filter[][],
  groupToMatch: Filter[],
): Filter[] | undefined => {
  const groupToMatchFilterType = getFilterType(groupToMatch[0]);
  return groups.find(
    group => getFilterType(group[0]) === groupToMatchFilterType,
  );
};

/**
 * Order items within a group based on the previous reordering.
 * Items that weren't in the reordered list will be after the reordered ones.
 */
const orderItems = (
  reorderedItems: Filter[],
  updatedItems: Filter[],
): Filter[] => {
  return reorderedItems
    .filter(item =>
      updatedItems.find(updatedItem => updatedItem.value === item.value),
    )
    .concat(
      updatedItems.filter(
        updatedItem =>
          !reorderedItems.find(item => item.value === updatedItem.value),
      ),
    );
};

const isReorderedGroup = (
  originalGroup: Filter[],
  updatedGroup: Filter[],
): boolean => {
  const intersectionOrderedByOriginal = intersectionWith(
    originalGroup,
    updatedGroup,
    isEqual,
  );
  const intersectionOrderedByUpdated = intersectionWith(
    updatedGroup,
    originalGroup,
    isEqual,
  );
  return !isEqual(intersectionOrderedByOriginal, intersectionOrderedByUpdated);
};

/**
 * Have the groups in an axis been re-ordered or moved between axes
 * */
const areGroupsReordered = (
  previousOrder: string[],
  previousOtherAxisOrder: string[],
  updatedOrder: string[],
  updatedOtherAxisOrder: string[],
) => {
  if (isEqual(previousOrder, updatedOrder)) {
    return false;
  }

  const intersectionOrderedByPrevious = intersection(
    previousOrder,
    updatedOrder,
  );
  const intersectionOrderedByUpdated = intersection(
    updatedOrder,
    previousOrder,
  );
  if (!isEqual(intersectionOrderedByPrevious, intersectionOrderedByUpdated)) {
    return true;
  }
  const anyGroupsMovedOutOfAxis = previousOrder.find(group =>
    updatedOtherAxisOrder.includes(group),
  );
  const anyGroupsMovedIntoAxis = updatedOrder.find(group =>
    previousOtherAxisOrder.includes(group),
  );

  if (anyGroupsMovedOutOfAxis || anyGroupsMovedIntoAxis) {
    return true;
  }
  return false;
};

const getAdditionalGroups = (
  otherAxisReorderedGroups: Filter[][],
  reorderedGroups: Filter[][],
  updatedGroups: Filter[][],
) => {
  return updatedGroups.filter(group => {
    const matchingGroupInAxis = getMatchingGroup(reorderedGroups, group);
    const matchingGroupInOtherAxis = getMatchingGroup(
      otherAxisReorderedGroups,
      group,
    );
    return !matchingGroupInAxis && !matchingGroupInOtherAxis;
  });
};

/**
 * Filter the reordered groups by ones that exist in either axis of the updated groups.
 * If the group items were reordered, or were reordered in the original saved version, then maintain that order, if not use the default order.
 */
const filterAndOrderReorderedGroups = ({
  allOriginalGroups,
  reorderedGroups,
  updatedGroups,
  updatedGroupsOnOtherAxis,
}: {
  allOriginalGroups: Filter[][];
  reorderedGroups: Filter[][];
  updatedGroups: Filter[][];
  updatedGroupsOnOtherAxis: Filter[][];
}): Filter[][] => {
  return reorderedGroups.reduce<Filter[][]>((acc, group) => {
    const matchingUpdatedGroup = getMatchingGroup(updatedGroups, group);
    const matchingOriginalGroup = getMatchingGroup(allOriginalGroups, group);
    const groupWasReordered = matchingUpdatedGroup
      ? isReorderedGroup(group, matchingUpdatedGroup)
      : false;

    if (matchingUpdatedGroup) {
      const originalWasReordered = matchingOriginalGroup
        ? isReorderedGroup(matchingOriginalGroup, matchingUpdatedGroup)
        : false;
      acc.push(
        groupWasReordered || originalWasReordered
          ? orderItems(group, matchingUpdatedGroup)
          : matchingUpdatedGroup,
      );
      return acc;
    }

    const matchingGroupInOtherAxis = getMatchingGroup(
      updatedGroupsOnOtherAxis,
      group,
    );

    if (matchingGroupInOtherAxis) {
      const groupWasReorderedInOtherAxis = isReorderedGroup(
        group,
        matchingGroupInOtherAxis,
      );
      const originalWasReorderedInOtherAxis = matchingOriginalGroup
        ? isReorderedGroup(matchingOriginalGroup, matchingGroupInOtherAxis)
        : false;

      acc.push(
        groupWasReorderedInOtherAxis || originalWasReorderedInOtherAxis
          ? orderItems(group, matchingGroupInOtherAxis)
          : matchingGroupInOtherAxis,
      );
    }

    return acc;
  }, []);
};

/**
 * Filter the original groups by ones that exist in either axis of the updated groups.
 * If the group items were reordered then maintain that order, if not use the default order.
 */
const filterAndOrderOriginalGroups = ({
  originalGroups,
  updatedGroups,
  updatedGroupsOnOtherAxis,
}: {
  originalGroups: Filter[][];
  updatedGroups: Filter[][];
  updatedGroupsOnOtherAxis: Filter[][];
}): Filter[][] => {
  return originalGroups.reduce<Filter[][]>((acc, group) => {
    const matchingGroup = getMatchingGroup(updatedGroups, group);
    if (matchingGroup) {
      acc.push(
        isReorderedGroup(group, matchingGroup)
          ? orderItems(group, matchingGroup)
          : matchingGroup,
      );
      return acc;
    }

    const matchingGroupInOtherAxis = updatedGroupsOnOtherAxis
      ? getMatchingGroup(updatedGroupsOnOtherAxis, group)
      : undefined;
    if (matchingGroupInOtherAxis) {
      acc.push(
        isReorderedGroup(group, matchingGroupInOtherAxis)
          ? orderItems(group, matchingGroupInOtherAxis)
          : matchingGroupInOtherAxis,
      );
      return acc;
    }

    return acc;
  }, []);
};

/**
 * Order the groups for an axis:
 * - filters the reordered groups to remove those no longer in the headers (checks both axes)
 * - adds groups that have been added to the headers after the reordered ones or in default order if groups have not been reordered
 */
const orderGroups = ({
  axis,
  originalTableHeaders,
  reorderedTableHeaders,
  updatedTableHeaders,
}: {
  axis: Axes;
  reorderedTableHeaders?: TableHeadersConfig;
  originalTableHeaders?: TableHeadersConfig;
  updatedTableHeaders: TableHeadersConfig;
}): Filter[][] => {
  const {
    groupsKey,
    lastGroupKey,
    otherAxisGroupsKey,
    otherAxisLastGroupKey,
  } = getKeys(axis);
  const reorderedGroups = reorderedTableHeaders
    ? [...reorderedTableHeaders[groupsKey], reorderedTableHeaders[lastGroupKey]]
    : undefined;
  const updatedGroups = [
    ...updatedTableHeaders[groupsKey],
    updatedTableHeaders[lastGroupKey],
  ];
  const reorderedGroupsOnOtherAxis = reorderedTableHeaders
    ? [
        ...reorderedTableHeaders[otherAxisGroupsKey],
        reorderedTableHeaders[otherAxisLastGroupKey],
      ]
    : [];
  const updatedGroupsOnOtherAxis = [
    ...updatedTableHeaders[otherAxisGroupsKey],
    updatedTableHeaders[otherAxisLastGroupKey],
  ];
  const originalGroups = originalTableHeaders
    ? [...originalTableHeaders[groupsKey], originalTableHeaders[lastGroupKey]]
    : [];
  const originalGroupsOnOtherAxis = originalTableHeaders
    ? [
        ...originalTableHeaders[otherAxisGroupsKey],
        originalTableHeaders[otherAxisLastGroupKey],
      ]
    : [];

  const orderedGroups = reorderedGroups
    ? filterAndOrderReorderedGroups({
        allOriginalGroups: [...originalGroups, ...originalGroupsOnOtherAxis],
        reorderedGroups,
        updatedGroups,
        updatedGroupsOnOtherAxis,
      })
    : filterAndOrderOriginalGroups({
        originalGroups,
        updatedGroups,
        updatedGroupsOnOtherAxis,
      });

  const reorderedGroupsOrder = reorderedGroups
    ? reorderedGroups.map(group => getFilterType(group[0]))
    : undefined;
  const updatedGroupsOrder = updatedGroups.map(group =>
    getFilterType(group[0]),
  );
  const updatedGroupsOnOtherAxisOrder = updatedGroupsOnOtherAxis.map(group =>
    getFilterType(group[0]),
  );
  const originalGroupsOrder = originalGroups.map(group =>
    getFilterType(group[0]),
  );
  const originalGroupsOnOtherAxisOrder = originalGroupsOnOtherAxis.map(group =>
    getFilterType(group[0]),
  );
  const reorderedGroupsOnOtherAxisOrder = reorderedGroupsOnOtherAxis.map(
    group => getFilterType(group[0]),
  );

  const groupsAreReordered = reorderedGroupsOrder
    ? areGroupsReordered(
        reorderedGroupsOrder,
        reorderedGroupsOnOtherAxisOrder,
        updatedGroupsOrder,
        updatedGroupsOnOtherAxisOrder,
      )
    : false;

  const originalGroupsWereReordered = originalGroupsOrder
    ? areGroupsReordered(
        originalGroupsOrder,
        originalGroupsOnOtherAxisOrder,
        updatedGroupsOrder,
        updatedGroupsOnOtherAxisOrder,
      )
    : false;

  const additionalGroups =
    reorderedGroupsOnOtherAxis && reorderedGroups
      ? getAdditionalGroups(
          reorderedGroupsOnOtherAxis,
          reorderedGroups,
          updatedGroups,
        )
      : getAdditionalGroups(
          originalGroupsOnOtherAxis,
          originalGroups,
          updatedGroups,
        );

  // If the groups were reordered append new groups to the end, else maintain the default order.
  return groupsAreReordered || originalGroupsWereReordered
    ? [...orderedGroups, ...additionalGroups]
    : [...orderedGroups, ...additionalGroups].sort((a, b) => {
        return (
          updatedGroupsOrder.indexOf(getFilterType(a[0])) -
          updatedGroupsOrder.indexOf(getFilterType(b[0]))
        );
      });
};

/**
 * Apply previous ordering, from the saved order and reordering, to updated table headers.
 */
const applyTableHeadersOrder = ({
  originalTableHeaders,
  reorderedTableHeaders,
  updatedTableHeaders,
}: {
  originalTableHeaders?: TableHeadersConfig;
  reorderedTableHeaders?: TableHeadersConfig;
  updatedTableHeaders: TableHeadersConfig;
}): TableHeadersConfig => {
  if (!reorderedTableHeaders && !originalTableHeaders) {
    return updatedTableHeaders;
  }

  const allOrderedColumnGroups = orderGroups({
    axis: 'column',
    originalTableHeaders,
    reorderedTableHeaders,
    updatedTableHeaders,
  });

  const allOrderedRowGroups = orderGroups({
    axis: 'row',
    originalTableHeaders,
    reorderedTableHeaders,
    updatedTableHeaders,
  });

  const allOrderedColumnGroupsLength = allOrderedColumnGroups.length;
  const allOrderedRowGroupsLength = allOrderedRowGroups.length;

  // For safety - can't think how this would be possible, but just in case, return the updated headers so the table doesn't break.
  if (!allOrderedColumnGroupsLength && !allOrderedRowGroupsLength) {
    return updatedTableHeaders;
  }

  // Make sure columns is populated by taking the first of the rows groups
  if (!allOrderedColumnGroupsLength) {
    return {
      columnGroups: [],
      columns: allOrderedRowGroups[0],
      rowGroups: allOrderedRowGroups.filter(
        (_, index) => index > 0 && index < allOrderedRowGroupsLength - 1,
      ),
      rows: allOrderedRowGroups[allOrderedRowGroupsLength - 1],
    };
  }

  // Make sure rows is populated by taking the first of the columns groups
  if (!allOrderedRowGroupsLength) {
    return {
      columnGroups: allOrderedColumnGroups.filter(
        (_, index) => index > 0 && index < allOrderedColumnGroupsLength - 1,
      ),
      columns: allOrderedColumnGroups[allOrderedColumnGroupsLength - 1],
      rowGroups: [],
      rows: allOrderedColumnGroups[0],
    };
  }

  return {
    columnGroups: allOrderedColumnGroups.filter(
      (_, index) => index !== allOrderedColumnGroupsLength - 1,
    ),
    columns: allOrderedColumnGroups[allOrderedColumnGroupsLength - 1],
    rowGroups: allOrderedRowGroups.filter(
      (_, index) => index < allOrderedRowGroupsLength - 1,
    ),
    rows: allOrderedRowGroups[allOrderedRowGroupsLength - 1],
  };
};

export default applyTableHeadersOrder;
