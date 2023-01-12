import mapRows from '../mapTableBody';
import { ExpandedHeader } from '../mapTableToJson';

describe('mapRows', () => {
  test('returns valid table cells for a single row', () => {
    const rows: string[][] = [['a', 'b', 'c']];
    const rowHeaders: ExpandedHeader[][] = [
      [
        {
          id: '1',
          text: '1',
          span: 1,
          crossSpan: 1,
          isGroup: false,
        },
      ],
    ];

    expect(mapRows(rows, rowHeaders)).toEqual([
      [
        { tag: 'th', scope: 'row', text: '1', rowSpan: 1, colSpan: 1 },
        { tag: 'td', text: 'a' },
        { tag: 'td', text: 'b' },
        { tag: 'td', text: 'c' },
      ],
    ]);
  });
});
