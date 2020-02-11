import React from 'react';
import { render } from 'react-testing-library';
import FixedMultiHeaderDataTable from '../FixedMultiHeaderDataTable';

describe('FixedMultiHeaderDataTable', () => {
  test('renders underlying table', () => {
    const { container } = render(
      <FixedMultiHeaderDataTable
        caption="Test table"
        columnHeaders={[
          { headers: [{ text: 'Col group A' }, { text: 'Col group B' }] },
          { headers: [{ text: 'Col group C' }, { text: 'Col group D' }] },
        ]}
        rowHeaders={[
          { headers: [{ text: 'Row group A' }, { text: 'Row group B' }] },
        ]}
        rows={[
          ['1', '2', '3', '4'],
          ['5', '6', '7', '8'],
        ]}
      />,
    );

    const table = container.querySelector('table') as HTMLElement;

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
