import { render } from '@testing-library/react';
import React from 'react';
import MultiHeaderTable from '../MultiHeaderTable';

describe('MultiHeaderTable', () => {
  test('can render basic table', () => {
    const { container } = render(
      <MultiHeaderTable
        tableJson={{
          tbody: [
            [
              {
                colSpan: 1,
                rowSpan: 1,
                scope: 'row',
                tag: 'th',
                text: 'Row header',
              },
              {
                tag: 'td',
                text: '425,590',
              },
              {
                tag: 'td',
                text: '425,591',
              },
            ],
          ],
          thead: [
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
                tag: 'th',
                text: 'Column heading',
              },
              {
                colSpan: 1,
                rowSpan: 1,
                scope: 'col',
                tag: 'th',
                text: 'Column heading 2',
              },
            ],
          ],
        }}
      />,
    );

    // 3x2 table
    expect(container.querySelectorAll('tbody tr')).toHaveLength(1);
    expect(container.querySelectorAll('tbody td')).toHaveLength(2);
    expect(container.innerHTML).toMatchSnapshot();

    expect(container.querySelectorAll('tbody tr th')).toHaveLength(1);

    // header
    expect(container.querySelectorAll('thead tr td')).toHaveLength(1);

    expect(container.querySelectorAll('thead tr th')).toHaveLength(2);

    expect(container.querySelectorAll('thead tr th')[0]).toHaveTextContent(
      'Column heading',
    );
    expect(container.querySelectorAll('thead tr th')[1]).toHaveTextContent(
      'Column heading 2',
    );

    // rows
    expect(container.querySelectorAll('tbody tr td')).toHaveLength(2);

    expect(container.querySelectorAll('tbody tr th')).toHaveLength(1);

    // row header
    expect(container.querySelectorAll('tbody tr th')[0]).toHaveTextContent(
      'Row header',
    );

    expect(container.querySelectorAll('tbody tr td')[0]).toHaveTextContent(
      '425,590',
    );
    expect(container.querySelectorAll('tbody tr td')[1]).toHaveTextContent(
      '425,591',
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('can render a complicated table with nesting', () => {
    const { container } = render(
      <MultiHeaderTable
        tableJson={{
          tbody: [
            [
              {
                rowSpan: 1,
                colSpan: 1,
                scope: 'row',
                text: 'Indicator value 1',
                tag: 'th',
              },
              {
                tag: 'td',
                text: '35',
              },
              {
                tag: 'td',
                text: 'no data',
              },
              {
                tag: 'td',
                text: '315',
              },
              {
                tag: 'td',
                text: 'no data',
              },
            ],
            [
              {
                rowSpan: 1,
                colSpan: 1,
                scope: 'row',
                text: 'Indicator value 2',
                tag: 'th',
              },
              {
                tag: 'td',
                text: '18,015',
              },
              {
                tag: 'td',
                text: '18,350',
              },
              {
                tag: 'td',
                text: '106,720',
              },
              {
                tag: 'td',
                text: '107,995',
              },
            ],
            [
              {
                rowSpan: 1,
                colSpan: 1,
                scope: 'row',
                text: 'Indicator value 3',
                tag: 'th',
              },
              {
                tag: 'td',
                text: '6,790',
              },
              {
                tag: 'td',
                text: '6,190',
              },
              {
                tag: 'td',
                text: '68,685',
              },
              {
                tag: 'td',
                text: '58,435',
              },
            ],
          ],
          thead: [
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
                text: 'State-funded primary',
                tag: 'th',
              },
              {
                colSpan: 2,
                rowSpan: 1,
                scope: 'colgroup',
                text: 'State-funded secondary',
                tag: 'th',
              },
            ],
            [
              {
                colSpan: 1,
                rowSpan: 1,
                scope: 'col',
                text: '2006/07',
                tag: 'th',
              },
              {
                colSpan: 1,
                rowSpan: 1,
                scope: 'col',
                text: '2007/08',
                tag: 'th',
              },
              {
                colSpan: 1,
                rowSpan: 1,
                scope: 'col',
                text: '2006/07',
                tag: 'th',
              },
              {
                colSpan: 1,
                rowSpan: 1,
                scope: 'col',
                text: '2007/08',
                tag: 'th',
              },
            ],
          ],
        }}
      />,
    );

    expect(container.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('tbody td')).toHaveLength(12);
    expect(container.innerHTML).toMatchSnapshot();

    // row 1
    expect(container.querySelectorAll('tbody tr th')).toHaveLength(3);

    const row1 = container.querySelectorAll('tbody tr')[0];

    expect(row1.querySelectorAll('th')[0]).toHaveTextContent(
      'Indicator value 1',
    );

    // row 1 cells
    expect(row1.querySelectorAll('td')).toHaveLength(4);

    const row1Cells = row1.querySelectorAll('td');

    expect(row1Cells[0]).toHaveTextContent('35');
    expect(row1Cells[1]).toHaveTextContent('no data');
    expect(row1Cells[2]).toHaveTextContent('315');
    expect(row1Cells[3]).toHaveTextContent('no data');

    // row 2
    const row2 = container.querySelectorAll('tbody tr')[1];
    expect(row2.querySelectorAll('th')).toHaveLength(1);

    expect(row2.querySelectorAll('th')[0]).toHaveTextContent(
      'Indicator value 2',
    );

    // row 2 cells
    expect(row2.querySelectorAll('td')).toHaveLength(4);

    const row2Cells = row2.querySelectorAll('td');

    expect(row2Cells[0]).toHaveTextContent('18,015');
    expect(row2Cells[1]).toHaveTextContent('18,350');
    expect(row2Cells[2]).toHaveTextContent('106,720');
    expect(row2Cells[3]).toHaveTextContent('107,995');

    // row 3
    const row3 = container.querySelectorAll('tbody tr')[2];
    expect(row3.querySelectorAll('th')).toHaveLength(1);

    expect(row3.querySelectorAll('th')[0]).toHaveTextContent(
      'Indicator value 3',
    );

    // row 3 cells
    expect(row3.querySelectorAll('td')).toHaveLength(4);

    const row3Cells = row3.querySelectorAll('td');

    expect(row3Cells[0]).toHaveTextContent('6,790');
    expect(row3Cells[1]).toHaveTextContent('6,190');
    expect(row3Cells[2]).toHaveTextContent('68,685');
    expect(row3Cells[3]).toHaveTextContent('58,435');

    // header
    expect(container.querySelectorAll('thead tr')).toHaveLength(2);

    // header row 1
    const headerRow1 = container.querySelectorAll('thead tr')[0];

    expect(headerRow1.querySelectorAll('th')).toHaveLength(2);

    expect(headerRow1.querySelectorAll('th')[0]).toHaveTextContent(
      'State-funded primary',
    );
    expect(headerRow1.querySelectorAll('th')[1]).toHaveTextContent(
      'State-funded secondary',
    );

    // header row 2
    const headerRow2 = container.querySelectorAll('thead tr')[1];

    expect(headerRow2.querySelectorAll('th')).toHaveLength(4);

    expect(headerRow2.querySelectorAll('th')[0]).toHaveTextContent('2006/07');
    expect(headerRow2.querySelectorAll('th')[1]).toHaveTextContent('2007/08');

    expect(headerRow2.querySelectorAll('th')[2]).toHaveTextContent('2006/07');
    expect(headerRow2.querySelectorAll('th')[3]).toHaveTextContent('2007/08');

    expect(container.innerHTML).toMatchSnapshot();
  });
});
