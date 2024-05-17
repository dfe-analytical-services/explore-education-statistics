import render from '@common-test/render';
import DataSetFileApiVersionHistory from '@frontend/modules/data-catalogue/components/DataSetFileApiVersionHistory';
import { testApiDataSetVersions } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('DataSetFileVersionHistory', () => {
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

    const row1 = within(rows[1]);
    expect(row1.getByText('2.0'));
    expect(row1.getByText('Published'));

    const row2 = within(rows[2]);
    expect(row2.getByRole('link', { name: '1.2' })).toHaveAttribute(
      'href',
      '/data-catalogue/data-set/data-set-file-id/1.2',
    );
    expect(row2.getByText('Deprecated'));

    const row3 = within(rows[3]);
    expect(row3.getByRole('link', { name: '1.0' })).toHaveAttribute(
      'href',
      '/data-catalogue/data-set/data-set-file-id/1.0',
    );
    expect(row3.getByText('Withdrawn'));
  });
});
