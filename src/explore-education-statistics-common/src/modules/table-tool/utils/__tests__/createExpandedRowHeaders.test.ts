import Header from '../Header';
import createExpandedRowHeaders from '../createExpandedRowHeaders';
import { ExpandedHeader } from '../mapTableToJson';

describe('createExpandedRowHeaders', () => {
  test('should return a single row of row headers if no groups are provided', () => {
    const rowHeaders: Header[] = [
      new Header('1', '1'),
      new Header('2', '2'),
      new Header('3', '3'),
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual<ExpandedHeader[][]>([
      [
        {
          crossSpan: 1,
          id: '1',
          isGroup: false,
          span: 1,
          text: '1',
        },
      ],
      [
        {
          crossSpan: 1,
          id: '2',
          isGroup: false,
          span: 1,
          text: '2',
        },
      ],
      [
        {
          crossSpan: 1,
          id: '3',
          isGroup: false,
          span: 1,
          text: '3',
        },
      ],
    ]);
  });

  test('should return multiple rows of row headers if groups are provided', () => {
    const rowHeaders: Header[] = [
      new Header('1', '1'),
      new Header('2', '2'),
      new Header('3', '3').addChild(new Header('2.1', '2.1')),
      new Header('4', '4').addChild(new Header('3.1', '3.1')),
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual<ExpandedHeader[][]>([
      [
        {
          crossSpan: 1,
          id: '1',
          isGroup: false,
          span: 1,
          text: '1',
        },
      ],
      [
        {
          crossSpan: 1,
          id: '2',
          isGroup: false,
          span: 1,
          text: '2',
        },
      ],
      [
        {
          crossSpan: 1,
          id: '3',
          isGroup: true,
          span: 1,
          text: '3',
        },
        {
          crossSpan: 1,
          id: '2.1',
          isGroup: false,
          span: 1,
          text: '2.1',
        },
      ],
      [
        {
          crossSpan: 1,
          id: '4',
          isGroup: true,
          span: 1,
          text: '4',
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

  test('returns correct headers with 2 levels of 2 row headers', () => {
    const rowHeaders: Header[] = [
      new Header('1', '1')
        .addChild(new Header('3', '3'))
        .addChild(new Header('4', '4')),
      new Header('2', '2')
        .addChild(new Header('3', '3'))
        .addChild(new Header('4', '4')),
    ];
    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: '1',
          text: '1',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: '3',
          text: '3',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: '4',
          text: '4',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: '2',
          text: '2',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: '3',
          text: '3',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: '4',
          text: '4',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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
    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: '1',
          text: '1',
          span: 4,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: '3',
          text: '3',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: '5',
          text: '5',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: '6',
          text: '6',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: '4',
          text: '4',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: '5',
          text: '5',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: '6',
          text: '6',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: '2',
          text: '2',
          span: 4,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: '3',
          text: '3',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: '5',
          text: '5',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: '6',
          text: '6',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: '4',
          text: '4',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: '5',
          text: '5',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: '6',
          text: '6',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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
    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'A',
          text: 'A',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'C',
          text: 'C',
          span: 1,
          isGroup: false,
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
    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
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
      [
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
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: true,
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

    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
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
      [
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
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: true,
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
      [
        {
          id: 'E',
          text: 'E',
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
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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

    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
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
      [
        {
          id: 'C',
          text: 'C',
          span: 1,
          isGroup: true,
          crossSpan: 2,
        },
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
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
        {
          id: 'F',
          text: 'F',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with multi-span `rowgroup` merged with its identical groups', () => {
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

    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
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
      [
        {
          id: 'C',
          text: 'C',
          span: 2,
          isGroup: true,
          crossSpan: 2,
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

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with multi-span `rowgroup` header merged with 2 identical groups ', () => {
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

    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
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
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'D',
          text: 'D',
          span: 2,
          isGroup: true,
          crossSpan: 2,
        },
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: false,
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
      ],
      [
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
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('does not return `rowgroup` headers with multi-span subgroup with invalid rowspans and colspans', () => {
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

    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
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
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'C',
          text: 'C',
          span: 3,
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
          span: 2,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      undefined,
      [
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
          isGroup: false,
          crossSpan: 1,
        },
      ],
    ] as ExpandedHeader[][];

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

    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'B',
          text: 'B',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'C',
          text: 'C',
          span: 1,
          isGroup: false,
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

    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 3,
          isGroup: true,
          crossSpan: 1,
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
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
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
          isGroup: false,
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
    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 4,
          isGroup: true,
          crossSpan: 1,
        },
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
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'C',
          text: 'C',
          span: 2,
          isGroup: true,
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
      [
        {
          id: 'G',
          text: 'G',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: true,
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

    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
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
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: true,
          crossSpan: 2,
        },
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
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
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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

    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: true,
          crossSpan: 1,
        },
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
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: false,
          crossSpan: 3,
        },
      ],
      [
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
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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

    const expandedRowHeaders: ExpandedHeader[][] = [
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
          isGroup: false,
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
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
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
        {
          id: 'H',
          text: 'H',
          span: 1,
          isGroup: false,
          crossSpan: 1,
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

    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 3,
          isGroup: true,
          crossSpan: 1,
        },
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
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'C',
          text: 'C',
          span: 1,
          isGroup: false,
          crossSpan: 2,
        },
      ],
      [
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
          isGroup: false,
          crossSpan: 1,
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

    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 3,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: false,
          crossSpan: 2,
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
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'E',
          text: 'E',
          span: 1,
          isGroup: true,
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

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });

  test('returns correct headers with deeply nested rows and multiple identical headers', () => {
    const rowHeaders: Header[] = [
      new Header('A', 'A').addChild(
        new Header('A', 'A').addChild(
          new Header('A', 'A').addChild(new Header('A', 'A')),
        ),
      ),
      new Header('B', 'B').addChild(
        new Header('B', 'B')
          .addChild(new Header('B', 'B').addChild(new Header('B', 'B')))
          .addChild(new Header('C', 'C').addChild(new Header('D', 'D'))),
      ),
      new Header('E', 'E').addChild(
        new Header('F', 'F')
          .addChild(
            new Header('F', 'F')
              .addChild(new Header('F', 'F'))
              .addChild(new Header('G', 'G')),
          )
          .addChild(
            new Header('H', 'H')
              .addChild(new Header('I', 'I'))
              .addChild(new Header('J', 'J')),
          ),
      ),
    ];

    const expandedRowHeaders: ExpandedHeader[][] = [
      [
        {
          id: 'A',
          text: 'A',
          span: 1,
          isGroup: false,
          crossSpan: 4,
        },
      ],
      [
        {
          id: 'B',
          text: 'B',
          span: 2,
          isGroup: true,
          crossSpan: 2,
        },
        {
          id: 'B',
          text: 'B',
          span: 1,
          isGroup: false,
          crossSpan: 2,
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
          id: 'D',
          text: 'D',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'E',
          text: 'E',
          span: 4,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 4,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'F',
          text: 'F',
          span: 2,
          isGroup: true,
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
      [
        {
          id: 'G',
          text: 'G',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'H',
          text: 'H',
          span: 2,
          isGroup: true,
          crossSpan: 1,
        },
        {
          id: 'I',
          text: 'I',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
      [
        {
          id: 'J',
          text: 'J',
          span: 1,
          isGroup: false,
          crossSpan: 1,
        },
      ],
    ];

    expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  });
  //     new Header('A', 'A')
  //       .addChild(new Header('B', '').addChild(new Header('C', 'C')))
  //       .addChild(new Header('D', 'D').addChild(new Header('D', 'D'))),
  //   ];

  //   const expandedRowHeaders: ExpandedHeader[][] = [
  //     [
  //       { id: 'A', text: 'A', span: 2, isGroup: true, crossSpan: 1 },
  //       { id: 'C', text: 'C', span: 1, isGroup: false, crossSpan: 2 },
  //     ],
  //     [{ id: 'D', text: 'D', span: 1, isGroup: false, crossSpan: 2 }],
  //   ];
  //   expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  // });

  // test.skip('returns correct headers with all colspan = 1 when has empty header cell text', () => {
  //   const rowHeaders: Header[] = [
  //     new Header('A', 'A'), // not sure this is valid as would always have child?
  //     new Header('D', '')
  //       .addChild(new Header('E', 'E'))
  //       .addChild(new Header('F', 'F')),
  //   ];

  //   const expandedRowHeaders: ExpandedHeader[][] = [
  //     [
  //       {
  //         crossSpan: 2, // received 1 when shouldnt be
  //         id: 'A',
  //         isGroup: false,
  //         span: 1,
  //         text: 'A',
  //       },
  //     ],
  //     [
  //       {
  //         crossSpan: 2,
  //         id: 'E',
  //         isGroup: false,
  //         span: 1,
  //         text: 'E',
  //       },
  //     ],
  //     [
  //       {
  //         crossSpan: 2,
  //         id: 'F',
  //         isGroup: false,
  //         span: 1,
  //         text: 'F',
  //       },
  //     ],
  //   ];

  //   expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  // });

  // test('returns correct column headers for 3 levels with empty header cell text', () => {
  //   const rowHeaders: Header[] = [
  //     new Header('A', 'A')
  //       .addChild(new Header('B', 'B').addChild(new Header('E', 'E')))
  //       .addChild(new Header('C', 'C').addChild(new Header('F', 'F'))),

  //     new Header('G', 'G').addChild(
  //       new Header('I', '').addChild(new Header('K', 'K')),
  //     ),

  //     new Header('H', 'H').addChild(
  //       new Header('J', 'J').addChild(new Header('L', 'L')),
  //     ),
  //   ];

  //   const expandedRowHeaders: ExpandedHeader[][] = [
  //     [
  //       {
  //         crossSpan: 1,
  //         id: 'A',
  //         isGroup: true,
  //         span: 2,
  //         text: 'A',
  //       },
  //       {
  //         crossSpan: 1,
  //         id: 'B',
  //         isGroup: true,
  //         span: 1,
  //         text: 'B',
  //       },
  //       {
  //         crossSpan: 1,
  //         id: 'E',
  //         isGroup: false,
  //         span: 1,
  //         text: 'E',
  //       },
  //     ],
  //     [
  //       {
  //         crossSpan: 1,
  //         id: 'C',
  //         isGroup: true,
  //         span: 1,
  //         text: 'C',
  //       },
  //       {
  //         crossSpan: 1,
  //         id: 'F',
  //         isGroup: false,
  //         span: 1,
  //         text: 'F',
  //       },
  //     ],
  //     [
  //       {
  //         crossSpan: 1,
  //         id: 'G',
  //         isGroup: true,
  //         span: 1,
  //         text: 'G',
  //       },

  //       {
  //         crossSpan: 2,
  //         id: 'K',
  //         isGroup: false,
  //         span: 1,
  //         text: 'K',
  //       },
  //     ],
  //     [
  //       {
  //         crossSpan: 1,
  //         id: 'H',
  //         isGroup: true,
  //         span: 1,
  //         text: 'H',
  //       },
  //       {
  //         crossSpan: 1,
  //         id: 'J',
  //         isGroup: true,
  //         span: 1,
  //         text: 'J',
  //       },
  //       {
  //         crossSpan: 1,
  //         id: 'L',
  //         isGroup: false,
  //         span: 1,
  //         text: 'L',
  //       },
  //     ],
  //   ];

  //   expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  // });

  // test('NEW', () => {
  //   const rowHeaders: Header[] = [
  //     new Header('A', 'A')
  //       .addChild(
  //         new Header('B', 'B').addChild(
  //           new Header('B', 'B')
  //             .addChild(
  //               new Header('C', 'C')
  //                 .addChild(new Header('E', 'E'))
  //                 .addChild(new Header('F', 'F')),
  //             )
  //             .addChild(
  //               new Header('D', 'D')
  //                 .addChild(new Header('E', 'E'))
  //                 .addChild(new Header('F', 'F')),
  //             ),
  //         ),
  //       )
  //       .addChild(
  //         new Header('', '').addChild(
  //           new Header('G', 'G')
  //             .addChild(
  //               new Header('C', 'C')
  //                 .addChild(new Header('E', 'E'))
  //                 .addChild(new Header('F', 'F')),
  //             )
  //             .addChild(
  //               new Header('D', 'D')
  //                 .addChild(new Header('E', 'E'))
  //                 .addChild(new Header('F', 'F')),
  //             ),
  //         ),
  //       ),
  //   ];

  //   const expandedRowHeaders: ExpandedHeader[][] = [
  //     [
  //       { id: 'A', text: 'A', span: 8, isGroup: true, crossSpan: 1 },
  //       { id: 'B', text: 'B', span: 4, isGroup: true, crossSpan: 2 },
  //       { id: 'C', text: 'C', span: 2, isGroup: true, crossSpan: 1 },
  //       { id: 'E', text: 'E', span: 1, isGroup: false, crossSpan: 1 },
  //     ],
  //     [{ id: 'F', text: 'F', span: 1, isGroup: false, crossSpan: 1 }],
  //     [
  //       { id: 'D', text: 'D', span: 2, isGroup: true, crossSpan: 1 },
  //       { id: 'E', text: 'E', span: 1, isGroup: false, crossSpan: 1 },
  //     ],
  //     [{ id: 'F', text: 'F', span: 1, isGroup: false, crossSpan: 1 }],
  //     [
  //       { id: 'G', text: 'G', span: 4, isGroup: true, crossSpan: 2 },
  //       { id: 'C', text: 'C', span: 2, isGroup: true, crossSpan: 1 },
  //       { id: 'E', text: 'E', span: 1, isGroup: false, crossSpan: 1 },
  //     ],
  //     [{ id: 'F', text: 'F', span: 1, isGroup: false, crossSpan: 1 }],
  //     [
  //       { id: 'D', text: 'D', span: 2, isGroup: true, crossSpan: 1 },
  //       { id: 'E', text: 'E', span: 1, isGroup: false, crossSpan: 1 },
  //     ],
  //     [{ id: 'F', text: 'F', span: 1, isGroup: false, crossSpan: 1 }],
  //   ];

  //   expect(createExpandedRowHeaders(rowHeaders)).toEqual(expandedRowHeaders);
  // });
});
