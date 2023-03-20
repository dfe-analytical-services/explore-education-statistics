import {
  testTableWithThreeLevelsOfRowAndColHeaders,
  testTableWithThreeLevelsOfRowAndColHeadersConfig,
  testTableWithTwoLevelsOfRowAndColHeaders,
  testTableWithTwoLevelsOfRowAndColHeadersConfig,
  testTableWithOneLevelOfRowAndColHeaders,
  testTableWithOneLevelOfRowAndColHeadersConfig,
  testTableWithTwoLevelsOfRowAndOneLevelOfColHeadersConfig,
  testTableWithOneLevelOfRowsAndTwoLevelsofColHeadersConfig,
  testTableWithMissingTimePeriod,
} from '@common/modules/table-tool/utils/__data__/testTableData';
import mapTableToJson, {
  TableCellJson,
} from '@common/modules/table-tool/utils/mapTableToJson';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';

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

  test('returns the correct JSON for  a table with two levels of col headers and one level of row headers', () => {
    const result = mapTableToJson({
      tableHeadersConfig: testTableWithOneLevelOfRowsAndTwoLevelsofColHeadersConfig,
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
