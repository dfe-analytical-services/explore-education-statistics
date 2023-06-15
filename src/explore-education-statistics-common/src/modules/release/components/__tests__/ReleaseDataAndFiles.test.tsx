/* eslint-disable @next/next/no-html-link-for-pages */
import ReleaseDataAndFiles from '@common/modules/release/components/ReleaseDataAndFiles';
import { Release } from '@common/services/publicationService';
import { FileInfo } from '@common/services/types/file';
import { within } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('ReleaseDataAndFiles', () => {
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

  const mockAllFilesButton = <a href="#">Mock download all data (zip)</a>;
  const mockCreateTablesButton = <a href="#">Mock create tables button</a>;
  const mockDataCatalogueLink = <a href="#">Mock data catalogue link</a>;
  const mockDownloadLink = (file: FileInfo) => (
    <a href="/">{`${file.name} (${file.extension}, ${file.size})`}</a>
  );
  const mockDataGuidanceLink = <a href="#">Mock data guidance link</a>;

  test('renders the download all data link if files are available', () => {
    render(
      <ReleaseDataAndFiles
        release={testRelease}
        renderAllFilesLink={mockAllFilesButton}
        renderCreateTablesLink={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(
      screen.getByText(
        'Download all data available in this release as a compressed ZIP file',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Mock download all data (zip)' }),
    ).toBeInTheDocument();
  });

  test('does not render the download all data link if no files are available', () => {
    render(
      <ReleaseDataAndFiles
        release={{
          ...testRelease,
          downloadFiles: [],
        }}
        renderAllFilesLink={mockAllFilesButton}
        renderCreateTablesLink={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(
      screen.queryByText(
        'Download all data available in this release as a compressed ZIP file',
      ),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', { name: 'Mock download all data' }),
    ).not.toBeInTheDocument();
  });

  test('renders the data catalogue section', () => {
    render(
      <ReleaseDataAndFiles
        release={testRelease}
        renderAllFilesLink={mockAllFilesButton}
        renderCreateTablesLink={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(
      screen.getByText(
        'Browse and download open data files from this release in our data catalogue',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Mock data catalogue link' }),
    ).toBeInTheDocument();
  });

  test('renders the download files list if showDataFileList is true', () => {
    render(
      <ReleaseDataAndFiles
        release={testRelease}
        renderAllFilesLink={mockAllFilesButton}
        renderCreateTablesLink={mockCreateTablesButton}
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
        name: 'Test data file 1 (csv, 10 KB)',
      }),
    ).toBeInTheDocument();

    expect(
      within(downloadFiles[1]).getByRole('link', {
        name: 'Test data file 2 (csv, 15 KB)',
      }),
    ).toBeInTheDocument();
  });

  test('renders the data guidance section if guidance is available', () => {
    render(
      <ReleaseDataAndFiles
        release={testRelease}
        renderAllFilesLink={mockAllFilesButton}
        renderCreateTablesLink={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

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
      <ReleaseDataAndFiles
        release={{
          ...testRelease,
          hasDataGuidance: false,
        }}
        renderAllFilesLink={mockAllFilesButton}
        renderCreateTablesLink={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

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
      <ReleaseDataAndFiles
        release={testRelease}
        renderAllFilesLink={mockAllFilesButton}
        renderCreateTablesLink={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(
      screen.getByText(
        'View tables that we have built for you, or create your own tables from open data using our table tool',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Mock create tables button' }),
    ).toBeInTheDocument();
  });

  test('renders the ancillary files section if ancillary files are available', () => {
    render(
      <ReleaseDataAndFiles
        release={testRelease}
        renderAllFilesLink={mockAllFilesButton}
        renderCreateTablesLink={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Additional supporting files' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        'All supporting files from this release are listed for individual download below:',
      ),
    ).toBeInTheDocument();

    // Files should be ordered alphabetically
    const otherFiles = within(screen.getByTestId('other-files')).getAllByRole(
      'listitem',
    );

    // File 1
    expect(
      within(otherFiles[0]).getByRole('link', {
        name: 'Test ancillary file 1 (txt, 25 KB)',
      }),
    ).toBeInTheDocument();
    expect(
      within(otherFiles[0]).getByText('A Test ancillary file 1 summary'),
    ).toBeInTheDocument();

    // File 2
    expect(
      within(otherFiles[1]).getByRole('link', {
        name: 'Test ancillary file 2 (pdf, 20 KB)',
      }),
    ).toBeInTheDocument();
    expect(
      within(otherFiles[1]).getByText('Test ancillary file 2 summary'),
    ).toBeInTheDocument();
  });

  test('does not render the ancillary files section if no ancillary files are available', () => {
    render(
      <ReleaseDataAndFiles
        release={{ ...testRelease, downloadFiles: [] }}
        renderAllFilesLink={mockAllFilesButton}
        renderCreateTablesLink={mockCreateTablesButton}
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

  test('renders related dashboards', () => {
    render(
      <ReleaseDataAndFiles
        release={testRelease}
        renderAllFilesLink={mockAllFilesButton}
        renderCreateTablesLink={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
        renderRelatedDashboards={<p>Related dashboard content</p>}
      />,
    );

    expect(screen.getByText('View related dashboard(s)')).toBeInTheDocument();

    expect(screen.getByText('Related dashboard content')).toBeInTheDocument();
  });

  test('does not render related dashboards', () => {
    render(
      <ReleaseDataAndFiles
        release={testRelease}
        renderAllFilesLink={mockAllFilesButton}
        renderCreateTablesLink={mockCreateTablesButton}
        renderDataCatalogueLink={mockDataCatalogueLink}
        renderDownloadLink={mockDownloadLink}
        renderDataGuidanceLink={mockDataGuidanceLink}
      />,
    );

    expect(
      screen.queryByText('View related dashboard(s)'),
    ).not.toBeInTheDocument();
  });
});
