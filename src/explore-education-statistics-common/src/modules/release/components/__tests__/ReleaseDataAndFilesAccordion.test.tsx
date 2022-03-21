import ReleaseDataAndFilesAccordion from '@common/modules/release/components/ReleaseDataAndFilesAccordion';
import { Release } from '@common/services/publicationService';
import { FileInfo } from '@common/services/types/file';
import { within } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('ReleaseDataAndFilesAccordion', () => {
  const testRelease = {
    id: 'r1',
    downloadFiles: [
      {
        id: 'file-1',
        name: 'Test data file 1',
        size: '10 KB',
        type: 'Data',
        extension: 'csv',
      },
      {
        id: 'file-2',
        name: 'Test data file 2',
        size: '15 KB',
        type: 'Data',
        extension: 'csv',
      },
      {
        id: 'file-3',
        name: 'All files',
        type: 'Ancillary',
        size: '100 KB',
        extension: 'zip',
      },
      {
        id: 'file-4',
        name: 'Test ancillary file 2',
        summary: 'Test ancillary file 2 summary',
        type: 'Ancillary',
        size: '20 KB',
        extension: 'pdf',
      },
      {
        id: 'file-5',
        name: 'Test ancillary file 1',
        summary: 'A Test ancillary file 1 summary',
        type: 'Ancillary',
        size: '25 KB',
        extension: 'txt',
      },
    ],
    hasDataGuidance: true,
    hasPreReleaseAccessList: true,
    publication: {
      slug: 'publication-1',
    },
    slug: 'release-1',
  } as Release;

  const mockAllFilesButton = <a href="#">Mock download all data</a>;
  const mockCreateTablesButton = <a href="#">Mock create tables button</a>;
  const mockDataCatalogueLink = <a href="#">Mock data catalogue link</a>;
  const mockDownloadLink = (file: FileInfo) => <a href="/">{file.name}</a>;
  const mockDataGuidanceLink = <a href="#">Mock data guidance link</a>;

  test('renders the download all data button if files are available', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease}
        renderAllFilesButton={mockAllFilesButton}
        renderCreateTablesButton={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(
      screen.getByText(
        'All data used in this release is available as open data for download',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Mock download all data' }),
    ).toBeInTheDocument();
  });

  test('does not render the download all data button if no files are available', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={{
          ...testRelease,
          downloadFiles: [],
        }}
        renderAllFilesButton={mockAllFilesButton}
        renderCreateTablesButton={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(
      screen.getByText(
        'All data used in this release is available as open data for download',
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('link', { name: 'Mock download all data' }),
    ).not.toBeInTheDocument();
  });

  test('renders the open data section', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease}
        renderAllFilesButton={mockAllFilesButton}
        renderCreateTablesButton={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(screen.getByText('Open data')).toBeInTheDocument();

    expect(
      screen.getByText(
        'Browse and download individual open data files from this release in our data catalogue',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Mock data catalogue link' }),
    ).toBeInTheDocument();
  });

  test('renders the download files list if showDataFileList is true', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease}
        renderAllFilesButton={mockAllFilesButton}
        renderCreateTablesButton={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
        showDownloadFilesList
      />,
    );

    userEvent.click(screen.getByText('Download files'));

    const downloadFiles = within(screen.getByTestId('data-files')).getAllByRole(
      'listitem',
    );

    expect(downloadFiles).toHaveLength(2);

    expect(
      within(downloadFiles[0]).getByRole('link', {
        name: 'Test data file 1',
      }),
    ).toBeInTheDocument();
    expect(
      within(downloadFiles[0]).getByText('(csv, 10 KB)'),
    ).toBeInTheDocument();

    expect(
      within(downloadFiles[1]).getByRole('link', {
        name: 'Test data file 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(downloadFiles[1]).getByText('(csv, 15 KB)'),
    ).toBeInTheDocument();
  });

  test('renders the data guidance section if guidance is available', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease}
        renderAllFilesButton={mockAllFilesButton}
        renderCreateTablesButton={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(screen.getByText('Guidance')).toBeInTheDocument();

    expect(
      screen.getByText(
        'Learn more about the data files used in this release using our online guidance',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Mock data guidance link' }),
    ).toBeInTheDocument();
  });

  test('does not render the data guidance section if guidance is not available', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={{
          ...testRelease,
          hasDataGuidance: false,
        }}
        renderAllFilesButton={mockAllFilesButton}
        renderCreateTablesButton={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(screen.queryByText('Guidance')).not.toBeInTheDocument();

    expect(
      screen.queryByText(
        'Learn more about the data files used in this release using our online guidance',
      ),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', { name: 'Mock data guidance link' }),
    ).not.toBeInTheDocument();
  });

  test('renders the create tables section', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease}
        renderAllFilesButton={mockAllFilesButton}
        renderCreateTablesButton={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(screen.getByText('Create your own tables')).toBeInTheDocument();

    expect(
      screen.getByText(
        'You can view featured tables that we have built for you, or create your own tables from the open data using our table tool',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Mock create tables button' }),
    ).toBeInTheDocument();
  });

  test('renders the ancillary files section if ancillary files are available', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease}
        renderAllFilesButton={mockAllFilesButton}
        renderCreateTablesButton={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'All supporting files' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        'All supporting files from this release are listed for individual download below:',
      ),
    ).toBeInTheDocument();

    userEvent.click(screen.getByText('List of all supporting files'));

    // Files should be ordered alphabetically
    const otherFiles = within(screen.getByTestId('other-files')).getAllByRole(
      'listitem',
    );

    // File 1
    expect(
      within(otherFiles[0]).getByRole('link', {
        name: 'Test ancillary file 1',
      }),
    ).toBeInTheDocument();
    expect(within(otherFiles[0]).getByText('(txt, 25 KB)')).toBeInTheDocument();
    expect(
      within(otherFiles[0]).getByText('A Test ancillary file 1 summary'),
    ).toBeInTheDocument();

    // File 2
    expect(
      within(otherFiles[1]).getByRole('link', {
        name: 'Test ancillary file 2',
      }),
    ).toBeInTheDocument();
    expect(within(otherFiles[1]).getByText('(pdf, 20 KB)')).toBeInTheDocument();
    expect(
      within(otherFiles[1]).getByText('Test ancillary file 2 summary'),
    ).toBeInTheDocument();
  });

  test('does not render the ancillary files section if no ancillary files are available', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={{ ...testRelease, downloadFiles: [] }}
        renderAllFilesButton={mockAllFilesButton}
        renderCreateTablesButton={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(
      screen.queryByRole('heading', { name: 'All supporting files' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(
        'All supporting files from this release are listed for individual download below:',
      ),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText('List of all supporting files'),
    ).not.toBeInTheDocument();
  });
});
