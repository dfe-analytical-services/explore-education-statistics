import ReleaseDataAndFilesAccordion from '@common/modules/release/components/ReleaseDataAndFilesAccordion';
import { Release } from '@common/services/publicationService';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { FileInfo } from '@common/services/types/file';

describe('ReleaseDataAndFilesAccordion', () => {
  const testRelease = {
    id: 'r1',
    downloadFiles: [
      {
        id: 'f1',
        name: 'All files',
        type: 'Ancillary',
      },
      {
        id: 'f2',
        name: 'Test data file',
        type: 'Data',
      },
      {
        id: 'f3',
        name: 'Test ancillary file',
        type: 'Ancillary',
      },
    ] as FileInfo[],
    hasMetaGuidance: true,
    hasPreReleaseAccessList: true,
    publication: {
      slug: 'p-slug',
    },
    slug: 'r-slug',
  };

  test('renders the data and files accordion', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease as Release}
        renderDownloadLink={file => <a href="/">{file.name}</a>}
        renderMetaGuidanceLink={<a href="#">mock meta guidance link</a>}
      />,
    );

    expect(screen.getByText('Explore data and files')).toBeInTheDocument();

    expect(screen.getByText('All files')).toBeInTheDocument();
    expect(screen.getByText('Test data file')).toBeInTheDocument();

    expect(screen.getByText('Open data')).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'mock meta guidance link' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Other files')).toBeInTheDocument();
    expect(screen.getByText('Test ancillary file')).toBeInTheDocument();
  });

  test('renders the create tables button', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease as Release}
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

  test('renders the pre-release access link', () => {
    render(
      <ReleaseDataAndFilesAccordion
        release={testRelease as Release}
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
