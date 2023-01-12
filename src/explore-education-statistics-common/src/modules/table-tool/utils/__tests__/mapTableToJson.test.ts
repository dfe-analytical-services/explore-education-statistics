import {
  testTableWithThreeLevelsOfRowAndColHeaders,
  testTableWithThreeLevelsOfRowAndColHeadersConfig,
  testTableWithTwoLevelsOfRowAndColHeaders,
  testTableWithTwoLevelsOfRowAndColHeadersConfig,
} from '@common/modules/table-tool/utils/__data__/testTableData';
import mapTableToJson, {
  TableCellJson,
} from '@common/modules/table-tool/utils/mapTableToJson';

describe('mapTableToJson', () => {
  test('returns the correct JSON for table with two levels of column headers and two levels of row headers', () => {
    const result = mapTableToJson(
      testTableWithTwoLevelsOfRowAndColHeadersConfig,
      testTableWithTwoLevelsOfRowAndColHeaders.subjectMeta,
      testTableWithTwoLevelsOfRowAndColHeaders.results,
    ).tableJson;

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

  test('returns the correct JSON for table with three levels of column headers and three levels of row headers', () => {
    const result = mapTableToJson(
      testTableWithThreeLevelsOfRowAndColHeadersConfig,
      testTableWithThreeLevelsOfRowAndColHeaders.subjectMeta,
      testTableWithThreeLevelsOfRowAndColHeaders.results,
    ).tableJson;

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
  });

  /* 
    TODO: - additional test cases to cover:
    * a table with one level of row and col headers
    * a table with two levels of row headers and one level of col headers
    * a table with two levels of col headers and one level of row headers
  */
});
