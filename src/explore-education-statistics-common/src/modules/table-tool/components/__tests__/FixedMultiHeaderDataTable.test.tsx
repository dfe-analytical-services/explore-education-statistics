import React from 'react';
import { render } from 'react-testing-library';
import FixedMultiHeaderDataTable from '../FixedMultiHeaderDataTable';

describe('FixedMultiHeaderDataTable', () => {
  test('renders underlying table', () => {
    const { container } = render(
      <FixedMultiHeaderDataTable
        caption="Test table"
        columnHeaders={[
          ['Col group A', 'Col group B'],
          ['Col group C', 'Col group D'],
        ]}
        rowHeaders={[['Row group A', 'Row group B']]}
        rows={[['1', '2', '3', '4'], ['5', '6', '7', '8']]}
      />,
    );

    const table = container.querySelector('table') as HTMLElement;

    expect(table.querySelectorAll('thead tr')).toHaveLength(2);
    expect(table.querySelectorAll('thead tr:nth-child(1) th')).toHaveLength(2);
    expect(table.querySelectorAll('thead tr:nth-child(2) th')).toHaveLength(4);

    expect(table.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(table.querySelectorAll('tbody tr:nth-child(1) th')).toHaveLength(1);
    expect(table.querySelectorAll('tbody tr:nth-child(2) th')).toHaveLength(1);

    expect(table).toMatchSnapshot();
  });
});
