import render from '@common-test/render';
import DataSetFileVariables from '@frontend/modules/data-catalogue/components/DataSetFileVariables';
import { testDataSetVariables } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('DataSetFileVariables', () => {
  test('renders the first five variables by default', () => {
    render(<DataSetFileVariables variables={testDataSetVariables} />);

    expect(
      screen.getByRole('heading', { name: 'Variables in this data set' }),
    ).toBeInTheDocument();

    const rows = within(
      screen.getByRole('table', {
        name: 'Table showing first 5 of 6 variables',
      }),
    ).getAllByRole('row');
    expect(rows).toHaveLength(6);

    const headerCells = within(rows[0]).getAllByRole('columnheader');
    expect(headerCells[0]).toHaveTextContent('Variable name');
    expect(headerCells[1]).toHaveTextContent('Variable description');

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('filter_1');
    expect(row1Cells[1]).toHaveTextContent('Filter 1 label');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('filter_2');
    expect(row2Cells[1]).toHaveTextContent('Filter 2 label');

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('indicator_1');
    expect(row3Cells[1]).toHaveTextContent('Indicator 1 label');

    const row4Cells = within(rows[4]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('indicator_2');
    expect(row4Cells[1]).toHaveTextContent('Indicator 2 label');

    const row5Cells = within(rows[5]).getAllByRole('cell');
    expect(row5Cells[0]).toHaveTextContent('indicator_3');
    expect(row5Cells[1]).toHaveTextContent('Indicator 3 label');

    expect(
      screen.getByRole('button', { name: 'Show all 6 variables' }),
    ).toBeInTheDocument();
  });

  test('renders all variables when show all is clicked', async () => {
    const { user } = render(
      <DataSetFileVariables variables={testDataSetVariables} />,
    );

    expect(
      screen.getByRole('heading', { name: 'Variables in this data set' }),
    ).toBeInTheDocument();

    const rows = within(
      screen.getByRole('table', {
        name: 'Table showing first 5 of 6 variables',
      }),
    ).getAllByRole('row');
    expect(rows).toHaveLength(6);

    await user.click(
      screen.getByRole('button', { name: 'Show all 6 variables' }),
    );

    expect(
      await screen.findByRole('button', { name: 'Show only 5 variables' }),
    ).toBeInTheDocument();

    const updatedRows = within(
      screen.getByRole('table', {
        name: 'Table showing all 6 variables',
      }),
    ).getAllByRole('row');
    expect(updatedRows).toHaveLength(7);

    const row6Cells = within(updatedRows[6]).getAllByRole('cell');
    expect(row6Cells[0]).toHaveTextContent('indicator_4');
    expect(row6Cells[1]).toHaveTextContent('Indicator 4 label');
  });

  test('does not render the show all button when there are fewer than 5 variables', () => {
    render(
      <DataSetFileVariables
        variables={[testDataSetVariables[0], testDataSetVariables[1]]}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Variables in this data set' }),
    ).toBeInTheDocument();

    const rows = within(
      screen.getByRole('table', {
        name: 'Table showing all 2 variables',
      }),
    ).getAllByRole('row');
    expect(rows).toHaveLength(3);

    const headerCells = within(rows[0]).getAllByRole('columnheader');
    expect(headerCells[0]).toHaveTextContent('Variable name');
    expect(headerCells[1]).toHaveTextContent('Variable description');

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('filter_1');
    expect(row1Cells[1]).toHaveTextContent('Filter 1 label');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('filter_2');
    expect(row2Cells[1]).toHaveTextContent('Filter 2 label');

    expect(
      screen.queryByRole('button', { name: 'Show all 6 variables' }),
    ).not.toBeInTheDocument();
  });
});
