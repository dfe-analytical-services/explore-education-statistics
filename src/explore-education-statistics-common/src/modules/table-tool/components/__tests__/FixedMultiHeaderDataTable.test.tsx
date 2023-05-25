import { render } from '@testing-library/react';
import React from 'react';
import FixedMultiHeaderDataTable from '../FixedMultiHeaderDataTable';

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
});
