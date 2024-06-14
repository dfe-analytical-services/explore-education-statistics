import render from '@common-test/render';
import DataSetFilePreview from '@frontend/modules/data-catalogue/components/DataSetFilePreview';
import { testDataSetCsvPreview } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import { screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('DataSetFilePreview', () => {
  test('renders correctly', () => {
    render(
      <DataSetFilePreview
        dataCsvPreview={testDataSetCsvPreview}
        onToggleFullScreen={noop}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Data set preview' }),
    ).toBeInTheDocument();

    const rows = within(
      screen.getByRole('table', {
        name: 'Table showing first 5 rows, from underlying data',
      }),
    ).getAllByRole('row');
    expect(rows).toHaveLength(6);

    const headerCells = within(rows[0]).getAllByRole('columnheader');
    expect(headerCells[0]).toHaveTextContent('time_period');
    expect(headerCells[1]).toHaveTextContent('geographic_level');
    expect(headerCells[2]).toHaveTextContent('filter_1');
    expect(headerCells[3]).toHaveTextContent('indicator_1');

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('201819');
    expect(row1Cells[1]).toHaveTextContent('National');
    expect(row1Cells[2]).toHaveTextContent('filter_1_value');
    expect(row1Cells[3]).toHaveTextContent('100');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('201920');
    expect(row2Cells[1]).toHaveTextContent('National');
    expect(row2Cells[2]).toHaveTextContent('filter_1_value');
    expect(row2Cells[3]).toHaveTextContent('101');

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('202021');
    expect(row3Cells[1]).toHaveTextContent('National');
    expect(row3Cells[2]).toHaveTextContent('filter_1_value');
    expect(row3Cells[3]).toHaveTextContent('102');

    const row4Cells = within(rows[4]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('202122');
    expect(row4Cells[1]).toHaveTextContent('National');
    expect(row4Cells[2]).toHaveTextContent('filter_1_value');
    expect(row4Cells[3]).toHaveTextContent('103');

    const row5Cells = within(rows[5]).getAllByRole('cell');
    expect(row5Cells[0]).toHaveTextContent('202223');
    expect(row5Cells[1]).toHaveTextContent('National');
    expect(row5Cells[2]).toHaveTextContent('filter_1_value');
    expect(row5Cells[3]).toHaveTextContent('104');

    expect(
      screen.getByRole('button', { name: 'Show full screen table' }),
    ).toBeInTheDocument();
  });

  test('renders the correct button when fullScreen is true', () => {
    render(
      <DataSetFilePreview
        dataCsvPreview={testDataSetCsvPreview}
        fullScreen
        onToggleFullScreen={noop}
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Close full screen table' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Show full screen table' }),
    ).not.toBeInTheDocument();
  });
});
