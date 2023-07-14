import {
  testTableWithThreeLevelsOfRowAndColHeaders,
  testTableWithThreeLevelsOfRowAndColHeadersConfig,
  testTableWithTwoLevelsOfRowAndColHeaders,
  testTableWithTwoLevelsOfRowAndColHeadersConfig,
  testTableWithOneLevelOfRowAndColHeaders,
  testTableWithOneLevelOfRowAndColHeadersConfig,
  testTableWithTwoLevelsOfRowAndOneLevelOfColHeadersConfig,
  testTableWithOneLevelOfRowsAndTwoLevelsOfColHeadersConfig,
  testTableWithMissingTimePeriod,
} from '@common/modules/table-tool/utils/__data__/testTableData';
import {
  testTableWithOnlyMergedCellsInColumnHeadersConfig,
  testTableWithOnlyMergedCellsInHeaders,
  testTableWithMergedAndUnMergedCellsInHeaders,
  testTableWithMergedAndUnMergedCellsInColumnHeadersConfig,
  testTableWithOnlyMergedCellsInRowHeadersConfig,
  testTableWithMergedAndUnmergedCellsInRowHeadersConfig,
  testTableWithOnlyMergedCellsInFirstLevelOfHeaders,
  testTableWithOnlyMergedCellsInMiddleLevelOfRowsConfig,
  testTableWithOnlyMergedCellsInFirstLevelOfColumnHeadersConfig,
  testTableWithMergedAndUnmergedCellsInFirstLevelOfColumnHeadersConfig,
  testTableWithMergedAndUnmergedCellsInLastLevelOfColumnHeadersConfig,
  testTableWithOnlyMergedCellsInLastLevelOfColumnHeadersConfig,
  testTableWithOnlyMergedCellsInFirstLevelOfRowHeadersConfig,
  testTableWithMergedAndUnmergedCellsInFirstLevelOfHeaders,
  testTableWithMergedAndUnmergedCellsInFirstLevelOfRowHeadersConfig,
  testTableWithOnlyMergedCellsInMiddleLevelOfColumnHeadersConfig,
  testTableWithOnlyMergedCellsInMiddleLevelOfHeaders,
  testTableWithMergedAndUnmergedCellsInMiddleLevelOfColumnHeadersConfig,
  testTableWithMergedAndUnmergedCellsInMiddleLevelOfHeaders,
  testTableWithMergedAndUnmergedCellsInMiddleLevelOfRowHeadersConfig,
  testTableWithOnlyMergedCellsInLastLevelOfHeaders,
  testTableWithOnlyMergedCellsInLastLevelOfRowHeadersConfig,
  testTableWithMergedAndUnmergedCellsInLastLevelOfHeaders,
  testTableWithMergedAndUnmergedCellsInLastLevelOfRowHeadersConfig,
} from '@common/modules/table-tool/utils/__data__/testTableDataWithMergedCells';
import mapTableToJson, {
  TableCellJson,
} from '@common/modules/table-tool/utils/mapTableToJson';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';
import {
  testTableWithDuplicateFilterLabelsInColumnHeadersConfig,
  testTableWithDuplicateFilterLabels,
  testTableWithDuplicateFilterLabelsAndMissingData,
  testTableWithDuplicateFilterLabelsInRowHeadersConfig,
  testTableWithMultipleGroupsWithSameLabelsInColumnHeadersConfig,
  testTableWithMultipleGroupsWithSameLabels,
  testTableWithMultipleGroupsWithSameLabelsInRowHeadersConfig,
} from '@common/modules/table-tool/utils/__data__/testTableDataWithDuplicateLabels';

describe('mapTableToJson', () => {
  test('returns the correct JSON for a table with one level of row and column headers', () => {
    const result = mapTableToJson({
      tableHeadersConfig: testTableWithOneLevelOfRowAndColHeadersConfig,
      subjectMeta: testTableWithOneLevelOfRowAndColHeaders.subjectMeta,
      results: testTableWithOneLevelOfRowAndColHeaders.results,
    }).tableJson;

    expect(result.thead).toEqual<TableCellJson[][]>([
      [
        {
          colSpan: 1,
          rowSpan: 1,
          tag: 'td',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          text: '2012/13',
          tag: 'th',
        },
      ],
    ]);

    expect(result.tbody).toEqual<TableCellJson[][]>([
      [
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'LA 1',
          tag: 'th',
        },
        { tag: 'td', text: '2,763' },
      ],
      [
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'LA 2',
          tag: 'th',
        },
        { tag: 'td', text: '346' },
      ],
    ]);
  });

  test('returns the correct JSON for a table with two levels of row headers and one level of col headers', () => {
    const result = mapTableToJson({
      tableHeadersConfig: testTableWithTwoLevelsOfRowAndOneLevelOfColHeadersConfig,
      subjectMeta: testTableWithOneLevelOfRowAndColHeaders.subjectMeta,
      results: testTableWithOneLevelOfRowAndColHeaders.results,
    }).tableJson;

    expect(result.thead).toEqual<TableCellJson[][]>([
      [
        {
          colSpan: 2,
          rowSpan: 1,
          tag: 'td',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          text: '2012/13',
          tag: 'th',
        },
      ],
    ]);

    expect(result.tbody).toEqual<TableCellJson[][]>([
      [
        {
          rowSpan: 2,
          colSpan: 1,
          scope: 'rowgroup',
          text: 'Indicator 1',
          tag: 'th',
        },
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'LA 1',
          tag: 'th',
        },
        { tag: 'td', text: '2,763' },
      ],
      [
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'LA 2',
          tag: 'th',
        },
        { tag: 'td', text: '346' },
      ],
    ]);
  });

  test('returns the correct JSON for a table with two levels of col headers and one level of row headers', () => {
    const result = mapTableToJson({
      tableHeadersConfig: testTableWithOneLevelOfRowsAndTwoLevelsOfColHeadersConfig,
      subjectMeta: testTableWithOneLevelOfRowAndColHeaders.subjectMeta,
      results: testTableWithOneLevelOfRowAndColHeaders.results,
    }).tableJson;

    expect(result.thead).toEqual<TableCellJson[][]>([
      [
        {
          colSpan: 1,
          rowSpan: 2,
          tag: 'td',
        },
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          text: '2012/13',
          tag: 'th',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          text: 'LA 1',
          tag: 'th',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          text: 'LA 2',
          tag: 'th',
        },
      ],
    ]);

    expect(result.tbody).toEqual<TableCellJson[][]>([
      [
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'Indicator 1',
          tag: 'th',
        },
        { tag: 'td', text: '2,763' },
        { tag: 'td', text: '346' },
      ],
    ]);
  });

  test('returns the correct JSON for a table with two levels of row and column headers', () => {
    const result = mapTableToJson({
      tableHeadersConfig: testTableWithTwoLevelsOfRowAndColHeadersConfig,
      subjectMeta: testTableWithTwoLevelsOfRowAndColHeaders.subjectMeta,
      results: testTableWithTwoLevelsOfRowAndColHeaders.results,
    }).tableJson;

    expect(result.thead).toEqual<TableCellJson[][]>([
      [
        { colSpan: 2, rowSpan: 2, tag: 'td' },
        { colSpan: 2, rowSpan: 1, scope: 'colgroup', tag: 'th', text: 'LA 1' },
        { colSpan: 2, rowSpan: 1, scope: 'colgroup', tag: 'th', text: 'LA 2' },
      ],
      [
        { colSpan: 1, rowSpan: 1, scope: 'col', tag: 'th', text: '2012/13' },
        { colSpan: 1, rowSpan: 1, scope: 'col', tag: 'th', text: '2013/14' },
        { colSpan: 1, rowSpan: 1, scope: 'col', tag: 'th', text: '2012/13' },
        { colSpan: 1, rowSpan: 1, scope: 'col', tag: 'th', text: '2013/14' },
      ],
    ]);

    expect(result.tbody).toEqual<TableCellJson[][]>([
      [
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'rowgroup',
          tag: 'th',
          text: 'Category 2 Filter 1',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'Indicator 1',
        },
        { tag: 'td', text: '331' },
        { tag: 'td', text: '76' },
        { tag: 'td', text: '2,763' },
        { tag: 'td', text: '1,327' },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'Indicator 2',
        },
        { tag: 'td', text: '446' },
        { tag: 'td', text: '378' },
        { tag: 'td', text: '2,817' },
        { tag: 'td', text: '2,016' },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'rowgroup',
          tag: 'th',
          text: 'Category 2 Filter 2',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'Indicator 1',
        },
        { tag: 'td', text: '21,584' },
        { tag: 'td', text: '7,697' },
        { tag: 'td', text: '103,464' },
        { tag: 'td', text: '35,891' },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'Indicator 2',
        },
        { tag: 'td', text: '4,322' },
        { tag: 'td', text: '3,859' },
        { tag: 'td', text: '26,396' },
        { tag: 'td', text: '17,018' },
      ],
    ]);
  });

  test('returns the correct JSON for a table with three levels of row and column headers', () => {
    const result = mapTableToJson({
      tableHeadersConfig: testTableWithThreeLevelsOfRowAndColHeadersConfig,
      subjectMeta: testTableWithThreeLevelsOfRowAndColHeaders.subjectMeta,
      results: testTableWithThreeLevelsOfRowAndColHeaders.results,
    }).tableJson;

    expect(result.thead).toEqual<TableCellJson[][]>([
      [
        { colSpan: 3, rowSpan: 3, tag: 'td' },
        {
          colSpan: 4,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'Category 2 Filter 1',
        },
        {
          colSpan: 4,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'Category 2 Filter 2',
        },
      ],
      [
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'Category 1 Filter 2',
        },
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'Category 1 Filter 3',
        },
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'Category 1 Filter 2',
        },
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'Category 1 Filter 3',
        },
      ],
      [
        { colSpan: 1, rowSpan: 1, scope: 'col', tag: 'th', text: '2012/13' },
        { colSpan: 1, rowSpan: 1, scope: 'col', tag: 'th', text: '2013/14' },
        { colSpan: 1, rowSpan: 1, scope: 'col', tag: 'th', text: '2012/13' },
        { colSpan: 1, rowSpan: 1, scope: 'col', tag: 'th', text: '2013/14' },
        { colSpan: 1, rowSpan: 1, scope: 'col', tag: 'th', text: '2012/13' },
        { colSpan: 1, rowSpan: 1, scope: 'col', tag: 'th', text: '2013/14' },
        { colSpan: 1, rowSpan: 1, scope: 'col', tag: 'th', text: '2012/13' },
        { colSpan: 1, rowSpan: 1, scope: 'col', tag: 'th', text: '2013/14' },
      ],
    ]);

    expect(result.tbody).toEqual<TableCellJson[][]>([
      [
        {
          rowSpan: 4,
          colSpan: 1,
          scope: 'rowgroup',
          text: 'Region 1',
          tag: 'th',
        },
        {
          rowSpan: 2,
          colSpan: 1,
          scope: 'rowgroup',
          text: 'LA 1',
          tag: 'th',
        },
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'Indicator 1',
          tag: 'th',
        },
        { tag: 'td', text: '331' },
        { tag: 'td', text: '44' },
        { tag: 'td', text: '20' },
        { tag: 'td', text: '32' },
        { tag: 'td', text: '19,340' },
        { tag: 'td', text: '7,163' },
        { tag: 'td', text: '2,458' },
        { tag: 'td', text: '767' },
      ],
      [
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'Indicator 2',
          tag: 'th',
        },
        { tag: 'td', text: '428' },
        { tag: 'td', text: '368' },
        { tag: 'td', text: '14' },
        { tag: 'td', text: '6' },
        { tag: 'td', text: '3,830' },
        { tag: 'td', text: '3,413' },
        { tag: 'td', text: '567' },
        { tag: 'td', text: '818' },
      ],
      [
        {
          rowSpan: 2,
          colSpan: 1,
          scope: 'rowgroup',
          text: 'LA 2',
          tag: 'th',
        },
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'Indicator 1',
          tag: 'th',
        },
        { tag: 'td', text: '2,587' },
        { tag: 'td', text: '1,285' },
        { tag: 'td', text: '122' },
        { tag: 'td', text: '42' },
        { tag: 'td', text: '99,656' },
        { tag: 'td', text: '34,012' },
        { tag: 'td', text: '4,872' },
        { tag: 'td', text: '2,362' },
      ],
      [
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'Indicator 2',
          tag: 'th',
        },
        { tag: 'td', text: '2,714' },
        { tag: 'td', text: '1,933' },
        { tag: 'td', text: '127' },
        { tag: 'td', text: '97' },
        { tag: 'td', text: '25,240' },
        { tag: 'td', text: '16,024' },
        { tag: 'td', text: '1,801' },
        { tag: 'td', text: '1,811' },
      ],
      [
        {
          rowSpan: 4,
          colSpan: 1,
          scope: 'rowgroup',
          text: 'Region 2',
          tag: 'th',
        },
        {
          rowSpan: 2,
          colSpan: 1,
          scope: 'rowgroup',
          text: 'LA 3',
          tag: 'th',
        },
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'Indicator 1',
          tag: 'th',
        },
        { tag: 'td', text: '95' },
        { tag: 'td', text: '90' },
        { tag: 'td', text: '366' },
        { tag: 'td', text: '212' },
        { tag: 'td', text: '15,470' },
        { tag: 'td', text: '2,631' },
        { tag: 'td', text: '6,402' },
        { tag: 'td', text: '1,804' },
      ],
      [
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'Indicator 2',
          tag: 'th',
        },
        { tag: 'td', text: '194' },
        { tag: 'td', text: '252' },
        { tag: 'td', text: '227' },
        { tag: 'td', text: '231' },
        { tag: 'td', text: '6,301' },
        { tag: 'td', text: '6,095' },
        { tag: 'td', text: '5,014' },
        { tag: 'td', text: '5,011' },
      ],
      [
        {
          rowSpan: 2,
          colSpan: 1,
          scope: 'rowgroup',
          text: 'LA 4',
          tag: 'th',
        },
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'Indicator 1',
          tag: 'th',
        },
        { tag: 'td', text: '734' },
        { tag: 'td', text: '639' },
        { tag: 'td', text: '56' },
        { tag: 'td', text: '20' },
        { tag: 'td', text: '19,243' },
        { tag: 'td', text: '4,248' },
        { tag: 'td', text: '2,608' },
        { tag: 'td', text: '727' },
      ],
      [
        {
          rowSpan: 1,
          colSpan: 1,
          scope: 'row',
          text: 'Indicator 2',
          tag: 'th',
        },
        { tag: 'td', text: '286' },
        { tag: 'td', text: '294' },
        { tag: 'td', text: '17' },
        { tag: 'td', text: '24' },
        { tag: 'td', text: '7,604' },
        { tag: 'td', text: '6,712' },
        { tag: 'td', text: '955' },
        { tag: 'td', text: '1,059' },
      ],
    ]);
  });

  describe('Handles multiple filter groups with the same labels', () => {
    test('returns the correct JSON when there are multiple groups with the same labels in column headers', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithMultipleGroupsWithSameLabelsInColumnHeadersConfig,
        subjectMeta: testTableWithMultipleGroupsWithSameLabels.subjectMeta,
        results: testTableWithMultipleGroupsWithSameLabels.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 1, rowSpan: 4, tag: 'td' },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'Group 1',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'Group 2',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'Category 3 Group 1 Filter 1',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'Category 3 Group 2 Filter 1',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'Group 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'Group 2',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'Group 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'Group 2',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Category 4 Group 1 Filter 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Category 4 Group 2 Filter 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Category 4 Group 1 Filter 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Category 4 Group 2 Filter 1',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Indicator 1',
          },
          { tag: 'td', text: '20' },
          { tag: 'td', text: '44' },
          { tag: 'td', text: '71' },
          { tag: 'td', text: '32' },
        ],
      ]);
    });

    test('returns the correct JSON when there are multiple groups with the same labels in row headers', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithMultipleGroupsWithSameLabelsInRowHeadersConfig,
        subjectMeta: testTableWithMultipleGroupsWithSameLabels.subjectMeta,
        results: testTableWithMultipleGroupsWithSameLabels.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 4, rowSpan: 1, tag: 'td' },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Indicator 1',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Group 1',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 3 Group 1 Filter 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Group 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 4 Group 1 Filter 1',
          },
          { tag: 'td', text: '20' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Group 2',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 4 Group 2 Filter 1',
          },
          { tag: 'td', text: '44' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Group 2',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 3 Group 2 Filter 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Group 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 4 Group 1 Filter 1',
          },
          { tag: 'td', text: '71' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Group 2',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 4 Group 2 Filter 1',
          },
          { tag: 'td', text: '32' },
        ],
      ]);
    });
  });

  describe('Handles filters in different groups with the same label', () => {
    test('returns the correct JSON when column headers contain filters in different groups with the same label', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithDuplicateFilterLabelsInColumnHeadersConfig,
        subjectMeta: testTableWithDuplicateFilterLabels.subjectMeta,
        results: testTableWithDuplicateFilterLabels.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 1, rowSpan: 3, tag: 'td' },
          {
            colSpan: 4,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Indicator 1',
            tag: 'th',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Filter 1',
            tag: 'th',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Filter 2',
            tag: 'th',
          },
        ],
        [
          { colSpan: 1, rowSpan: 1, scope: 'col', text: 'Filter 1', tag: 'th' },
          { colSpan: 1, rowSpan: 1, scope: 'col', text: 'Filter 2', tag: 'th' },
          { colSpan: 1, rowSpan: 1, scope: 'col', text: 'Filter 1', tag: 'th' },
          { colSpan: 1, rowSpan: 1, scope: 'col', text: 'Filter 2', tag: 'th' },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: '2012/13',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '85',
          },
          {
            tag: 'td',
            text: '88',
          },
          {
            tag: 'td',
            text: '89',
          },
          {
            tag: 'td',
            text: '90',
          },
        ],
      ]);
    });

    test('returns the correct JSON when column headers contain filters in different groups with the same label and have missing data', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithDuplicateFilterLabelsInColumnHeadersConfig,
        subjectMeta:
          testTableWithDuplicateFilterLabelsAndMissingData.subjectMeta,
        results: testTableWithDuplicateFilterLabelsAndMissingData.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 1, rowSpan: 2, tag: 'td' },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Indicator 1',
            tag: 'th',
          },
        ],
        [
          { colSpan: 1, rowSpan: 1, scope: 'col', text: 'Filter 1', tag: 'th' },
          { colSpan: 1, rowSpan: 1, scope: 'col', text: 'Filter 2', tag: 'th' },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: '2012/13',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '85',
          },
          {
            tag: 'td',
            text: '90',
          },
        ],
      ]);
    });

    test('returns the correct JSON when row headers contain filters in different groups with the same label', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithDuplicateFilterLabelsInRowHeadersConfig,
        subjectMeta: testTableWithDuplicateFilterLabels.subjectMeta,
        results: testTableWithDuplicateFilterLabels.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 3, rowSpan: 1, tag: 'td' },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: '2012/13',
            tag: 'th',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            rowSpan: 4,
            colSpan: 1,
            scope: 'rowgroup',
            text: 'Indicator 1',
            tag: 'th',
          },
          {
            rowSpan: 2,
            colSpan: 1,
            scope: 'rowgroup',
            text: 'Filter 1',
            tag: 'th',
          },
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: 'Filter 1',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '85',
          },
        ],
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: 'Filter 2',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '88',
          },
        ],
        [
          {
            rowSpan: 2,
            colSpan: 1,
            scope: 'rowgroup',
            text: 'Filter 2',
            tag: 'th',
          },
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: 'Filter 1',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '89',
          },
        ],
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: 'Filter 2',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '90',
          },
        ],
      ]);
    });

    test('returns the correct JSON when row headers contain filters in different groups with the same label and have missing data', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithDuplicateFilterLabelsInRowHeadersConfig,
        subjectMeta:
          testTableWithDuplicateFilterLabelsAndMissingData.subjectMeta,
        results: testTableWithDuplicateFilterLabelsAndMissingData.results,
      }).tableJson;

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            rowSpan: 2,
            colSpan: 1,
            scope: 'rowgroup',
            text: 'Indicator 1',
            tag: 'th',
          },
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: 'Filter 1',
            tag: 'th',
          },

          {
            tag: 'td',
            text: '85',
          },
        ],
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: 'Filter 2',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '90',
          },
        ],
      ]);
    });
  });

  describe('Handles merged column headers', () => {
    test('returns the correct JSON when column headers contain a single row of merged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOnlyMergedCellsInColumnHeadersConfig,
        subjectMeta: testTableWithOnlyMergedCellsInHeaders.subjectMeta,
        results: testTableWithOnlyMergedCellsInHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 1, rowSpan: 1, tag: 'td' },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 1 Group 1',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 1 Group 2',
            tag: 'th',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: 'Indicator 1',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '85',
          },
          {
            tag: 'td',
            text: '74',
          },
        ],
      ]);
    });

    test('returns the correct JSON when column headers contain a single row of merged and unmerged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithMergedAndUnMergedCellsInColumnHeadersConfig,
        subjectMeta: testTableWithMergedAndUnMergedCellsInHeaders.subjectMeta,
        results: testTableWithMergedAndUnMergedCellsInHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 1, rowSpan: 2, tag: 'td' },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            text: 'Category 1 Group 1',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Category 1 Group 2',
            tag: 'th',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 1 Group 2 Filter 1',
            tag: 'th',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: 'Indicator 1',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '95',
          },
          {
            tag: 'td',
            text: '85',
          },
        ],
      ]);
    });

    test('returns the correct JSON when the first level of column headers contains only merged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOnlyMergedCellsInFirstLevelOfColumnHeadersConfig,
        subjectMeta:
          testTableWithOnlyMergedCellsInFirstLevelOfHeaders.subjectMeta,
        results: testTableWithOnlyMergedCellsInFirstLevelOfHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 1, rowSpan: 2, tag: 'td' },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'Category 1 Group 1',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'Category 1 Group 2',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: '2012/13',
          },
          { tag: 'td', text: '74' },
          { tag: 'td', text: '85' },
          { tag: 'td', text: '92' },
          { tag: 'td', text: '87' },
        ],
      ]);
    });

    test('returns the correct JSON when the first level of column headers contains merged and unmerged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithMergedAndUnmergedCellsInFirstLevelOfColumnHeadersConfig,
        subjectMeta:
          testTableWithMergedAndUnmergedCellsInFirstLevelOfHeaders.subjectMeta,
        results:
          testTableWithMergedAndUnmergedCellsInFirstLevelOfHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 1, rowSpan: 3, tag: 'td' },
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'colgroup',
            tag: 'th',
            text: 'Category 1 Group 1',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'Category 1 Group 2',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'Category 1 Group 2 Filter 1',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: '2012/13',
          },
          { tag: 'td', text: '74' },
          { tag: 'td', text: '85' },
          { tag: 'td', text: '92' },
          { tag: 'td', text: '87' },
        ],
      ]);
    });

    test('returns the correct JSON when a middle level of column headers contains only merged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOnlyMergedCellsInMiddleLevelOfColumnHeadersConfig,
        subjectMeta:
          testTableWithOnlyMergedCellsInMiddleLevelOfHeaders.subjectMeta,
        results: testTableWithOnlyMergedCellsInMiddleLevelOfHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 1, rowSpan: 3, tag: 'td' },
          {
            colSpan: 4,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Indicator 1',
            tag: 'th',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Category 1 Group 1',
            tag: 'th',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Category 1 Group 2',
            tag: 'th',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 2 Group 1 Filter 1',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 2 Group 1 Filter 2',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 2 Group 1 Filter 1',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 2 Group 1 Filter 2',
            tag: 'th',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: '2012/13',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '75',
          },
          {
            tag: 'td',
            text: '70',
          },
          {
            tag: 'td',
            text: '73',
          },
          {
            tag: 'td',
            text: '66',
          },
        ],
      ]);
    });

    test('returns the correct JSON when a middle level of column headers contains merged and unmerged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithMergedAndUnmergedCellsInMiddleLevelOfColumnHeadersConfig,
        subjectMeta:
          testTableWithMergedAndUnmergedCellsInMiddleLevelOfHeaders.subjectMeta,
        results:
          testTableWithMergedAndUnmergedCellsInMiddleLevelOfHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 1, rowSpan: 4, tag: 'td' },
          {
            colSpan: 6,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Indicator 1',
            tag: 'th',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'colgroup',
            text: 'Category 1 Group 1',
            tag: 'th',
          },
          {
            colSpan: 4,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Category 1 Group 2',
            tag: 'th',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Category 1 Group 2 Filter 1',
            tag: 'th',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Category 1 Group 2',
            tag: 'th',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 2 Group 1 Filter 1',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 2 Group 1 Filter 2',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 2 Group 1 Filter 1',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 2 Group 1 Filter 2',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 2 Group 1 Filter 1',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 2 Group 1 Filter 2',
            tag: 'th',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: '2012/13',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '88',
          },
          {
            tag: 'td',
            text: '85',
          },
          {
            tag: 'td',
            text: '87',
          },
          {
            tag: 'td',
            text: '85',
          },
          {
            tag: 'td',
            text: '79',
          },
          {
            tag: 'td',
            text: '74',
          },
        ],
      ]);
    });

    test('returns the correct JSON when the last level of column headers contains only merged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOnlyMergedCellsInLastLevelOfColumnHeadersConfig,
        subjectMeta:
          testTableWithOnlyMergedCellsInLastLevelOfHeaders.subjectMeta,
        results: testTableWithOnlyMergedCellsInLastLevelOfHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 1, rowSpan: 3, tag: 'td' },
          {
            colSpan: 4,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Indicator 1',
            tag: 'th',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Category 2 Group 1 Filter 1',
            tag: 'th',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Category 2 Group 1 Filter 2',
            tag: 'th',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 1 Group 1',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 1 Group 2',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 1 Group 1',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 1 Group 2',
            tag: 'th',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: '2012/13',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '74',
          },
          {
            tag: 'td',
            text: '92',
          },
          {
            tag: 'td',
            text: '85',
          },
          {
            tag: 'td',
            text: '87',
          },
        ],
      ]);
    });

    test('returns the correct JSON when the last level of column headers contains merged and unmerged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithMergedAndUnmergedCellsInLastLevelOfColumnHeadersConfig,
        subjectMeta:
          testTableWithMergedAndUnmergedCellsInLastLevelOfHeaders.subjectMeta,
        results:
          testTableWithMergedAndUnmergedCellsInLastLevelOfHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 1, rowSpan: 4, tag: 'td' },
          {
            colSpan: 4,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Indicator 1',
            tag: 'th',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Category 2 Group 1 Filter 1',
            tag: 'th',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Category 2 Group 1 Filter 2',
            tag: 'th',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            text: 'Category 1 Group 1',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Category 1 Group 2',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            text: 'Category 1 Group 1',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            text: 'Category 1 Group 2',
            tag: 'th',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 1 Group 2 Filter 1',
            tag: 'th',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: 'Category 1 Group 2 Filter 1',
            tag: 'th',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: '2012/13',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '74',
          },
          {
            tag: 'td',
            text: '92',
          },
          {
            tag: 'td',
            text: '85',
          },
          {
            tag: 'td',
            text: '87',
          },
        ],
      ]);
    });
  });

  describe('Handles merged row headers', () => {
    test('returns the correct JSON when row headers contain a single row of merged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOnlyMergedCellsInRowHeadersConfig,
        subjectMeta: testTableWithOnlyMergedCellsInHeaders.subjectMeta,
        results: testTableWithOnlyMergedCellsInHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 1, rowSpan: 1, tag: 'td' },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: '2012/13',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 1 Group 1',
          },
          { tag: 'td', text: '85' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 1 Group 2',
          },
          { tag: 'td', text: '74' },
        ],
      ]);
    });

    test('returns the correct JSON when row headers contain a single row of merged and unmerged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithMergedAndUnmergedCellsInRowHeadersConfig,
        subjectMeta: testTableWithMergedAndUnMergedCellsInHeaders.subjectMeta,
        results: testTableWithMergedAndUnMergedCellsInHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 2, rowSpan: 1, tag: 'td' },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: '2012/13',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 1 Group 1',
          },
          { tag: 'td', text: '95' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 2',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 1 Group 2 Filter 1',
          },
          { tag: 'td', text: '85' },
        ],
      ]);
    });

    test('returns the correct JSON when the first level of row headers contains only merged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOnlyMergedCellsInFirstLevelOfRowHeadersConfig,
        subjectMeta:
          testTableWithOnlyMergedCellsInFirstLevelOfHeaders.subjectMeta,
        results: testTableWithOnlyMergedCellsInFirstLevelOfHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 2, rowSpan: 1, tag: 'td' },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: '2012/13',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          { tag: 'td', text: '74' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
          { tag: 'td', text: '85' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 2',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          { tag: 'td', text: '92' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
          { tag: 'td', text: '87' },
        ],
      ]);
    });

    test('returns the correct JSON when the first level of row headers contains merged and unmerged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithMergedAndUnmergedCellsInFirstLevelOfRowHeadersConfig,
        subjectMeta:
          testTableWithMergedAndUnmergedCellsInFirstLevelOfHeaders.subjectMeta,
        results:
          testTableWithMergedAndUnmergedCellsInFirstLevelOfHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 3, rowSpan: 1, tag: 'td' },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: '2012/13',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          { tag: 'td', text: '74' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
          { tag: 'td', text: '85' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 2',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 2 Filter 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          { tag: 'td', text: '92' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
          { tag: 'td', text: '87' },
        ],
      ]);
    });

    test('returns the correct JSON when a middle level of row headers contains only merged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOnlyMergedCellsInMiddleLevelOfRowsConfig,
        subjectMeta:
          testTableWithOnlyMergedCellsInMiddleLevelOfHeaders.subjectMeta,
        results: testTableWithOnlyMergedCellsInMiddleLevelOfHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 3, rowSpan: 1, tag: 'td' },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: '2012/13',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            colSpan: 1,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Indicator 1',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          { tag: 'td', text: '75' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
          { tag: 'td', text: '70' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 2',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          { tag: 'td', text: '73' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
          { tag: 'td', text: '66' },
        ],
      ]);
    });

    test('returns the correct JSON when a middle level of row headers contains merged and unmerged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithMergedAndUnmergedCellsInMiddleLevelOfRowHeadersConfig,
        subjectMeta:
          testTableWithMergedAndUnmergedCellsInMiddleLevelOfHeaders.subjectMeta,
        results:
          testTableWithMergedAndUnmergedCellsInMiddleLevelOfHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 4, rowSpan: 1, tag: 'td' },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: '2012/13',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            colSpan: 1,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Indicator 1',
          },
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          { tag: 'td', text: '88' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
          { tag: 'td', text: '85' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 2',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 2 Filter 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          { tag: 'td', text: '87' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
          { tag: 'td', text: '85' },
        ],
      ]);
    });

    test('returns the correct JSON when the last level of row headers contains only merged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOnlyMergedCellsInLastLevelOfRowHeadersConfig,
        subjectMeta:
          testTableWithOnlyMergedCellsInLastLevelOfHeaders.subjectMeta,
        results: testTableWithOnlyMergedCellsInLastLevelOfHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 3, rowSpan: 1, tag: 'td' },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: '2012/13',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            colSpan: 1,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Indicator 1',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 1 Group 1',
          },
          { tag: 'td', text: '74' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 1 Group 2',
          },
          { tag: 'td', text: '92' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 1 Group 1',
          },
          { tag: 'td', text: '85' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 1 Group 2',
          },
          { tag: 'td', text: '87' },
        ],
      ]);
    });

    test('returns the correct JSON when the last level of row headers contains merged and unmerged cells', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithMergedAndUnmergedCellsInLastLevelOfRowHeadersConfig,
        subjectMeta:
          testTableWithMergedAndUnmergedCellsInLastLevelOfHeaders.subjectMeta,
        results:
          testTableWithMergedAndUnmergedCellsInLastLevelOfHeaders.results,
      }).tableJson;

      expect(result.thead).toEqual<TableCellJson[][]>([
        [
          { colSpan: 4, rowSpan: 1, tag: 'td' },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: '2012/13',
          },
        ],
      ]);

      expect(result.tbody).toEqual<TableCellJson[][]>([
        [
          {
            colSpan: 1,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Indicator 1',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 1',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 1 Group 1',
          },
          { tag: 'td', text: '74' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 2',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 1 Group 2 Filter 1',
          },
          { tag: 'td', text: '92' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 2 Group 1 Filter 2',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 1 Group 1',
          },
          { tag: 'td', text: '85' },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Category 1 Group 2',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Category 1 Group 2 Filter 1',
          },
          { tag: 'td', text: '87' },
        ],
      ]);
    });
  });

  describe('hasMissingRowsOrColumns', () => {
    const testQuery: ReleaseTableDataQuery = {
      subjectId: 'subject-1-id',
      timePeriod: {
        startYear: 2012,
        startCode: 'AY',
        endYear: 2013,
        endCode: 'AY',
      },
      filters: [
        'category-1-filter-2',
        'category-2-filter-1',
        'category-2-filter-2',
      ],
      indicators: ['indicator-1', 'indicator-2'],
      locationIds: ['la-1', 'la-2'],
    };

    test('is false when no rows or columns are excluded', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOneLevelOfRowAndColHeadersConfig,
        subjectMeta: testTableWithOneLevelOfRowAndColHeaders.subjectMeta,
        results: testTableWithOneLevelOfRowAndColHeaders.results,
        query: testQuery,
      });

      expect(result.hasMissingRowsOrColumns).toBe(false);
    });

    test('is true when a row or column for an indicator is missing because it has no data', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOneLevelOfRowAndColHeadersConfig,
        subjectMeta: testTableWithOneLevelOfRowAndColHeaders.subjectMeta,
        results: testTableWithOneLevelOfRowAndColHeaders.results,
        query: {
          ...testQuery,
          indicators: ['indicator-1', 'indicator-2', 'missing-indicator'],
        },
      });

      expect(result.hasMissingRowsOrColumns).toBe(true);
    });

    test('is true when a row or column for a filter is missing because it has no data', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOneLevelOfRowAndColHeadersConfig,
        subjectMeta: testTableWithOneLevelOfRowAndColHeaders.subjectMeta,
        results: testTableWithOneLevelOfRowAndColHeaders.results,
        query: {
          ...testQuery,
          filters: [
            'category-1-filter-2',
            'category-2-filter-1',
            'missing-filter',
            'category-2-filter-2',
          ],
        },
      });

      expect(result.hasMissingRowsOrColumns).toBe(true);
    });

    test('is true when a row or column for a location is missing because it has no data', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOneLevelOfRowAndColHeadersConfig,
        subjectMeta: testTableWithOneLevelOfRowAndColHeaders.subjectMeta,
        results: testTableWithOneLevelOfRowAndColHeaders.results,
        query: {
          ...testQuery,
          locationIds: ['missing-location', 'la-1', 'la-2'],
        },
      });

      expect(result.hasMissingRowsOrColumns).toBe(true);
    });

    test('is true when a row or column for a time period is excluded because it has no data', () => {
      const result = mapTableToJson({
        tableHeadersConfig: testTableWithOneLevelOfRowAndColHeadersConfig,
        subjectMeta: testTableWithMissingTimePeriod.subjectMeta,
        results: testTableWithMissingTimePeriod.results,
        query: testQuery,
      });

      expect(result.hasMissingRowsOrColumns).toBe(true);
    });
  });
});
