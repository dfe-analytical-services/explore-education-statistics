import ReleaseDataAndFilesAccordion from '@common/modules/release/components/ReleaseDataAndFilesAccordion';
import { Release } from '@common/services/publicationService';
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
        name: 'Test data file 2',
        size: '10 KB',
        type: 'Data',
        extension: 'csv',
      },
      {
        id: 'file-2',
        name: 'A Test data file 1',
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
    hasMetaGuidance: true,
    hasPreReleaseAccessList: true,
    publication: {
      slug: 'publication-1',
    },
    slug: 'release-1',
  } as Release;

  test('renders with files', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease}
        renderAllFilesButton={<a href="#">Mock all files button</a>}
        renderDownloadLink={file => <a href="/">{file.name}</a>}
        renderMetaGuidanceLink={<a href="#">mock meta guidance link</a>}
      />,
    );

    expect(screen.getByText('Explore data and files')).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Mock all files button' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Open data')).toBeInTheDocument();

    expect(screen.getByText('Other files')).toBeInTheDocument();
    expect(screen.getByText('List of other files')).toBeInTheDocument();

    userEvent.click(screen.getByText('List of other files'));

    // Files should be ordered alphabetically
    const otherFiles = within(
      screen.getByTestId('other-download-files'),
    ).getAllByRole('listitem');

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

  test('renders meta guidance link', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease}
        renderDownloadLink={file => <a href="/">{file.name}</a>}
        renderMetaGuidanceLink={<a href="#">mock meta guidance link</a>}
      />,
    );

    expect(
      screen.getByRole('link', { name: 'mock meta guidance link' }),
    ).toBeInTheDocument();
  });

  test('does not render meta guidance link if there is no meta guidance', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={{
          ...testRelease,
          hasMetaGuidance: false,
        }}
        renderDownloadLink={file => <a href="/">{file.name}</a>}
        renderMetaGuidanceLink={<a href="#">mock meta guidance link</a>}
      />,
    );

    expect(
      screen.queryByRole('link', { name: 'mock meta guidance link' }),
    ).not.toBeInTheDocument();
  });

  test('renders without files', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={{
          ...testRelease,
          downloadFiles: [],
        }}
        renderDownloadLink={file => <a href="/">{file.name}</a>}
        renderMetaGuidanceLink={<a href="#">mock meta guidance link</a>}
      />,
    );

    expect(screen.queryByTestId('download-files')).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('other-download-files'),
    ).not.toBeInTheDocument();
  });

  test('renders data catalogue link', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease}
        renderDataCatalogueLink={<a href="#">mock data catalogue link</a>}
        renderDownloadLink={file => <a href="/">{file.name}</a>}
        renderMetaGuidanceLink={<a href="#">mock meta guidance link</a>}
      />,
    );

    expect(
      screen.getByRole('link', { name: 'mock data catalogue link' }),
    ).toBeInTheDocument();
  });

  test('renders data files if there is no data catalogue link', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease}
        renderDownloadLink={file => <a href="/">{file.name}</a>}
        renderMetaGuidanceLink={<a href="#">mock meta guidance link</a>}
      />,
    );

    expect(
      screen.queryByRole('link', { name: 'mock data catalogue link' }),
    ).not.toBeInTheDocument();

    // Files should be ordered alphabetically, with the
    // 'All files' zip always being at the top
    const downloadFiles = within(
      screen.getByTestId('download-files'),
    ).getAllByRole('listitem');

    // File 1
    expect(
      within(downloadFiles[0]).getByRole('link', {
        name: 'A Test data file 1',
      }),
    ).toBeInTheDocument();
    expect(
      within(downloadFiles[0]).getByText('(csv, 15 KB)'),
    ).toBeInTheDocument();

    // File 2
    expect(
      within(downloadFiles[1]).getByRole('link', {
        name: 'Test data file 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(downloadFiles[1]).getByText('(csv, 10 KB)'),
    ).toBeInTheDocument();
  });

  test('renders create tables link', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease}
        renderCreateTablesButton={<a href="#">mock create tables button</a>}
        renderDownloadLink={file => <a href="/">{file.name}</a>}
        renderMetaGuidanceLink={<a href="#">mock meta guidance link</a>}
      />,
    );

    expect(screen.getByText('Create your own tables')).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'mock create tables button' }),
    ).toBeInTheDocument();
  });

  test('renders pre-release access link', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease}
        renderPreReleaseAccessLink={
          <a href="#">mock pre-release access link</a>
        }
        renderDownloadLink={file => <a href="/">{file.name}</a>}
        renderMetaGuidanceLink={<a href="#">mock meta guidance link</a>}
      />,
    );

    expect(screen.getByText('Pre-release access list')).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'mock pre-release access link' }),
    ).toBeInTheDocument();
  });
});
