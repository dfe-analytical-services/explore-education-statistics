import camelCase from 'lodash/camelCase';

export const mappableTableId = (groupKey: string) =>
  `mappable-table-${camelCase(groupKey)}`;

export const autoMappedTableId = (groupKey: string) =>
  `auto-mapped-table-${camelCase(groupKey)}`;
