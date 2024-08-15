import render from '@common-test/render';
import _downloadService from '@common/services/downloadService';
import { ApiDataSetVersionChanges } from '@common/services/types/apiDataSetChanges';
import {
  testApiDataSet,
  testApiDataSetVersion,
  testDataSetFile,
} from '@frontend/modules/data-catalogue/__data__/testDataSets';
import DataSetFilePage from '@frontend/modules/data-catalogue/DataSetFilePage';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';

jest.mock('@common/services/downloadService');

const downloadService = _downloadService as jest.Mocked<
  typeof _downloadService
>;

describe('DataSetFilePage', () => {
  test('renders the data set file heading, summary and info', async () => {
    render(<DataSetFilePage dataSetFile={testDataSetFile} />);

    expect(await screen.findByText('On this page')).toBeInTheDocument();

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
    render(<DataSetFilePage dataSetFile={testDataSetFile} />);

    expect(await screen.findByText('On this page')).toBeInTheDocument();

    expect(screen.getByText('Latest data')).toBeInTheDocument();
    expect(screen.queryByText('Not the latest data')).not.toBeInTheDocument();
  });

  test('renders the `not the latest data` tag when it is not the latest data', async () => {
    render(
      <DataSetFilePage
        dataSetFile={{
          ...testDataSetFile,
          release: {
            ...testDataSetFile.release,
            isLatestPublishedRelease: false,
          },
        }}
      />,
    );

    expect(await screen.findByText('On this page')).toBeInTheDocument();

    expect(screen.getByText('Not the latest data')).toBeInTheDocument();

    expect(screen.queryByText('Latest data')).not.toBeInTheDocument();
  });

  test('renders the `not the latest data` tag when the publication is superseded', async () => {
    render(
      <DataSetFilePage
        dataSetFile={{
          ...testDataSetFile,
          release: {
            ...testDataSetFile.release,
            isSuperseded: true,
          },
        }}
      />,
    );

    expect(await screen.findByText('On this page')).toBeInTheDocument();

    expect(screen.getByText('Not the latest data')).toBeInTheDocument();

    expect(screen.queryByText('Latest data')).not.toBeInTheDocument();
  });

  test('calls the download service with the correct id when the download button is clicked', async () => {
    const { user } = render(<DataSetFilePage dataSetFile={testDataSetFile} />);

    expect(await screen.findByText('On this page')).toBeInTheDocument();

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
    render(<DataSetFilePage dataSetFile={testDataSetFile} />);

    expect(await screen.findByText('On this page')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Data set details' }),
    ).toBeInTheDocument();
  });

  test('renders the data set preview section', async () => {
    render(<DataSetFilePage dataSetFile={testDataSetFile} />);

    expect(await screen.findByText('On this page')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Data set preview' }),
    ).toBeInTheDocument();
  });

  test('renders the data set variables section', async () => {
    render(<DataSetFilePage dataSetFile={testDataSetFile} />);

    expect(await screen.findByText('On this page')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Variables in this data set' }),
    ).toBeInTheDocument();
  });

  test('renders the data set footnotes section', async () => {
    render(<DataSetFilePage dataSetFile={testDataSetFile} />);

    expect(await screen.findByText('On this page')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Footnotes' }),
    ).toBeInTheDocument();
  });

  test('does not render the data set footnotes section when there are no footnotes', async () => {
    render(
      <DataSetFilePage dataSetFile={{ ...testDataSetFile, footnotes: [] }} />,
    );

    expect(await screen.findByText('On this page')).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Footnotes' }),
    ).not.toBeInTheDocument();
  });

  test('renders the data set usage section', async () => {
    render(<DataSetFilePage dataSetFile={testDataSetFile} />);

    expect(await screen.findByText('On this page')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Using this data' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'View or create your own tables' }),
    ).toHaveAttribute(
      'href',
      '/data-tables/publication-slug/release-slug?subjectId=subject-id',
    );
  });

  describe('non-API data set', () => {
    test('does not render API version info', async () => {
      render(<DataSetFilePage dataSetFile={testDataSetFile} />);

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      const infoSection = within(screen.getByTestId('data-set-file-info'));
      expect(infoSection.queryByText('API version')).not.toBeInTheDocument();
    });

    test('renders the page navigation correctly', async () => {
      render(<DataSetFilePage dataSetFile={testDataSetFile} />);

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      const nav = within(
        screen.getByRole('navigation', { name: 'On this page' }),
      );
      const navLinks = nav.getAllByRole('link');
      expect(navLinks).toHaveLength(5);
      expect(navLinks[0]).toHaveAttribute('href', '#dataSetDetails');
      expect(navLinks[1]).toHaveAttribute('href', '#dataSetPreview');
      expect(navLinks[2]).toHaveAttribute('href', '#dataSetVariables');
      expect(navLinks[3]).toHaveAttribute('href', '#dataSetFootnotes');
      expect(navLinks[4]).toHaveAttribute('href', '#dataSetUsage');
    });

    test('renders the page navigation correctly when there are no footnotes', async () => {
      render(
        <DataSetFilePage dataSetFile={{ ...testDataSetFile, footnotes: [] }} />,
      );

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      const nav = within(
        screen.getByRole('navigation', { name: 'On this page' }),
      );
      const navLinks = nav.getAllByRole('link');
      expect(navLinks).toHaveLength(4);
      expect(navLinks[0]).toHaveAttribute('href', '#dataSetDetails');
      expect(navLinks[1]).toHaveAttribute('href', '#dataSetPreview');
      expect(navLinks[2]).toHaveAttribute('href', '#dataSetVariables');
      expect(navLinks[3]).toHaveAttribute('href', '#dataSetUsage');
    });

    test('does not render the API version history section', async () => {
      render(<DataSetFilePage dataSetFile={testDataSetFile} />);

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      expect(
        screen.queryByRole('heading', { name: 'API data set version history' }),
      ).not.toBeInTheDocument();
    });

    test('does not render the API quick start section', async () => {
      render(<DataSetFilePage dataSetFile={testDataSetFile} />);

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      expect(
        screen.queryByRole('heading', { name: 'API data set quick start' }),
      ).not.toBeInTheDocument();
    });
  });

  describe('API data set', () => {
    test('renders the version info', async () => {
      render(
        <DataSetFilePage
          apiDataSet={testApiDataSet}
          apiDataSetVersion={testApiDataSetVersion}
          dataSetFile={testDataSetFile}
        />,
      );

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      const infoSection = within(screen.getByTestId('data-set-file-info'));
      expect(infoSection.getByText('API version')).toBeInTheDocument();
      expect(infoSection.getByText('1.0')).toBeInTheDocument();
    });

    test('renders the page navigation correctly', async () => {
      render(
        <DataSetFilePage
          apiDataSet={testApiDataSet}
          apiDataSetVersion={testApiDataSetVersion}
          dataSetFile={testDataSetFile}
        />,
      );

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      const nav = within(
        screen.getByRole('navigation', { name: 'On this page' }),
      );

      const navLinks = nav.getAllByRole('link');
      expect(navLinks).toHaveLength(7);
      expect(navLinks[0]).toHaveAttribute('href', '#dataSetDetails');
      expect(navLinks[1]).toHaveAttribute('href', '#dataSetPreview');
      expect(navLinks[2]).toHaveAttribute('href', '#dataSetVariables');
      expect(navLinks[3]).toHaveAttribute('href', '#dataSetFootnotes');
      expect(navLinks[4]).toHaveAttribute('href', '#dataSetUsage');
      expect(navLinks[5]).toHaveAttribute('href', '#apiQuickStart');
      expect(navLinks[6]).toHaveAttribute('href', '#apiVersionHistory');
    });

    test('renders the page navigation correctly when there are no footnotes', async () => {
      render(
        <DataSetFilePage
          apiDataSet={testApiDataSet}
          apiDataSetVersion={testApiDataSetVersion}
          dataSetFile={{ ...testDataSetFile, footnotes: [] }}
        />,
      );

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      const nav = within(
        screen.getByRole('navigation', { name: 'On this page' }),
      );

      const navLinks = nav.getAllByRole('link');
      expect(navLinks).toHaveLength(6);
      expect(navLinks[0]).toHaveAttribute('href', '#dataSetDetails');
      expect(navLinks[1]).toHaveAttribute('href', '#dataSetPreview');
      expect(navLinks[2]).toHaveAttribute('href', '#dataSetVariables');
      expect(navLinks[3]).toHaveAttribute('href', '#dataSetUsage');
      expect(navLinks[4]).toHaveAttribute('href', '#apiQuickStart');
      expect(navLinks[5]).toHaveAttribute('href', '#apiVersionHistory');
    });

    test('renders the page navigation correctly when there is an API changelog', async () => {
      const testApiDataSetVersionChanges: ApiDataSetVersionChanges = {
        majorChanges: {
          filters: [
            { previousState: { id: 'filter-1', label: 'Filter 1', hint: '' } },
          ],
        },
        minorChanges: {},
      };

      render(
        <DataSetFilePage
          apiDataSet={testApiDataSet}
          apiDataSetVersion={testApiDataSetVersion}
          apiDataSetVersionChanges={testApiDataSetVersionChanges}
          dataSetFile={{ ...testDataSetFile, footnotes: [] }}
        />,
      );

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      const nav = within(
        screen.getByRole('navigation', { name: 'On this page' }),
      );

      const navLinks = nav.getAllByRole('link');
      expect(navLinks).toHaveLength(7);
      expect(navLinks[0]).toHaveAttribute('href', '#dataSetDetails');
      expect(navLinks[1]).toHaveAttribute('href', '#dataSetPreview');
      expect(navLinks[2]).toHaveAttribute('href', '#dataSetVariables');
      expect(navLinks[3]).toHaveAttribute('href', '#dataSetUsage');
      expect(navLinks[4]).toHaveAttribute('href', '#apiQuickStart');
      expect(navLinks[5]).toHaveAttribute('href', '#apiVersionHistory');
      expect(navLinks[6]).toHaveAttribute('href', '#apiChangelog');
    });

    test('renders the API version history section', async () => {
      render(
        <DataSetFilePage
          apiDataSet={testApiDataSet}
          apiDataSetVersion={testApiDataSetVersion}
          dataSetFile={testDataSetFile}
        />,
      );

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      expect(
        screen.getByRole('heading', { name: 'API data set version history' }),
      ).toBeInTheDocument();
    });

    test('renders the API quick start section', async () => {
      render(
        <DataSetFilePage
          apiDataSet={testApiDataSet}
          apiDataSetVersion={testApiDataSetVersion}
          dataSetFile={testDataSetFile}
        />,
      );

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      expect(
        screen.getByRole('heading', { name: 'API data set quick start' }),
      ).toBeInTheDocument();
    });

    test('renders the API changelog', async () => {
      const testApiDataSetVersionChanges: ApiDataSetVersionChanges = {
        majorChanges: {
          filters: [
            { previousState: { id: 'filter-1', label: 'Filter 1', hint: '' } },
          ],
        },
        minorChanges: {},
      };

      render(
        <DataSetFilePage
          apiDataSet={testApiDataSet}
          apiDataSetVersion={{
            ...testApiDataSetVersion,
            version: '2.0',
          }}
          apiDataSetVersionChanges={testApiDataSetVersionChanges}
          dataSetFile={testDataSetFile}
        />,
      );

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      expect(
        screen.getByRole('heading', { name: 'API data set changelog' }),
      ).toBeInTheDocument();
    });

    test('does not render the API changelog if none provided', async () => {
      render(
        <DataSetFilePage
          apiDataSet={testApiDataSet}
          apiDataSetVersion={testApiDataSetVersion}
          dataSetFile={testDataSetFile}
        />,
      );

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      expect(
        screen.queryByRole('heading', { name: 'API data set changelog' }),
      ).not.toBeInTheDocument();
    });

    test('does not render the API changelog if it is empty', async () => {
      render(
        <DataSetFilePage
          apiDataSet={testApiDataSet}
          apiDataSetVersion={testApiDataSetVersion}
          apiDataSetVersionChanges={{
            majorChanges: {},
            minorChanges: {},
          }}
          dataSetFile={testDataSetFile}
        />,
      );

      expect(await screen.findByText('On this page')).toBeInTheDocument();

      expect(
        screen.queryByRole('heading', { name: 'API data set changelog' }),
      ).not.toBeInTheDocument();
    });
  });
});
