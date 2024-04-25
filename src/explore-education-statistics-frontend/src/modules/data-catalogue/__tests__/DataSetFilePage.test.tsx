import _downloadService from '@common/services/downloadService';
import render from '@common-test/render';
import DataSetFilePage from '@frontend/modules/data-catalogue/DataSetFilePage';
import _dataSetService from '@frontend/services/dataSetFileService';
import { testDataSet } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

jest.mock('@frontend/services/dataSetFileService');
jest.mock('@common/services/downloadService');

const dataSetService = _dataSetService as jest.Mocked<typeof _dataSetService>;
const downloadService = _downloadService as jest.Mocked<
  typeof _downloadService
>;

describe('DataSetFilePage', () => {
  test('renders the page correctly', async () => {
    dataSetService.getDataSetFile.mockResolvedValue(testDataSet);
    render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

    await waitFor(() =>
      expect(screen.getByText('On this page')).toBeInTheDocument(),
    );

    expect(
      screen.getByRole('heading', { name: 'Data set 1' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Latest data')).toBeInTheDocument();
    expect(screen.getByText('1 January 2024')).toBeInTheDocument();
    expect(screen.getByText('Data set 1 summary')).toBeInTheDocument();

    expect(
      within(screen.getByTestId('Theme')).getByText('Theme 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Publication')).getByText('Publication 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Release')).getByRole('link', {
        name: 'Release 1',
      }),
    ).toHaveAttribute('href', '/find-statistics/publication-slug/release-slug');
    expect(
      within(screen.getByTestId('Release type')).getByRole('button', {
        name: /National statistics/,
      }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Geographic levels')).getByText(
        'Local authority, National',
      ),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText('Indicator 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Indicators')).getByText('Indicator 2'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('Filter 1'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Filters')).getByText('Filter 2'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Time period')).getByText('2023 to 2024'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Download data set (ZIP)' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Download this data set (ZIP)' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'View or create your own tables',
      }),
    ).toHaveAttribute('href', '/data-tables/publication-slug/release-slug');

    const nav = within(
      screen.getByRole('navigation', { name: 'On this page' }),
    );
    const navLinks = nav.getAllByRole('link');
    expect(navLinks).toHaveLength(2);
    expect(navLinks[0]).toHaveAttribute('href', '#details');
    expect(navLinks[1]).toHaveAttribute('href', '#using');
  });

  test('renders the `latest data` tag when it is the latest data', async () => {
    dataSetService.getDataSetFile.mockResolvedValue(testDataSet);
    render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

    await waitFor(() =>
      expect(screen.getByText('On this page')).toBeInTheDocument(),
    );

    expect(screen.getByText('Latest data')).toBeInTheDocument();
    expect(screen.queryByText('Not the latest data')).not.toBeInTheDocument();
  });

  test('renders the `not the latest data` tag when it is not the latest data', async () => {
    dataSetService.getDataSetFile.mockResolvedValue({
      ...testDataSet,
      release: { ...testDataSet.release, isLatestPublishedRelease: false },
    });
    render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

    await waitFor(() =>
      expect(screen.getByText('On this page')).toBeInTheDocument(),
    );

    expect(screen.getByText('Not the latest data')).toBeInTheDocument();

    expect(screen.queryByText('Latest data')).not.toBeInTheDocument();
  });

  test('calls the download service with the correct id when the download button is clicked', async () => {
    dataSetService.getDataSetFile.mockResolvedValue(testDataSet);
    render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

    await waitFor(() =>
      expect(screen.getByText('On this page')).toBeInTheDocument(),
    );

    await userEvent.click(
      screen.getByRole('button', { name: 'Download data set (ZIP)' }),
    );

    await waitFor(() => {
      expect(downloadService.downloadFiles).toHaveBeenCalledWith<
        Parameters<typeof downloadService.downloadFiles>
      >('release-id', ['file-id']);
    });
  });

  test('clicking the release type shows the modal', async () => {
    dataSetService.getDataSetFile.mockResolvedValue(testDataSet);
    render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

    await waitFor(() =>
      expect(screen.getByText('On this page')).toBeInTheDocument(),
    );

    await userEvent.click(
      screen.getByRole('button', { name: /National statistics/ }),
    );

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', { name: 'National statistics' }),
    ).toBeInTheDocument();
    expect(modal.getByRole('button', { name: 'Close' })).toBeInTheDocument();
  });
});
