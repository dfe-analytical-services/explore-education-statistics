import _downloadService from '@common/services/downloadService';
import render from '@common-test/render';
import DataSetFilePage from '@frontend/modules/data-catalogue/DataSetFilePage';
import _apiDataSetService from '@frontend/services/apiDataSetService';
import _dataSetFileService from '@frontend/services/dataSetFileService';
import {
  testApiDataSet,
  testApiDataSetVersion,
  testApiDataSetVersions,
  testDataSetFile,
  testDataSetWithApi,
} from '@frontend/modules/data-catalogue/__data__/testDataSets';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';

jest.mock('@frontend/services/apiDataSetService');
jest.mock('@frontend/services/dataSetFileService');
jest.mock('@common/services/downloadService');

const apiDataSetService = _apiDataSetService as jest.Mocked<
  typeof _apiDataSetService
>;
const dataSetFileService = _dataSetFileService as jest.Mocked<
  typeof _dataSetFileService
>;
const downloadService = _downloadService as jest.Mocked<
  typeof _downloadService
>;

describe('DataSetFilePage', () => {
  test('renders the data set file heading, summary and info', async () => {
    dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetFile);
    render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

    await waitFor(() =>
      expect(screen.getByText('On this page')).toBeInTheDocument(),
    );

    expect(
      screen.getByRole('heading', { name: 'Data set 1' }),
    ).toBeInTheDocument();

    const infoSection = within(screen.getByTestId('data-set-file-info'));
    expect(infoSection.getByText('Latest data')).toBeInTheDocument();
    expect(infoSection.getByText('1 January 2024')).toBeInTheDocument();
    expect(
      infoSection.getByRole('button', { name: 'Download data set (ZIP)' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Data set 1 summary')).toBeInTheDocument();
  });

  test('renders the `latest data` tag when it is the latest data', async () => {
    dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetFile);
    render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

    await waitFor(() =>
      expect(screen.getByText('On this page')).toBeInTheDocument(),
    );

    expect(screen.getByText('Latest data')).toBeInTheDocument();
    expect(screen.queryByText('Not the latest data')).not.toBeInTheDocument();
  });

  test('renders the `not the latest data` tag when it is not the latest data', async () => {
    dataSetFileService.getDataSetFile.mockResolvedValue({
      ...testDataSetFile,
      release: {
        ...testDataSetFile.release,
        isLatestPublishedRelease: false,
      },
    });
    render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

    await waitFor(() =>
      expect(screen.getByText('On this page')).toBeInTheDocument(),
    );

    expect(screen.getByText('Not the latest data')).toBeInTheDocument();

    expect(screen.queryByText('Latest data')).not.toBeInTheDocument();
  });

  test('calls the download service with the correct id when the download button is clicked', async () => {
    dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetFile);
    const { user } = render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

    await waitFor(() =>
      expect(screen.getByText('On this page')).toBeInTheDocument(),
    );

    await user.click(
      screen.getByRole('button', { name: 'Download data set (ZIP)' }),
    );

    await waitFor(() => {
      expect(downloadService.downloadFiles).toHaveBeenCalledWith<
        Parameters<typeof downloadService.downloadFiles>
      >('release-id', ['file-id']);
    });
  });

  test('renders the data set details', async () => {
    dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetFile);
    render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

    await waitFor(() =>
      expect(screen.getByText('On this page')).toBeInTheDocument(),
    );

    expect(
      screen.getByRole('heading', { name: 'Data set details' }),
    ).toBeInTheDocument();
  });

  describe('non-api data set', () => {
    test('does not render the version info', async () => {
      dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetFile);
      render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

      await waitFor(() =>
        expect(screen.getByText('On this page')).toBeInTheDocument(),
      );

      const infoSection = within(screen.getByTestId('data-set-file-info'));
      expect(infoSection.queryByText('API version')).not.toBeInTheDocument();
    });

    test('renders the page navigation', async () => {
      dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetFile);
      render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

      await waitFor(() =>
        expect(screen.getByText('On this page')).toBeInTheDocument(),
      );

      const nav = within(
        screen.getByRole('navigation', { name: 'On this page' }),
      );
      const navLinks = nav.getAllByRole('link');
      expect(navLinks).toHaveLength(2);
      expect(navLinks[0]).toHaveAttribute('href', '#dataSetDetails');
      expect(navLinks[1]).toHaveAttribute('href', '#dataSetUsage');
    });

    test('renders the using this data section', async () => {
      dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetFile);
      render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

      await waitFor(() =>
        expect(screen.getByText('On this page')).toBeInTheDocument(),
      );

      expect(
        screen.getByRole('heading', {
          name: 'Using this data',
        }),
      ).toBeInTheDocument();
    });

    test('does not render the version history section', async () => {
      dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetFile);
      render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

      await waitFor(() =>
        expect(screen.getByText('On this page')).toBeInTheDocument(),
      );

      expect(
        screen.queryByRole('heading', { name: 'API data set version history' }),
      ).not.toBeInTheDocument();
    });

    test('does not render the quick start section', async () => {
      dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetFile);
      render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

      await waitFor(() =>
        expect(screen.getByText('On this page')).toBeInTheDocument(),
      );

      expect(
        screen.queryByRole('heading', {
          name: 'API data set endpoints quick start',
        }),
      ).not.toBeInTheDocument();
    });
  });

  describe('api data set', () => {
    test('renders the version info', async () => {
      dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetWithApi);
      apiDataSetService.getDataSet.mockResolvedValue(testApiDataSet);
      apiDataSetService.getDataSetVersion.mockResolvedValue(
        testApiDataSetVersion,
      );
      apiDataSetService.listDataSetVersions.mockResolvedValue(
        testApiDataSetVersions,
      );
      render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

      await waitFor(() =>
        expect(screen.getByText('On this page')).toBeInTheDocument(),
      );

      const infoSection = within(screen.getByTestId('data-set-file-info'));
      expect(infoSection.getByText('API version')).toBeInTheDocument();
      expect(infoSection.getByText('1.0')).toBeInTheDocument();
    });

    test('renders the page navigation', async () => {
      dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetWithApi);
      apiDataSetService.getDataSet.mockResolvedValue(testApiDataSet);
      apiDataSetService.getDataSetVersion.mockResolvedValue(
        testApiDataSetVersion,
      );
      apiDataSetService.listDataSetVersions.mockResolvedValue(
        testApiDataSetVersions,
      );
      render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

      await waitFor(() =>
        expect(screen.getByText('On this page')).toBeInTheDocument(),
      );

      const nav = within(
        screen.getByRole('navigation', { name: 'On this page' }),
      );
      const navLinks = nav.getAllByRole('link');
      expect(navLinks).toHaveLength(4);
      expect(navLinks[0]).toHaveAttribute('href', '#dataSetDetails');
      expect(navLinks[1]).toHaveAttribute('href', '#dataSetUsage');
      expect(navLinks[2]).toHaveAttribute('href', '#apiVersionHistory');
      expect(navLinks[3]).toHaveAttribute('href', '#apiQuickStart');
    });

    test('renders the using this data section', async () => {
      dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetWithApi);
      apiDataSetService.getDataSet.mockResolvedValue(testApiDataSet);
      apiDataSetService.getDataSetVersion.mockResolvedValue(
        testApiDataSetVersion,
      );
      apiDataSetService.listDataSetVersions.mockResolvedValue(
        testApiDataSetVersions,
      );
      render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

      await waitFor(() =>
        expect(screen.getByText('On this page')).toBeInTheDocument(),
      );

      expect(
        screen.getByRole('heading', {
          name: 'Using this data',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('link', { name: 'View or create your own tables' }),
      ).toHaveAttribute(
        'href',
        '/data-tables/publication-slug/release-slug?subjectId=subject-id',
      );
    });

    test('renders the version history section', async () => {
      dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetWithApi);
      apiDataSetService.getDataSet.mockResolvedValue(testApiDataSet);
      apiDataSetService.getDataSetVersion.mockResolvedValue(
        testApiDataSetVersion,
      );
      apiDataSetService.listDataSetVersions.mockResolvedValue(
        testApiDataSetVersions,
      );
      render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

      await waitFor(() =>
        expect(screen.getByText('On this page')).toBeInTheDocument(),
      );

      expect(
        screen.getByRole('heading', { name: 'API data set version history' }),
      ).toBeInTheDocument();
    });

    test('renders the quick start section', async () => {
      dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetWithApi);
      apiDataSetService.getDataSet.mockResolvedValue(testApiDataSet);
      apiDataSetService.getDataSetVersion.mockResolvedValue(
        testApiDataSetVersion,
      );
      apiDataSetService.listDataSetVersions.mockResolvedValue(
        testApiDataSetVersions,
      );
      render(<DataSetFilePage dataSetFileId="datasetfile-id" />);

      await waitFor(() =>
        expect(screen.getByText('On this page')).toBeInTheDocument(),
      );

      expect(
        screen.getByRole('heading', {
          name: 'API data set quick start',
        }),
      ).toBeInTheDocument();
    });
  });
});
