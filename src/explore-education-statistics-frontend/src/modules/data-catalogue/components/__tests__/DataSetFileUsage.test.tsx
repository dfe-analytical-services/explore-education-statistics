import render from '@common-test/render';
import DataSetFileUsage from '@frontend/modules/data-catalogue/components/DataSetFileUsage';
import { screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('DataSetFileUsage', () => {
  test('renders correctly without API data set', () => {
    render(
      <DataSetFileUsage
        dataSetFileId="data-set-file-id"
        hasApiDataSet={false}
        tableToolLink="test-table-tool-link"
        onDownload={noop}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Using this data' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Download this data set (ZIP)' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'View or create your own tables' }),
    ).toHaveAttribute('href', 'test-table-tool-link');

    expect(
      screen.queryByRole('link', { name: 'API documentation' }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with API data set', () => {
    render(
      <DataSetFileUsage
        dataSetFileId="data-set-file-id"
        hasApiDataSet
        tableToolLink="test-table-tool-link"
        onDownload={noop}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Using this data' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Download this data set (ZIP)' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'View or create your own tables' }),
    ).toHaveAttribute('href', 'test-table-tool-link');

    expect(
      screen.getByRole('link', {
        name: 'API documentation',
      }),
    ).toHaveAttribute(
      'href',
      'https://dev.statistics.api.education.gov.uk/docs',
    );
  });

  test('clicking the download button calls the `onDownload` handler', async () => {
    const handleDownload = jest.fn();

    const { user } = render(
      <DataSetFileUsage
        dataSetFileId="data-set-file-id"
        hasApiDataSet
        tableToolLink="test-table-tool-link"
        onDownload={handleDownload}
      />,
    );

    expect(handleDownload).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Download this data set (ZIP)' }),
    );

    expect(handleDownload).toHaveBeenCalledTimes(1);
  });

  test('renders download with code API link', () => {
    render(
      <DataSetFileUsage
        dataSetFileId="data-set-file-id"
        hasApiDataSet
        tableToolLink="test-table-tool-link"
        onDownload={noop}
      />,
    );

    expect(screen.getByTestId('copy-download-url')).toBeInTheDocument();
  });
});
