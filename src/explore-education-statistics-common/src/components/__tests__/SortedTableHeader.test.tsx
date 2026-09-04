import userEvent from '@testing-library/user-event';
import React from 'react';
import { render, screen } from '@testing-library/react';
import SortedTableHeader, { TableSort } from '../SortedTableHeader';

describe('SortedTableHeader', () => {
  const renderHeader = (
    sort: TableSort<'name' | 'date'>,
    onClick: (column: 'name' | 'date') => void = jest.fn(),
  ): void => {
    render(
      <table>
        <thead>
          <tr>
            <SortedTableHeader
              column="name"
              label="Name"
              sort={sort}
              onClick={onClick}
            />
          </tr>
        </thead>
      </table>,
    );
  };

  test('renders both chevrons when the table is sorted by another column', () => {
    renderHeader({ column: 'date', direction: 'ascending' });

    expect(screen.getByRole('columnheader')).toHaveAttribute(
      'aria-sort',
      'none',
    );
    expect(
      screen.getByRole('button', { name: 'Name' }).querySelectorAll('path'),
    ).toHaveLength(2);
  });

  test('renders a single chevron when the table is sorted by this column ascending', () => {
    renderHeader({ column: 'name', direction: 'ascending' });

    expect(screen.getByRole('columnheader')).toHaveAttribute(
      'aria-sort',
      'ascending',
    );
    expect(
      screen.getByRole('button', { name: 'Name' }).querySelectorAll('path'),
    ).toHaveLength(1);
  });

  test('renders a single chevron when the table is sorted by this column descending', () => {
    renderHeader({ column: 'name', direction: 'descending' });

    expect(screen.getByRole('columnheader')).toHaveAttribute(
      'aria-sort',
      'descending',
    );
    expect(
      screen.getByRole('button', { name: 'Name' }).querySelectorAll('path'),
    ).toHaveLength(1);
  });

  test('calls `onClick` with the column when the control is clicked', async () => {
    const handleClick = jest.fn();

    renderHeader({ column: 'date', direction: 'ascending' }, handleClick);

    await userEvent.click(screen.getByRole('button', { name: 'Name' }));

    expect(handleClick).toHaveBeenCalledWith('name');
  });
});
