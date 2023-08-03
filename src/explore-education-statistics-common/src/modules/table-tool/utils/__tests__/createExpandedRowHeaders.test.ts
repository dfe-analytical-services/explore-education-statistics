import Header from '../Header';
import createExpandedRowHeaders from '../createExpandedRowHeaders';
import { TableCellJson } from '../mapTableToJson';

describe('createExpandedRowHeaders', () => {
  test('should return a single row of row headers if no groups are provided', () => {
    const rowHeaders: Header[] = [
      new Header('1', '1'),
      new Header('2', '2'),
      new Header('3', '3'),
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual<TableCellJson[][]>([
      [
        {
          colSpan: 1,
          tag: 'th',
          scope: 'row',
          rowSpan: 1,
          text: '1',
        },
      ],
      [
        {
          colSpan: 1,
          tag: 'th',
          scope: 'row',
          rowSpan: 1,
          text: '2',
        },
      ],
      [
        {
          colSpan: 1,
          tag: 'th',
          scope: 'row',
          rowSpan: 1,
          text: '3',
        },
      ],
    ]);
  });

  test('should return multiple rows of row headers if groups are provided', () => {
    const rowHeaders: Header[] = [
      new Header('1', '1').addChild(new Header('1.1', '1.1')),
      new Header('2', '2').addChild(new Header('2.1', '2.1')),
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual<TableCellJson[][]>([
      [
        {
          colSpan: 1,
          tag: 'th',
          scope: 'rowgroup',
          rowSpan: 1,
          text: '1',
        },
        {
          colSpan: 1,
          tag: 'th',
          scope: 'row',
          rowSpan: 1,
          text: '1.1',
        },
      ],
      [
        {
          colSpan: 1,
          tag: 'th',
          scope: 'rowgroup',
          rowSpan: 1,
          text: '2',
        },
        {
          colSpan: 1,
          tag: 'th',
          scope: 'row',
          rowSpan: 1,
          text: '2.1',
        },
      ],
    ]);
  });

  test('returns correct headers with 2 levels of 2 row headers', () => {
    const rowHeaders: Header[] = [
      new Header('1', '1')
        .addChild(new Header('3', '3'))
        .addChild(new Header('4', '4')),
      new Header('2', '2')
        .addChild(new Header('3', '3'))
        .addChild(new Header('4', '4')),
    ];
    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: '1',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: '3',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: '4',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: '2',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: '3',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: '4',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with 3 levels of 2 row headers', () => {
    const rowHeaders: Header[] = [
      new Header('1', '1')
        .addChild(
          new Header('3', '3')
            .addChild(new Header('5', '5'))
            .addChild(new Header('6', '6')),
        )
        .addChild(
          new Header('4', '4')
            .addChild(new Header('5', '5'))
            .addChild(new Header('6', '6')),
        ),
      new Header('2', '2')
        .addChild(
          new Header('3', '3')
            .addChild(new Header('5', '5'))
            .addChild(new Header('6', '6')),
        )
        .addChild(
          new Header('4', '4')
            .addChild(new Header('5', '5'))
            .addChild(new Header('6', '6')),
        ),
    ];
    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: '1',
          rowSpan: 4,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: '3',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: '5',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: '6',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: '4',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: '5',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: '6',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: '2',
          rowSpan: 4,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: '3',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: '5',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: '6',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: '4',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: '5',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: '6',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with one `rowgroup` header subgroup', () => {
    const rowHeaders: Header[] = [
      new Header('B', 'B').addChild(
        new Header('A', 'A')
          .addChild(new Header('C', 'C'))
          .addChild(new Header('D', 'D')),
      ),
    ];
    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'A',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with two `rowgroup` header subgroups', () => {
    const rowHeaders: Header[] = [
      new Header('B', 'B').addChild(
        new Header('A', 'A').addChild(new Header('F', 'F')),
      ),
      new Header('C', 'C')
        .addChild(new Header('D', 'D').addChild(new Header('F', 'F')))
        .addChild(new Header('E', 'E').addChild(new Header('F', 'F'))),
    ];
    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'A',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'E',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with three `rowgroup` header subgroups', () => {
    const rowHeaders: Header[] = [
      new Header('B', 'B').addChild(
        new Header('A', 'A').addChild(new Header('H', 'H')),
      ),
      new Header('C', 'C')
        .addChild(new Header('D', 'D').addChild(new Header('H', 'H')))
        .addChild(new Header('F', 'F').addChild(new Header('H', 'H'))),
      new Header('E', 'E').addChild(
        new Header('G', 'G').addChild(new Header('H', 'H')),
      ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'A',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'E',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'G',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with `rowgroup` header merged with identical subgroup', () => {
    const rowHeaders: Header[] = [
      new Header('B', 'B').addChild(
        new Header('A', 'A').addChild(new Header('F', 'F')),
      ),
      new Header('C', 'C').addChild(
        new Header('C', 'C').addChild(new Header('F', 'F')),
      ),
      new Header('D', 'D').addChild(
        new Header('E', 'E').addChild(new Header('F', 'F')),
      ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'A',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 2,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with multi-rowSpan `rowgroup` merged with its identical groups', () => {
    const rowHeaders: Header[] = [
      new Header('B', 'B').addChild(
        new Header('A', 'A').addChild(new Header('F', 'F')),
      ),
      new Header('C', 'C').addChild(
        new Header('C', 'C')
          .addChild(new Header('F', 'F'))
          .addChild(new Header('F', 'F')),
      ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'A',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 2,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 2,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with multi-rowSpan `rowgroup` header merged with 2 identical groups ', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('B', 'B').addChild(new Header('C', 'C')),
      ),
      new Header('D', 'D').addChild(
        new Header('D', 'D')
          .addChild(new Header('D', 'D'))
          .addChild(new Header('E', 'E')),
      ),
      new Header('F', 'F').addChild(
        new Header('G', 'G').addChild(new Header('H', 'H')),
      ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'D',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 2,
        },
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'E',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'G',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('does not return `rowgroup` headers with multi-rowSpan subgroup with invalid rowrowSpans and colrowSpans', () => {
    const rowHeaders: Header[] = [
      new Header('B', 'B').addChild(
        new Header('A', 'A').addChild(new Header('E', 'E')),
      ),
      new Header('C', 'C')
        .addChild(
          new Header('C', 'C')
            .addChild(new Header('E', 'E'))
            .addChild(new Header('E', 'E')),
        )
        .addChild(new Header('D', 'D').addChild(new Header('E', 'E'))),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'A',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          rowSpan: 3,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          rowSpan: 2,
          scope: 'row',
          colSpan: 1,
        },
      ],
      undefined,
      [
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ] as TableCellJson[][];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with one `row` header subgroup', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('B', 'B')
          .addChild(new Header('C', 'C'))
          .addChild(new Header('D', 'D')),
      ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'B',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with two `row` header subgroups', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A')
        .addChild(new Header('B', 'B').addChild(new Header('D', 'D')))
        .addChild(
          new Header('C', 'C')
            .addChild(new Header('E', 'E'))
            .addChild(new Header('F', 'F')),
        ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          rowSpan: 3,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with three `row` header subgroups', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A')
        .addChild(new Header('B', 'B').addChild(new Header('E', 'E')))
        .addChild(
          new Header('C', 'C')
            .addChild(new Header('F', 'F'))
            .addChild(new Header('G', 'G')),
        )
        .addChild(new Header('D', 'D').addChild(new Header('H', 'H'))),
    ];
    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          rowSpan: 4,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'G',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];
    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with `rowgroup` header merged with identical parent', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('B', 'B').addChild(new Header('C', 'C')),
      ),
      new Header('D', 'D').addChild(
        new Header('D', 'D').addChild(new Header('E', 'E')),
      ),
      new Header('F', 'F').addChild(
        new Header('G', 'G').addChild(new Header('H', 'H')),
      ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 2,
        },
        {
          tag: 'th',
          text: 'E',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'G',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];
    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with `rowgroup` header merged with multiple identical parents', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('B', 'B').addChild(new Header('C', 'C')),
      ),
      new Header('D', 'D').addChild(
        new Header('D', 'D').addChild(new Header('D', 'D')),
      ),
      new Header('F', 'F').addChild(
        new Header('G', 'G').addChild(new Header('H', 'H')),
      ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'row',
          colSpan: 3,
        },
      ],
      [
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'G',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with `rowgroup` header merged with identical parent on first row', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('A', 'A').addChild(new Header('B', 'B')),
      ),
      new Header('C', 'C').addChild(
        new Header('D', 'D').addChild(new Header('E', 'E')),
      ),
      new Header('F', 'F').addChild(
        new Header('G', 'G').addChild(new Header('H', 'H')),
      ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 2,
        },
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'G',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with `row` header merged with identical parent', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A')
        .addChild(new Header('B', 'B').addChild(new Header('E', 'E')))
        .addChild(new Header('C', 'C').addChild(new Header('C', 'C')))
        .addChild(new Header('D', 'D').addChild(new Header('F', 'F'))),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          rowSpan: 3,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          rowSpan: 1,
          scope: 'row',
          colSpan: 2,
        },
      ],
      [
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with `row` header merged with identical parent on first row', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A')
        .addChild(new Header('B', 'B').addChild(new Header('B', 'B')))
        .addChild(new Header('C', 'C').addChild(new Header('D', 'D')))
        .addChild(new Header('E', 'E').addChild(new Header('F', 'F'))),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          rowSpan: 3,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'row',
          colSpan: 2,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'E',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with deeply nested rows and multiple identical headers', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('A', 'A').addChild(
          new Header('A', 'A').addChild(
            new Header('A', 'A').addChild(new Header('A', 'A')),
          ),
        ),
      ),
      new Header('B', 'B').addChild(
        new Header('B', 'B')
          .addChild(
            new Header('B', 'B').addChild(
              new Header('B', 'B').addChild(new Header('B', 'B')),
            ),
          )
          .addChild(
            new Header('C', 'C').addChild(
              new Header('D', 'D').addChild(new Header('D1', 'D1')),
            ),
          ),
      ),
      new Header('E', 'E').addChild(
        new Header('F', 'F')
          .addChild(
            new Header('F', 'F')
              .addChild(new Header('F', 'F').addChild(new Header('F', 'F')))
              .addChild(new Header('G', 'G').addChild(new Header('G1', 'G1'))),
          )
          .addChild(
            new Header('H', 'H')
              .addChild(new Header('I', 'I').addChild(new Header('I', 'I')))
              .addChild(new Header('J', 'J').addChild(new Header('J', 'J'))),
          ),
      ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          rowSpan: 1,
          scope: 'row',
          colSpan: 5,
        },
      ],
      [
        {
          tag: 'th',
          text: 'B',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 2,
        },
        {
          tag: 'th',
          text: 'B',
          rowSpan: 1,
          scope: 'row',
          colSpan: 3,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'D1',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'E',
          rowSpan: 4,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 4,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          rowSpan: 1,
          scope: 'row',
          colSpan: 2,
        },
      ],
      [
        {
          tag: 'th',
          text: 'G',
          rowSpan: 1,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'G1',
          rowSpan: 1,
          scope: 'row',
          colSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'H',
          rowSpan: 2,
          scope: 'rowgroup',
          colSpan: 1,
        },
        {
          tag: 'th',
          text: 'I',
          rowSpan: 1,
          scope: 'row',
          colSpan: 2,
        },
      ],
      [
        {
          tag: 'th',
          text: 'J',
          rowSpan: 1,
          scope: 'row',
          colSpan: 2,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with only headers merged with identical parent in the first row', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('A', 'A')
          .addChild(new Header('C', 'C'))
          .addChild(new Header('D', 'D')),
      ),
      new Header('B', 'B').addChild(
        new Header('B', 'B')
          .addChild(new Header('C', 'C'))
          .addChild(new Header('D', 'D')),
      ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'rowgroup',
          tag: 'th',
          text: 'A',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'C',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'D',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'rowgroup',
          tag: 'th',
          text: 'B',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'C',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'D',
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with only headers merged with identical parent in the middle row', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A')
        .addChild(
          new Header('B', 'B').addChild(
            new Header('B', 'B')
              .addChild(new Header('D', 'D'))
              .addChild(new Header('E', 'E')),
          ),
        )
        .addChild(
          new Header('C', 'C').addChild(
            new Header('C', 'C')
              .addChild(new Header('D', 'D'))
              .addChild(new Header('E', 'E')),
          ),
        ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          colSpan: 1,
          rowSpan: 4,
          scope: 'rowgroup',
          tag: 'th',
          text: 'A',
        },
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'rowgroup',
          tag: 'th',
          text: 'B',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'D',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'E',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'rowgroup',
          tag: 'th',
          text: 'C',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'D',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'E',
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with only headers merged with identical parent in the last row', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A')
        .addChild(
          new Header('B', 'B')
            .addChild(new Header('D', 'D').addChild(new Header('D', 'D')))
            .addChild(new Header('E', 'E').addChild(new Header('E', 'E'))),
        )
        .addChild(
          new Header('C', 'C')
            .addChild(new Header('D', 'D').addChild(new Header('D', 'D')))
            .addChild(new Header('E', 'E').addChild(new Header('E', 'E'))),
        ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          colSpan: 1,
          rowSpan: 4,
          scope: 'rowgroup',
          tag: 'th',
          text: 'A',
        },
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'rowgroup',
          tag: 'th',
          text: 'B',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'D',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'E',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'rowgroup',
          tag: 'th',
          text: 'C',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'D',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'E',
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns the correct headers when there are multiple groups with the same labels', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('C', 'C')
          .addChild(new Header('A', 'A').addChild(new Header('D', 'D')))
          .addChild(new Header('B', 'B').addChild(new Header('E', 'E'))),
      ),
      new Header('B', 'B').addChild(
        new Header('F', 'F')
          .addChild(new Header('A', 'A').addChild(new Header('D', 'D')))
          .addChild(new Header('B', 'B').addChild(new Header('E', 'E'))),
      ),
    ];

    const expandedRowHeaders: TableCellJson[][] = [
      [
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'rowgroup',
          tag: 'th',
          text: 'A',
        },
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'rowgroup',
          tag: 'th',
          text: 'C',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'rowgroup',
          tag: 'th',
          text: 'A',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'D',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'rowgroup',
          tag: 'th',
          text: 'B',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'E',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'rowgroup',
          tag: 'th',
          text: 'B',
        },
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'rowgroup',
          tag: 'th',
          text: 'F',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'rowgroup',
          tag: 'th',
          text: 'A',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'D',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'rowgroup',
          tag: 'th',
          text: 'B',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'row',
          tag: 'th',
          text: 'E',
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  describe('a mix of rows with merged headers without siblings and merged headers with siblings', () => {
    test('returns correct headers for scenario 1', () => {
      const rowHeaders: Header[] = [
        new Header('A', 'A')
          .addChild(
            new Header('C', 'C')
              .addChild(new Header('D', 'D'))
              .addChild(new Header('Total', 'Total')),
          )
          .addChild(
            new Header('Total', 'Total').addChild(new Header('Total', 'Total')),
          ),
        new Header('B', 'B').addChild(
          new Header('Total', 'Total').addChild(new Header('Total', 'Total')),
        ),
      ];

      const expandedRowHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 1,
            rowSpan: 3,
            scope: 'rowgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'D',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Total',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Total',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'B',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Total',
          },
        ],
      ];

      expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
    });

    test('returns correct headers for scenario 2', () => {
      const rowHeaders: Header[] = [
        new Header('A', 'A').addChild(
          new Header('Total', 'Total').addChild(
            new Header('Total', 'Total')
              .addChild(new Header('C', 'C'))
              .addChild(new Header('D', 'D')),
          ),
        ),
        new Header('B', 'B')
          .addChild(
            new Header('Total', 'Total').addChild(
              new Header('Total', 'Total')
                .addChild(new Header('C', 'C'))
                .addChild(new Header('D', 'D')),
            ),
          )
          .addChild(
            new Header('E', 'E').addChild(
              new Header('F', 'F')
                .addChild(new Header('C', 'C'))
                .addChild(new Header('D', 'D')),
            ),
          ),
      ];

      const expandedRowHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'C',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'D',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'B',
          },
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'C',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'D',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'E',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'F',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'C',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'D',
          },
        ],
      ];

      expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
    });

    test('returns correct headers for scenario 3', () => {
      const rowHeaders: Header[] = [
        new Header('A', 'A').addChild(
          new Header('Total', 'Total').addChild(
            new Header('Total', 'Total').addChild(new Header('Total', 'Total')),
          ),
        ),
        new Header('B', 'B')
          .addChild(
            new Header('Total', 'Total').addChild(
              new Header('Total', 'Total')
                .addChild(new Header('Total', 'Total'))
                .addChild(new Header('D', 'D')),
            ),
          )
          .addChild(
            new Header('E', 'E').addChild(
              new Header('F', 'F')
                .addChild(new Header('Total', 'Total'))
                .addChild(new Header('D', 'D')),
            ),
          ),
      ];

      const expandedRowHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 3,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Total',
          },
        ],

        [
          {
            colSpan: 1,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'B',
          },
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Total',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'D',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'E',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'F',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Total',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'D',
          },
        ],
      ];

      expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
    });

    test('returns correct headers for scenario 4', () => {
      const rowHeaders: Header[] = [
        new Header('E', 'E').addChild(
          new Header('Total', 'Total').addChild(new Header('Total', 'Total')),
        ),
        new Header('A', 'A')
          .addChild(
            new Header('C', 'C')
              .addChild(new Header('D', 'D'))
              .addChild(new Header('Total', 'Total')),
          )
          .addChild(
            new Header('Total', 'Total').addChild(new Header('Total', 'Total')),
          ),
        new Header('B', 'B').addChild(
          new Header('Total', 'Total').addChild(new Header('Total', 'Total')),
        ),
      ];

      const expandedRowHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'E',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Total',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 3,
            scope: 'rowgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'D',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Total',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Total',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'B',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Total',
          },
        ],
      ];

      expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
    });

    test('returns correct headers for scenario 5', () => {
      const rowHeaders: Header[] = [
        new Header('A', 'A').addChild(
          new Header('Total', 'Total').addChild(
            new Header('Total', 'Total')
              .addChild(new Header('C', 'C').addChild(new Header('C', 'C')))
              .addChild(
                new Header('D', 'D')
                  .addChild(new Header('G', 'G'))
                  .addChild(new Header('C', 'C')),
              ),
          ),
        ),
        new Header('B', 'B')
          .addChild(
            new Header('Total', 'Total').addChild(
              new Header('Total', 'Total')
                .addChild(
                  new Header('C', 'C')
                    .addChild(new Header('G', 'G'))
                    .addChild(new Header('C', 'C')),
                )
                .addChild(
                  new Header('D', 'D')
                    .addChild(new Header('G', 'G'))
                    .addChild(new Header('C', 'C')),
                ),
            ),
          )
          .addChild(
            new Header('E', 'E').addChild(
              new Header('F', 'F').addChild(
                new Header('C', 'C').addChild(new Header('C', 'C')),
              ),
            ),
          ),
      ];

      const expandedRowHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 1,
            rowSpan: 3,
            scope: 'rowgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 2,
            rowSpan: 3,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'C',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'D',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'G',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'C',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 5,
            scope: 'rowgroup',
            tag: 'th',
            text: 'B',
          },
          {
            colSpan: 2,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'G',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'C',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'D',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'G',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'C',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'E',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'F',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'C',
          },
        ],
      ];

      expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
    });

    test('returns correct headers for scenario 6', () => {
      const rowHeaders: Header[] = [
        new Header('A', 'A')
          .addChild(new Header('C', 'C').addChild(new Header('C', 'C')))
          .addChild(new Header('D', 'D').addChild(new Header('D', 'D'))),
        new Header('B', 'B')
          .addChild(
            new Header('E', 'E')
              .addChild(new Header('F', 'F'))
              .addChild(new Header('G', 'G')),
          )
          .addChild(
            new Header('H', 'H')
              .addChild(new Header('I', 'I'))
              .addChild(new Header('J', 'J')),
          ),
      ];

      const expandedRowHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'C',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'D',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'B',
          },

          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'E',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'F',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'G',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'H',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'I',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'J',
          },
        ],
      ];

      expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
    });

    test('returns correct headers for scenario 7', () => {
      const rowHeaders: Header[] = [
        new Header('A', 'A').addChild(
          new Header('C', 'C')
            .addChild(new Header('F', 'F').addChild(new Header('G', 'G')))
            .addChild(new Header('H', 'H').addChild(new Header('H', 'H')))
            .addChild(new Header('F', 'F').addChild(new Header('F', 'F'))),
        ),

        new Header('B', 'B').addChild(
          new Header('E', 'E')
            .addChild(new Header('H', 'H').addChild(new Header('H', 'H')))
            .addChild(new Header('F', 'F').addChild(new Header('F', 'F'))),
        ),
      ];

      const expandedRowHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 1,
            rowSpan: 3,
            scope: 'rowgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 1,
            rowSpan: 3,
            scope: 'rowgroup',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'rowgroup',
            tag: 'th',
            text: 'F',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'G',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'H',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'F',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'B',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'E',
          },

          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'H',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'F',
          },
        ],
      ];

      expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
    });

    test('returns correct headers for scenario 8', () => {
      const rowHeaders: Header[] = [
        new Header('A', 'A').addChild(
          new Header('C', 'C')
            .addChild(
              new Header('F', 'F').addChild(
                new Header('G', 'G')
                  .addChild(new Header('X', 'X'))
                  .addChild(new Header('Y', 'Y')),
              ),
            )
            .addChild(
              new Header('H', 'H').addChild(
                new Header('H', 'H')
                  .addChild(new Header('X', 'X'))
                  .addChild(new Header('Y', 'Y')),
              ),
            ),
        ),
        new Header('B', 'B').addChild(
          new Header('E', 'E')
            .addChild(
              new Header('F', 'F').addChild(
                new Header('F', 'F')
                  .addChild(new Header('X', 'X'))
                  .addChild(new Header('Y', 'Y')),
              ),
            )
            .addChild(
              new Header('H', 'H').addChild(
                new Header('H', 'H')
                  .addChild(new Header('X', 'X'))
                  .addChild(new Header('Y', 'Y')),
              ),
            ),
        ),
      ];

      const expandedRowHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 1,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 1,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'F',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'G',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'X',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Y',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'H',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'X',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Y',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'B',
          },
          {
            colSpan: 1,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'E',
          },

          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'F',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'X',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Y',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'H',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'X',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'Y',
          },
        ],
      ];

      expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
    });

    test('returns correct headers for scenario 9', () => {
      const rowHeaders: Header[] = [
        new Header('A', 'A')
          .addChild(new Header('C', 'C').addChild(new Header('C', 'C')))
          .addChild(new Header('D', 'D').addChild(new Header('D', 'D')))
          .addChild(
            new Header('E', 'E')
              .addChild(new Header('F', 'F'))
              .addChild(new Header('G', 'G')),
          ),
        new Header('B', 'B')
          .addChild(new Header('C', 'C').addChild(new Header('C', 'C')))
          .addChild(new Header('E', 'E').addChild(new Header('E', 'E'))),
      ];

      const expandedRowHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 1,
            rowSpan: 4,
            scope: 'rowgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'C',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'D',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'E',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'F',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'G',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'rowgroup',
            tag: 'th',
            text: 'B',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'C',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'row',
            tag: 'th',
            text: 'E',
          },
        ],
      ];

      expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
    });
  });
});
