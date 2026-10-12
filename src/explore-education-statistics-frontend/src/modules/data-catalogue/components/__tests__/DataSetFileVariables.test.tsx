import render from '@common-test/render';
import DataSetFileVariables from '@frontend/modules/data-catalogue/components/DataSetFileVariables';
import { testDataSetVariables } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';

// Note: jsdom does not load the `js-enabled` stylesheet that hides
// `dfe-js-hidden`, so these assert on the class rather than on visibility.
describe('DataSetFileVariables', () => {
  test('server-renders every variable, so they are readable without JS', () => {
    const container = document.createElement('div');
    container.innerHTML = renderToStaticMarkup(
      <DataSetFileVariables variables={testDataSetVariables} />,
    );

    const table = within(container).getByRole('table', {
      name: 'Table showing all 6 variables',
    });
    const rows = within(table).getAllByRole('row');
    expect(rows).toHaveLength(7);

    const row6Cells = within(rows[6]).getAllByRole('cell');
    expect(row6Cells[0]).toHaveTextContent('indicator_4');
    expect(row6Cells[1]).toHaveTextContent('Indicator 4 label');

    // Present in the HTML, but hidden once JS has loaded.
    expect(rows[5]).not.toHaveClass('dfe-js-hidden');
    expect(rows[6]).toHaveClass('dfe-js-hidden');
  });

  test('hides the overflow and describes the table as truncated once mounted', () => {
    render(<DataSetFileVariables variables={testDataSetVariables} />);

    expect(
      screen.getByRole('heading', { name: 'Variables in this data set' }),
    ).toBeInTheDocument();

    const rows = within(
      screen.getByRole('table', {
        name: 'Table showing first 5 of 6 variables',
      }),
    ).getAllByRole('row');
    expect(rows).toHaveLength(7);

    const headerCells = within(rows[0]).getAllByRole('columnheader');
    expect(headerCells[0]).toHaveTextContent('Variable name');
    expect(headerCells[1]).toHaveTextContent('Variable description');

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('filter_1');
    expect(row1Cells[1]).toHaveTextContent('Filter 1 label');

    const row5Cells = within(rows[5]).getAllByRole('cell');
    expect(row5Cells[0]).toHaveTextContent('indicator_3');
    expect(row5Cells[1]).toHaveTextContent('Indicator 3 label');

    expect(rows[1]).not.toHaveClass('dfe-js-hidden');
    expect(rows[5]).not.toHaveClass('dfe-js-hidden');
    expect(rows[6]).toHaveClass('dfe-js-hidden');

    expect(
      screen.getByRole('button', { name: 'Show all 6 variables' }),
    ).toHaveAttribute('aria-expanded', 'false');
  });

  test('reveals the hidden variables when show all is clicked', async () => {
    const { user } = render(
      <DataSetFileVariables variables={testDataSetVariables} />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Show all 6 variables' }),
    );

    expect(
      await screen.findByRole('button', { name: 'Show only 5 variables' }),
    ).toHaveAttribute('aria-expanded', 'true');

    const rows = within(
      screen.getByRole('table', {
        name: 'Table showing all 6 variables',
      }),
    ).getAllByRole('row');
    expect(rows).toHaveLength(7);

    rows.forEach(row => {
      expect(row).not.toHaveClass('dfe-js-hidden');
    });
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

    expect(rows[1]).not.toHaveClass('dfe-js-hidden');
    expect(rows[2]).not.toHaveClass('dfe-js-hidden');

    expect(screen.queryByRole('button')).not.toBeInTheDocument();
  });
});
