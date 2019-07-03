import React from 'react';
import { render } from 'react-testing-library';
import FixedMultiHeaderDataTable from '../FixedMultiHeaderDataTable';
import styles from '../FixedMultiHeaderDataTable.module.scss';

describe('FixedMultiHeaderDataTable', () => {
  test('renders a sticky header table', () => {
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

    const stickyHeader = container.getElementsByClassName(
      styles.headerTable,
    )[0];

    expect(stickyHeader).toHaveAttribute('aria-hidden', 'true');

    expect(stickyHeader.querySelectorAll('thead tr')).toHaveLength(2);
    expect(
      stickyHeader.querySelectorAll('thead tr:nth-child(1) th'),
    ).toHaveLength(2);
    expect(
      stickyHeader.querySelectorAll('thead tr:nth-child(2) th'),
    ).toHaveLength(4);

    expect(stickyHeader).toMatchSnapshot();
  });

  test('renders a sticky column table', () => {
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

    const stickyColumn = container.getElementsByClassName(
      styles.columnTable,
    )[0];

    expect(stickyColumn).toHaveAttribute('aria-hidden', 'true');

    expect(stickyColumn.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(
      stickyColumn.querySelectorAll('tbody tr:nth-child(1) th'),
    ).toHaveLength(1);
    expect(
      stickyColumn.querySelectorAll('tbody tr:nth-child(2) th'),
    ).toHaveLength(1);

    expect(stickyColumn).toMatchSnapshot();
  });

  test('renders a sticky intersection', () => {
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

    const intersection = container.getElementsByClassName(
      styles.intersectionTable,
    )[0];

    expect(intersection).toHaveAttribute('aria-hidden', 'true');

    expect(intersection.querySelector('thead tr td')).toHaveAttribute(
      'colspan',
      '1',
    );
    expect(intersection.querySelector('thead tr td')).toHaveAttribute(
      'rowspan',
      '2',
    );

    expect(intersection).toMatchSnapshot();
  });

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

    const table = container.querySelector(
      'table:not([aria-hidden])',
    ) as HTMLElement;

    expect(table.querySelectorAll('thead tr')).toHaveLength(2);
    expect(table.querySelectorAll('thead tr:nth-child(1) th')).toHaveLength(2);
    expect(table.querySelectorAll('thead tr:nth-child(2) th')).toHaveLength(4);

    expect(table.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(table.querySelectorAll('tbody tr:nth-child(1) th')).toHaveLength(1);
    expect(table.querySelectorAll('tbody tr:nth-child(2) th')).toHaveLength(1);

    expect(table).toMatchSnapshot();
  });
});
