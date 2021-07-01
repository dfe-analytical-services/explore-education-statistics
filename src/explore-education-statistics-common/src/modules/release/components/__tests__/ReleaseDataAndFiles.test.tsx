import ReleaseDataAndFiles from '@common/modules/release/components/ReleaseDataAndFiles';
import { Release } from '@common/services/publicationService';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { FileInfo } from '@common/services/types/file';

describe('ReleaseDataAndFiles', () => {
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
        name: 'data file',
        type: 'Data',
      },
      {
        id: 'f3',
        name: 'ancillary file',
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
      <ReleaseDataAndFiles
        release={testRelease as Release}
        renderDownloadLink={file => <a href="/">{file.name}</a>}
        renderMetaGuidanceLink={<span>mock meta guidance link</span>}
      />,
    );

    expect(screen.getByText('Explore data and files')).toBeInTheDocument();

    expect(screen.getByText('All files')).toBeInTheDocument();
    expect(screen.getByText('data file')).toBeInTheDocument();

    expect(screen.getByText('Open data')).toBeInTheDocument();
    expect(screen.getByText('mock meta guidance link')).toBeInTheDocument();

    expect(screen.getByText('Other files')).toBeInTheDocument();
    expect(screen.getByText('ancillary file')).toBeInTheDocument();
  });

  test('renders the create tables button', () => {
    render(
      <ReleaseDataAndFiles
        release={testRelease as Release}
        renderCreateTablesButton={<span>mock create tables button</span>}
        renderDownloadLink={file => <a href="/">{file.name}</a>}
        renderMetaGuidanceLink={<span>mock meta guidance link</span>}
      />,
    );

    expect(screen.getByText('Create your own tables')).toBeInTheDocument();
    expect(screen.getByText('mock create tables button')).toBeInTheDocument();
  });

  test('renders the pre-release access link', () => {
    render(
      <ReleaseDataAndFiles
        release={testRelease as Release}
        renderPreReleaseAccessLink={<span>mock pre-release access link</span>}
        renderDownloadLink={file => <a href="/">{file.name}</a>}
        renderMetaGuidanceLink={<span>mock meta guidance link</span>}
      />,
    );

    expect(screen.getByText('Pre-release access list')).toBeInTheDocument();
    expect(
      screen.getByText('mock pre-release access link'),
    ).toBeInTheDocument();
  });
});
