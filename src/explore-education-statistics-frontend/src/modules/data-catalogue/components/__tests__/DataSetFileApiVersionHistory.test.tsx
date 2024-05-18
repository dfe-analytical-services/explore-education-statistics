import render from '@common-test/render';
import DataSetFileApiVersionHistory from '@frontend/modules/data-catalogue/components/DataSetFileApiVersionHistory';
import { testApiDataSetVersions } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('DataSetFileApiVersionHistory', () => {
  test('renders correctly', () => {
    render(
      <DataSetFileApiVersionHistory
        currentVersion="2.0"
        dataSetFileId="data-set-file-id"
        dataSetVersions={testApiDataSetVersions}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'API data set version history' }),
    ).toBeInTheDocument();

    const table = within(screen.getByRole('table'));

    const rows = table.getAllByRole('row');
    expect(rows).toHaveLength(4);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('2.0 (current)');
    expect(within(row1Cells[0]).queryByRole('link')).not.toBeInTheDocument();
    expect(row1Cells[1]).toHaveTextContent('Published');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(
      within(row2Cells[0]).getByRole('link', { name: '1.2' }),
    ).toHaveAttribute('href', '/data-catalogue/data-set/data-set-file-id/1.2');
    expect(row2Cells[1]).toHaveTextContent('Deprecated');

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(
      within(row3Cells[0]).getByRole('link', { name: '1.0' }),
    ).toHaveAttribute('href', '/data-catalogue/data-set/data-set-file-id/1.0');
    expect(row3Cells[1]).toHaveTextContent('Withdrawn');
  });
});
