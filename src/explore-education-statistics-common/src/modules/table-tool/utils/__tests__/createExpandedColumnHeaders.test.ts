import Header from '@common/modules/table-tool/utils/Header';
import createExpandedColumnHeaders from '@common/modules/table-tool/utils/createExpandedColumnHeaders';
import { TableCellJson } from '@common/modules/table-tool/utils/mapTableToJson';

describe('createExpandedColumnHeaders', () => {
  test('should return a single row of column headers if no groups are provided', () => {
    const columnHeaders: Header[] = [
      new Header('1', '1'),
      new Header('2', '2'),
      new Header('3', '3'),
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual<
      TableCellJson[][]
    >([
      [
        { tag: 'th', text: '1', colSpan: 1, rowSpan: 1, scope: 'col' },
        { tag: 'th', text: '2', colSpan: 1, rowSpan: 1, scope: 'col' },
        { tag: 'th', text: '3', colSpan: 1, rowSpan: 1, scope: 'col' },
      ],
    ]);
  });

  test('should return multiple rows of column headers if groups are provided', () => {
    const columnHeaders: Header[] = [
      new Header('1', '1').addChild(new Header('1.1', '1.1')),
      new Header('2', '2').addChild(new Header('2.1', '2.1')),
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual<
      TableCellJson[][]
    >([
      [
        {
          rowSpan: 1,
          tag: 'th',
          scope: 'colgroup',
          colSpan: 1,
          text: '1',
        },
        {
          rowSpan: 1,
          tag: 'th',
          scope: 'colgroup',
          colSpan: 1,
          text: '2',
        },
      ],
      [
        {
          rowSpan: 1,
          tag: 'th',
          scope: 'col',
          colSpan: 1,
          text: '1.1',
        },
        {
          rowSpan: 1,
          tag: 'th',
          scope: 'col',
          colSpan: 1,
          text: '2.1',
        },
      ],
    ]);
  });

  test('returns the correct headers with one `colgroup` header subgroup', () => {
    const columnHeaders: Header[] = [
      new Header('B', 'B').addChild(
        new Header('A', 'A')
          .addChild(new Header('C', 'C'))
          .addChild(new Header('D', 'D')),
      ),
    ];

    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          colSpan: 2,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 2,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns the correct headers with two `colgroup` header subgroups', () => {
    const columnHeaders: Header[] = [
      new Header('B', 'B').addChild(
        new Header('A', 'A').addChild(new Header('F', 'F')),
      ),
      new Header('C', 'C')
        .addChild(new Header('D', 'D').addChild(new Header('F', 'F')))
        .addChild(new Header('E', 'E').addChild(new Header('F', 'F'))),
    ];
    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          colSpan: 2,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns the correct headers with three `colgroup` header subgroups', () => {
    const columnHeaders: Header[] = [
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

    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          colSpan: 2,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'G',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'H',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns the correct headers with `colgroup` header merged with identical parent', () => {
    const columnHeaders: Header[] = [
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
    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 2,
        },
        {
          tag: 'th',
          text: 'D',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns the correct headers with `colgroup` header merged with multiple identical parents', () => {
    const columnHeaders: Header[] = [
      new Header('B', 'B').addChild(
        new Header('A', 'A').addChild(new Header('F', 'F')),
      ),
      new Header('C', 'C').addChild(
        new Header('C', 'C').addChild(new Header('C', 'C')),
      ),
      new Header('D', 'D').addChild(
        new Header('E', 'E').addChild(new Header('F', 'F')),
      ),
    ];
    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          colSpan: 1,
          scope: 'col',
          rowSpan: 3,
        },
        {
          tag: 'th',
          text: 'D',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns the correct headers with `colgroup` header merged with identical parent on first column', () => {
    const columnHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('A', 'A').addChild(new Header('F', 'F')),
      ),
      new Header('B', 'B').addChild(
        new Header('C', 'C').addChild(new Header('F', 'F')),
      ),
      new Header('D', 'D').addChild(
        new Header('E', 'E').addChild(new Header('F', 'F')),
      ),
    ];
    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 2,
        },
        {
          tag: 'th',
          text: 'B',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns the correct headers with `col` header merged with identical parent', () => {
    const columnHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('B', 'B').addChild(new Header('C', 'C')),
      ),
      new Header('D', 'D').addChild(
        new Header('E', 'E').addChild(new Header('E', 'E')),
      ),
      new Header('F', 'F').addChild(
        new Header('G', 'G').addChild(new Header('H', 'H')),
      ),
    ];
    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'B',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          colSpan: 1,
          scope: 'col',
          rowSpan: 2,
        },
        {
          tag: 'th',
          text: 'G',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns the correct headers with `col` header merged with identical parent on first column', () => {
    const columnHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('B', 'B').addChild(new Header('B', 'B')),
      ),
      new Header('C', 'C').addChild(
        new Header('D', 'D').addChild(new Header('E', 'E')),
      ),
      new Header('F', 'F').addChild(
        new Header('G', 'G').addChild(new Header('H', 'H')),
      ),
    ];
    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'B',
          colSpan: 1,
          scope: 'col',
          rowSpan: 2,
        },
        {
          tag: 'th',
          text: 'D',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'G',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'E',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns the correct headers with multi-span `colgroup` merged with its identical groups', () => {
    const columnHeaders: Header[] = [
      new Header('B', 'B').addChild(
        new Header('A', 'A').addChild(new Header('F', 'F')),
      ),
      new Header('C', 'C').addChild(
        new Header('C', 'C')
          .addChild(new Header('F', 'F'))
          .addChild(new Header('F', 'F')),
      ),
    ];
    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          colSpan: 2,
          scope: 'colgroup',
          rowSpan: 2,
        },
      ],
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 2,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('does not return `colgroup` headers with multi-span subgroup with invalid rowspans and colspans', () => {
    const columnHeaders: Header[] = [
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
    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'B',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          colSpan: 3,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          colSpan: 2,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'E',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          colSpan: 2,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns the correct headers with one `col` header subgroup', () => {
    const columnHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('B', 'B')
          .addChild(new Header('C', 'C'))
          .addChild(new Header('D', 'D')),
      ),
    ];
    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 2,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'B',
          colSpan: 2,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'C',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns the correct headers with two `col` header subgroups', () => {
    const columnHeaders: Header[] = [
      new Header('A', 'A')
        .addChild(new Header('B', 'B').addChild(new Header('D', 'D')))
        .addChild(
          new Header('C', 'C')
            .addChild(new Header('E', 'E'))
            .addChild(new Header('F', 'F')),
        ),
    ];
    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 3,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'B',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          colSpan: 2,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'D',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'E',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns the correct headers with three `col` header subgroups', () => {
    const columnHeaders: Header[] = [
      new Header('A', 'A')
        .addChild(new Header('B', 'B').addChild(new Header('E', 'E')))
        .addChild(
          new Header('C', 'C')
            .addChild(new Header('F', 'F'))
            .addChild(new Header('G', 'G')),
        )
        .addChild(new Header('D', 'D').addChild(new Header('H', 'H'))),
    ];
    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          tag: 'th',
          text: 'A',
          colSpan: 4,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'B',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'C',
          colSpan: 2,
          scope: 'colgroup',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'D',
          colSpan: 1,
          scope: 'colgroup',
          rowSpan: 1,
        },
      ],
      [
        {
          tag: 'th',
          text: 'E',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'F',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'G',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
        {
          tag: 'th',
          text: 'H',
          colSpan: 1,
          scope: 'col',
          rowSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns correct headers with deeply nested rows and multiple identical headers', () => {
    const columnHeaders: Header[] = [
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

    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          colSpan: 1,
          rowSpan: 5,
          scope: 'col',
          tag: 'th',
          text: 'A',
        },
        {
          colSpan: 2,
          rowSpan: 2,
          scope: 'colgroup',
          tag: 'th',
          text: 'B',
        },
        {
          colSpan: 4,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'E',
        },
      ],
      [
        {
          colSpan: 4,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'F',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 3,
          scope: 'col',
          tag: 'th',
          text: 'B',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'C',
        },
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'F',
        },
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'H',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'D',
        },
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'col',
          tag: 'th',
          text: 'F',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'G',
        },
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'col',
          tag: 'th',
          text: 'I',
        },
        {
          colSpan: 1,
          rowSpan: 2,
          scope: 'col',
          tag: 'th',
          text: 'J',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          tag: 'th',
          text: 'D1',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          tag: 'th',
          text: 'G1',
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns correct headers with only headers merged with identical parent in the first row', () => {
    const colHeaders: Header[] = [
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

    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'A',
        },
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'B',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          tag: 'th',
          text: 'C',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          tag: 'th',
          text: 'D',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          tag: 'th',
          text: 'C',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          tag: 'th',
          text: 'D',
        },
      ],
    ];

    expect(createExpandedColumnHeaders(colHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns correct headers with only headers merged with identical parent in the middle row', () => {
    const colHeaders: Header[] = [
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

    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          colSpan: 4,
          rowSpan: 1,
          scope: 'colgroup',
          text: 'A',
          tag: 'th',
        },
      ],
      [
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          text: 'B',
          tag: 'th',
        },
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          text: 'C',
          tag: 'th',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          text: 'D',
          tag: 'th',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          text: 'E',
          tag: 'th',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          text: 'D',
          tag: 'th',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          text: 'E',
          tag: 'th',
        },
      ],
    ];

    expect(createExpandedColumnHeaders(colHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns correct headers with only headers merged with identical parent in the last row', () => {
    const colHeaders: Header[] = [
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

    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          colSpan: 4,
          rowSpan: 1,
          scope: 'colgroup',
          text: 'A',
          tag: 'th',
        },
      ],
      [
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          text: 'B',
          tag: 'th',
        },
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          text: 'C',
          tag: 'th',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          text: 'D',
          tag: 'th',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          text: 'E',
          tag: 'th',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          text: 'D',
          tag: 'th',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          text: 'E',
          tag: 'th',
        },
      ],
    ];

    expect(createExpandedColumnHeaders(colHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  test('returns correct headers when there are multiple groups with the same labels', () => {
    const columnHeaders: Header[] = [
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

    const expandedColumnHeaders: TableCellJson[][] = [
      [
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'A',
        },
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'B',
        },
      ],
      [
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'C',
        },
        {
          colSpan: 2,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'F',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'A',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'B',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'A',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'colgroup',
          tag: 'th',
          text: 'B',
        },
      ],
      [
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          tag: 'th',
          text: 'D',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          tag: 'th',
          text: 'E',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          tag: 'th',
          text: 'D',
        },
        {
          colSpan: 1,
          rowSpan: 1,
          scope: 'col',
          tag: 'th',
          text: 'E',
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });

  describe('a mix of rows with merged headers without siblings and merged headers with siblings', () => {
    test('returns correct headers for scenario 1', () => {
      const columnHeaders: Header[] = [
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

      const expandedColumnHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 3,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'B',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'Total',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'D',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Total',
          },
        ],
      ];

      expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
        expandedColumnHeaders,
      );
    });

    test('returns correct headers for scenario 2', () => {
      const columnHeaders: Header[] = [
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

      const expandedColumnHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 4,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'B',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'colgroup',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'colgroup',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'E',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'F',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'D',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'D',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'D',
          },
        ],
      ];

      expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
        expandedColumnHeaders,
      );
    });

    test('returns correct headers for scenario 3', () => {
      const columnHeaders: Header[] = [
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

      const expandedColumnHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 4,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'B',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 3,
            scope: 'col',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'colgroup',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'E',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'F',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'D',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'D',
          },
        ],
      ];

      expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
        expandedColumnHeaders,
      );
    });

    test('returns correct headers for scenario 4', () => {
      const columnHeaders: Header[] = [
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

      const expandedColumnHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'E',
          },
          {
            colSpan: 3,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'B',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'Total',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'D',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Total',
          },
        ],
      ];

      expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
        expandedColumnHeaders,
      );
    });

    test('returns correct headers for scenario 5', () => {
      const columnHeaders: Header[] = [
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

      const expandedColumnHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 3,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 5,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'B',
          },
        ],
        [
          {
            colSpan: 3,
            rowSpan: 2,
            scope: 'colgroup',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 4,
            rowSpan: 2,
            scope: 'colgroup',
            tag: 'th',
            text: 'Total',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'E',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'F',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'D',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'D',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'C',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'G',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'G',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'G',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'C',
          },
        ],
      ];

      expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
        expandedColumnHeaders,
      );
    });

    test('returns correct headers for scenario 6', () => {
      const columnHeaders: Header[] = [
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

      const expandedColumnHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 4,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'B',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'D',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'E',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'H',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'F',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'G',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'I',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'J',
          },
        ],
      ];

      expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
        expandedColumnHeaders,
      );
    });

    test('returns correct headers for scenario 7', () => {
      const columnHeaders: Header[] = [
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

      const expandedColumnHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 3,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'B',
          },
        ],
        [
          {
            colSpan: 3,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'E',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'F',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'H',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'F',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'H',
          },
          {
            colSpan: 1,
            rowSpan: 2,
            scope: 'col',
            tag: 'th',
            text: 'F',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'G',
          },
        ],
      ];

      expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
        expandedColumnHeaders,
      );
    });

    test('returns correct headers for scenario 8', () => {
      const columnHeaders: Header[] = [
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

      const expandedColumnHeaders: TableCellJson[][] = [
        [
          {
            colSpan: 4,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'A',
          },
          {
            colSpan: 4,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'B',
          },
        ],
        [
          {
            colSpan: 4,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'C',
          },
          {
            colSpan: 4,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'E',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'F',
          },
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'colgroup',
            tag: 'th',
            text: 'H',
          },
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'colgroup',
            tag: 'th',
            text: 'F',
          },
          {
            colSpan: 2,
            rowSpan: 2,
            scope: 'colgroup',
            tag: 'th',
            text: 'H',
          },
        ],
        [
          {
            colSpan: 2,
            rowSpan: 1,
            scope: 'colgroup',
            tag: 'th',
            text: 'G',
          },
        ],
        [
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'X',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Y',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'X',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Y',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'X',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Y',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'X',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            tag: 'th',
            text: 'Y',
          },
        ],
      ];

      expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
        expandedColumnHeaders,
      );
    });
  });
});
