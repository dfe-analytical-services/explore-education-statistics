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

/**
 * Apply previous ordering to default table headers.
 */
const applyTableHeadersOrder = ({
  reorderedTableHeaders,
  defaultTableHeaders,
}: {
  reorderedTableHeaders: TableHeadersConfig;
  defaultTableHeaders: TableHeadersConfig;
}): TableHeadersConfig => {
  const defaultRowGroups = [
    ...defaultTableHeaders.rowGroups,
    defaultTableHeaders.rows,
  ];
  const defaultColGroups = [
    ...defaultTableHeaders.columnGroups,
    defaultTableHeaders.columns,
  ];
  const reorderedRowGroups = [
    ...reorderedTableHeaders.rowGroups,
    reorderedTableHeaders.rows,
  ];
  const reorderedColGroups = [
    ...reorderedTableHeaders.columnGroups,
    reorderedTableHeaders.columns,
  ];

  const { orderedGroups: orderedRowGroups } = orderGroups({
    defaultGroups: defaultRowGroups,
    reorderedGroups: reorderedRowGroups,
    otherAxisDefaultGroups: defaultColGroups,
    otherAxisReorderedGroups: reorderedColGroups,
  });

  const moveNewColumnGroupsToRows = orderedRowGroups.length <= 1;

  const {
    orderedGroups: allOrderedColumnGroups,
    newGroups: newColumnGroups,
  } = orderGroups({
    defaultGroups: defaultColGroups,
    reorderedGroups: reorderedColGroups,
    otherAxisDefaultGroups: defaultRowGroups,
    otherAxisReorderedGroups: reorderedRowGroups,
    returnNewGroups: moveNewColumnGroupsToRows,
  });

  const allOrderedColumnGroupsLength = allOrderedColumnGroups.length;

  // If there's zero or one row groups then add any new column groups
  // to row groups instead to avoid causing horizontal scrolling
  // by having lots of columns.
  const allOrderedRowGroups = moveNewColumnGroupsToRows
    ? [...orderedRowGroups, ...newColumnGroups]
    : orderedRowGroups;
  const allOrderedRowGroupsLength = allOrderedRowGroups.length;

  // For safety - can't think how this would be possible,
  // but just in case, return the default headers so the table doesn't break.
  if (!allOrderedColumnGroupsLength && !allOrderedRowGroupsLength) {
    return defaultTableHeaders;
  }

  // Make sure columns is populated by taking the first of the rows groups
  if (!allOrderedColumnGroupsLength) {
    return {
      columnGroups: [],
      columns: allOrderedRowGroups[0],
      rowGroups: allOrderedRowGroups.slice(1, -1),
      rows: allOrderedRowGroups[allOrderedRowGroupsLength - 1],
    };
  }

  // Make sure rows is populated by taking the first of the columns groups
  if (!allOrderedRowGroupsLength) {
    return {
      columnGroups: allOrderedColumnGroups.slice(1, -1),
      columns: allOrderedColumnGroups[allOrderedColumnGroupsLength - 1],
      rowGroups: [],
      rows: allOrderedColumnGroups[0],
    };
  }

  return {
    columnGroups: allOrderedColumnGroups.slice(0, -1),
    columns: allOrderedColumnGroups[allOrderedColumnGroupsLength - 1],
    rowGroups: allOrderedRowGroups.slice(0, -1),
    rows: allOrderedRowGroups[allOrderedRowGroupsLength - 1],
  };
};

/**
 * Order the groups for an axis:
 * - filters the reordered groups to remove those no longer in the headers,
 * checks both axes
 * - adds groups that have been added to the headers after the reordered ones
 * or in default order if groups have not been reordered
 * - if returnNewGroups is true then the new groups are returned from here
 * instead of being added to the reordered groups
 */
function orderGroups({
  defaultGroups,
  reorderedGroups,
  otherAxisDefaultGroups,
  otherAxisReorderedGroups,
  returnNewGroups = false,
}: {
  defaultGroups: Filter[][];
  reorderedGroups: Filter[][];
  otherAxisDefaultGroups: Filter[][];
  otherAxisReorderedGroups: Filter[][];
  returnNewGroups?: boolean;
}): { orderedGroups: Filter[][]; newGroups: Filter[][] } {
  const orderedGroups = filterReorderedGroups({
    reorderedGroups,
    defaultGroups,
    otherAxisDefaultGroups,
  });

  const reorderedGroupsOrder = reorderedGroups.map(group =>
    getFilterType(group[0]),
  );

  const defaultGroupsOrder = defaultGroups.map(group =>
    getFilterType(group[0]),
  );

  const otherAxisReorderedGroupsOrder = otherAxisReorderedGroups.map(group =>
    getFilterType(group[0]),
  );

  const otherAxisDefaultGroupsOrder = otherAxisDefaultGroups.map(group =>
    getFilterType(group[0]),
  );

  const groupsAreReordered = areGroupsReordered({
    defaultOrder: defaultGroupsOrder,
    otherAxisDefaultOrder: otherAxisDefaultGroupsOrder,
    previousOrder: reorderedGroupsOrder,
    previousOtherAxisOrder: otherAxisReorderedGroupsOrder,
  });

  const newGroups = getNewGroups({
    otherAxisReorderedGroups,
    reorderedGroups,
    defaultGroups,
  });

  const allGroups = groupsAreReordered
    ? [...orderedGroups, ...newGroups]
    : [...orderedGroups, ...newGroups].sort((a, b) => {
        return (
          defaultGroupsOrder.indexOf(getFilterType(a[0])) -
          defaultGroupsOrder.indexOf(getFilterType(b[0]))
        );
      });

  return {
    orderedGroups: returnNewGroups ? orderedGroups : allGroups,
    newGroups: returnNewGroups ? newGroups : [],
  };
}

/**
 * Filter reordered groups by ones that exist in either axis of default groups.
 * If the group items were reordered maintain that order,
 * if not use default order.
 */
function filterReorderedGroups({
  reorderedGroups,
  defaultGroups,
  otherAxisDefaultGroups,
}: {
  reorderedGroups: Filter[][];
  defaultGroups: Filter[][];
  otherAxisDefaultGroups: Filter[][];
}): Filter[][] {
  return reorderedGroups.reduce<Filter[][]>((acc, group) => {
    const matchingDefaultGroup = getMatchingGroup(defaultGroups, group);
    if (matchingDefaultGroup) {
      const groupWasReordered = isReorderedGroup(group, matchingDefaultGroup);
      acc.push(
        groupWasReordered
          ? filterItemsAndAddNew(group, matchingDefaultGroup)
          : matchingDefaultGroup,
      );
      return acc;
    }

    const matchingGroupInOtherAxis = getMatchingGroup(
      otherAxisDefaultGroups,
      group,
    );
    if (matchingGroupInOtherAxis) {
      const groupWasReorderedInOtherAxis = isReorderedGroup(
        group,
        matchingGroupInOtherAxis,
      );
      acc.push(
        groupWasReorderedInOtherAxis
          ? filterItemsAndAddNew(group, matchingGroupInOtherAxis)
          : matchingGroupInOtherAxis,
      );
    }

    return acc;
  }, []);
}

/**
 * Filter reordered items to those in the default group.
 * Add new items from the default group.
 */
function filterItemsAndAddNew(
  reorderedItems: Filter[],
  defaultItems: Filter[],
): Filter[] {
  return reorderedItems
    .filter(item =>
      defaultItems.find(defaultItem => defaultItem.value === item.value),
    )
    .concat(
      defaultItems.filter(
        defaultItem =>
          !reorderedItems.find(item => item.value === defaultItem.value),
      ),
    );
}

function getMatchingGroup(
  groups: Filter[][],
  groupToMatch: Filter[],
): Filter[] | undefined {
  const groupToMatchFilterType = getFilterType(groupToMatch[0]);
  return groups.find(
    group => getFilterType(group[0]) === groupToMatchFilterType,
  );
}

function getNewGroups({
  otherAxisReorderedGroups,
  reorderedGroups,
  defaultGroups,
}: {
  otherAxisReorderedGroups: Filter[][];
  reorderedGroups: Filter[][];
  defaultGroups: Filter[][];
}): Filter[][] {
  return defaultGroups.filter(group => {
    const matchingGroupInAxis = getMatchingGroup(reorderedGroups, group);
    const matchingGroupInOtherAxis = getMatchingGroup(
      otherAxisReorderedGroups,
      group,
    );
    return !matchingGroupInAxis && !matchingGroupInOtherAxis;
  });
}

function isReorderedGroup(
  originalGroup: Filter[],
  defaultGroup: Filter[],
): boolean {
  const intersectionOrderedByOriginal = intersectionWith(
    originalGroup,
    defaultGroup,
    isEqual,
  );
  const intersectionOrderedByDefault = intersectionWith(
    defaultGroup,
    originalGroup,
    isEqual,
  );
  return !isEqual(intersectionOrderedByOriginal, intersectionOrderedByDefault);
}

/**
 * Have the groups in an axis been re-ordered or moved between axes
 * */
function areGroupsReordered({
  defaultOrder,
  otherAxisDefaultOrder,
  previousOrder,
  previousOtherAxisOrder,
}: {
  defaultOrder: string[];
  otherAxisDefaultOrder: string[];
  previousOrder: string[];
  previousOtherAxisOrder: string[];
}): boolean {
  if (isEqual(previousOrder, defaultOrder)) {
    return false;
  }

  const intersectionOrderedByPrevious = intersection(
    previousOrder,
    defaultOrder,
  );
  const intersectionOrderedByDefault = intersection(
    defaultOrder,
    previousOrder,
  );
  if (!isEqual(intersectionOrderedByPrevious, intersectionOrderedByDefault)) {
    return true;
  }
  const anyGroupsMovedOutOfAxis = previousOrder.find(group =>
    otherAxisDefaultOrder.includes(group),
  );
  const anyGroupsMovedIntoAxis = defaultOrder.find(group =>
    previousOtherAxisOrder.includes(group),
  );

  if (anyGroupsMovedOutOfAxis || anyGroupsMovedIntoAxis) {
    return true;
  }
  return false;
}

function getFilterType(filter: Filter): string {
  if (filter instanceof CategoryFilter) {
    return filter.category;
  }
  if (filter instanceof LocationFilter) {
    return 'locationFilter';
  }
  if (filter instanceof TimePeriodFilter) {
    return 'timePeriodFilter';
  }
  return 'indicator';
}

export default applyTableHeadersOrder;
