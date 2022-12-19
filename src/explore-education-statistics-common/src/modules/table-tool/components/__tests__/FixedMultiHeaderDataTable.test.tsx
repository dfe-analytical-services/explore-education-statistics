import Header from '@common/modules/table-tool/components/utils/Header';
import { render } from '@testing-library/react';
import React from 'react';
import FixedMultiHeaderDataTable from '../FixedMultiHeaderDataTable';

describe('FixedMultiHeaderDataTable', () => {
  test('renders underlying table', () => {
    const { container } = render(
      <FixedMultiHeaderDataTable
        caption="Test table"
        captionId="test-caption-id"
        columnHeaders={[
          new Header('A', 'Col group A')
            .addChild(new Header('C', 'Col C'))
            .addChild(new Header('D', 'Col D')),
          new Header('B', 'Col group B')
            .addChild(new Header('C', 'Col C'))
            .addChild(new Header('D', 'Col D')),
        ]}
        footnotesId="test-footnotes-id"
        rowHeaders={[new Header('A', 'Row A'), new Header('B', 'Row B')]}
        rows={[
          ['1', '2', '3', '4'],
          ['5', '6', '7', '8'],
        ]}
      />,
    );

    const table = container.querySelector('table') as HTMLElement;

    expect(table).toHaveAttribute('aria-labelledby', 'test-caption-id');

    expect(table.querySelectorAll('thead tr')).toHaveLength(2);
    expect(table.querySelectorAll('thead tr:nth-child(1) th')).toHaveLength(2);
    expect(table.querySelectorAll('thead tr:nth-child(2) th')).toHaveLength(4);

    expect(table.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(table.querySelectorAll('tbody tr:nth-child(1) th')).toHaveLength(1);
    expect(table.querySelectorAll('tbody tr:nth-child(2) th')).toHaveLength(1);

    expect(table.querySelectorAll('tbody tr:nth-child(1) td')).toHaveLength(4);
    expect(table.querySelectorAll('tbody tr:nth-child(2) td')).toHaveLength(4);

    expect(table).toMatchSnapshot();
  });
});
