import Header from '@common/modules/table-tool/utils/Header';
import createExpandedColumnHeaders from '@common/modules/table-tool/utils/createExpandedColumnHeaders';
import { ExpandedHeader } from '@common/modules/table-tool/utils/mapTableToJson';

// these tests cover the test cases for headers that used to be in MultiHeaderDataTable

describe('createExpandedColumnHeaders', () => {
  test('should return a single row of column headers if no groups are provided', () => {
    const columnHeaders: Header[] = [
      new Header('1', '1'),
      new Header('2', '2'),
      new Header('3', '3'),
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual<
      ExpandedHeader[][]
    >([
      [
        { id: '1', text: '1', span: 1, crossSpan: 1, isGroup: false },
        { id: '2', text: '2', span: 1, crossSpan: 1, isGroup: false },
        { id: '3', text: '3', span: 1, crossSpan: 1, isGroup: false },
      ],
    ]);
  });

  test('should return multiple rows of column headers if groups are provided', () => {
    const columnHeaders: Header[] = [
      new Header('1', '1'),
      new Header('2', '2'),
      new Header('3', '3').addChild(new Header('2.1', '2.1')),
      new Header('4', '4').addChild(new Header('3.1', '3.1')),
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual<
      ExpandedHeader[][]
    >([
      [
        {
          crossSpan: 1,
          id: '1',
          isGroup: false,
          span: 1,
          text: '1',
        },
        {
          crossSpan: 1,
          id: '2',
          isGroup: false,
          span: 1,
          text: '2',
        },
        {
          crossSpan: 1,
          id: '3',
          isGroup: true,
          span: 1,
          text: '3',
        },
        {
          crossSpan: 1,
          id: '4',
          isGroup: true,
          span: 1,
          text: '4',
        },
      ],
      [
        {
          crossSpan: 1,
          id: '2.1',
          isGroup: false,
          span: 1,
          text: '2.1',
        },
        {
          crossSpan: 1,
          id: '3.1',
          isGroup: false,
          span: 1,
          text: '3.1',
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

    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'A',
          text: 'A',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'C',
          text: 'C',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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
    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'C',
          text: 'C',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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

    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'C',
          text: 'C',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'G',
          text: 'G',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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
    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'C',
          text: 'C',
          span: 1,
          isGroup: true,
          crossSpan: 2,
        },
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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
    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'C',
          text: 'C',
          span: 1,
          isGroup: true,
          crossSpan: 3,
        },
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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
    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
          crossSpan: 2,
        },
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'C',
          text: 'C',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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
    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: true,
          crossSpan: 2,
        },
        {
          id: 'G',
          text: 'G',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'C',
          text: 'C',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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
    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'C',
          text: 'C',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 2,
        },
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'G',
          text: 'G',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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
    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'C',
          text: 'C',
          span: 2,
          isGroup: true,
          crossSpan: 2,
        },
      ],
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 2,
          isGroup: false,
          crossSpan: 1,
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
    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'C',
          text: 'C',
          span: 3,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'C',
          text: 'C',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'E',
          text: 'E',
          span: 2,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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
    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'B',
          text: 'B',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'C',
          text: 'C',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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
    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 3,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'C',
          text: 'C',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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
    const expandedColumnHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 4,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'C',
          text: 'C',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'G',
          text: 'G',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
    ];

    expect(createExpandedColumnHeaders(columnHeaders)).toEqual(
      expandedColumnHeaders,
    );
  });
});
