import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import FixedMultiHeaderDataTable from '../FixedMultiHeaderDataTable';
import { TableCellJson } from '../../utils/mapTableToJson';

describe('FixedMultiHeaderDataTable', () => {
  test('renders underlying table', () => {
    const { container } = render(
      <FixedMultiHeaderDataTable
        caption="Test table"
        captionId="test-caption-id"
        footnotesId="test-footnotes-id"
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
  });

  test.each([
    { rows: 0, shouldShow: false, label: '0 rows' },
    { rows: 1, shouldShow: false, label: '1 row' },
    { rows: 9, shouldShow: false, label: '9 rows' },
    { rows: 10, shouldShow: false, label: '10 rows' },
    { rows: 11, shouldShow: true, label: '11 rows' },
    { rows: 12, shouldShow: true, label: '12 rows' },
    { rows: 20, shouldShow: true, label: '20 rows' },
    { rows: 50, shouldShow: true, label: '50 rows' },
  ])('Back to top link visibility ($label)', async ({ rows, shouldShow }) => {
    const rowTemplate = [
      {
        colSpan: 1,
        rowSpan: 1,
        scope: 'row' as const,
        tag: 'th',
        text: 'Row header',
      },
      { tag: 'td', text: '425,590' },
      { tag: 'td', text: '425,591' },
    ];

    render(
      <FixedMultiHeaderDataTable
        caption="Test table"
        captionId="test-caption-id"
        footnotesId="test-footnotes-id"
        tableJson={{
          tbody: Array.from(
            { length: rows },
            () => rowTemplate,
          ) as TableCellJson[][],
          thead: [
            [
              { colSpan: 1, rowSpan: 1, tag: 'td' },
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

    await waitFor(() => {
      const link = screen.queryByRole('link', { name: 'Back to top' });
      if (shouldShow) {
        expect(link).toBeInTheDocument();
      } else {
        expect(link).not.toBeInTheDocument();
      }
    });
  });
});
