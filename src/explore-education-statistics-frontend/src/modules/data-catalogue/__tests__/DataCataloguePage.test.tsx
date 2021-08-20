import DataCataloguePage from '@frontend/modules/data-catalogue/DataCataloguePage';
import {
  DownloadTheme,
  PublicationSummary,
  Theme,
} from '@common/services/themeService';
import _publicationService, {
  ReleaseSummary,
} from '@common/services/publicationService';
import { SubjectWithDownloadFiles } from '@frontend/modules/data-catalogue/components/DownloadStep';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import preloadAll from 'jest-next-dynamic';
import React from 'react';

jest.mock('@common/services/publicationService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('DataCataloguePage', () => {
  const testThemes: Theme[] = [
    {
      id: 'theme-1',
      title: 'Pupils and schools',
      topics: [
        {
          id: 'topic-1',
          title: 'Admission appeals',
          publications: [
            {
              id: 'publication-1',
              title: 'Test publication',
              slug: 'test-publication',
            },
          ],
        },
      ],
    } as DownloadTheme,
  ];

  const testPublication = {
    id: 'publication-1',
    title: 'Test publication',
    slug: 'test-publication',
  } as PublicationSummary;

  const testReleases: ReleaseSummary[] = [
    {
      id: 'rel-1',
      latestRelease: true,
      published: '2021-06-30T11:21:17',
      slug: 'rel-1-slug',
      title: 'Release 1',
    } as ReleaseSummary,
    {
      id: 'rel-3',
      latestRelease: false,
      published: '2021-01-01T11:21:17',
      slug: 'rel-3-slug',
      title: 'Another Release',
    } as ReleaseSummary,
    {
      id: 'rel-2',
      latestRelease: false,
      published: '2021-05-30T11:21:17',
      slug: 'rel-2-slug',
      title: 'Release 2',
    } as ReleaseSummary,
  ];

  const testSubjects: SubjectWithDownloadFiles[] = [
    {
      id: 'subject-1',
      name: 'Subject 1',
      content: 'Some content here',
      geographicLevels: ['SoYo'],
      timePeriods: {
        from: '2016',
        to: '2019',
      },
      downloadFile: {
        id: 'file-1',
        extension: 'csv',
        fileName: 'file-1.csv',
        name: 'File 1',
        size: '100mb',
        type: 'Data',
      },
    },
    {
      id: 'subject-2',
      name: 'Another Subject',
      content: 'Some content here',
      geographicLevels: ['SoYo'],
      timePeriods: {
        from: '2016',
        to: '2019',
      },
      downloadFile: {
        id: 'file-2',
        extension: 'csv',
        fileName: 'file-2.csv',
        name: 'File 2',
        size: '100mb',
        type: 'Data',
      },
    },
    {
      id: 'subject-3',
      name: 'Subject 3',
      content: 'Some content here',
      geographicLevels: ['SoYo'],
      timePeriods: {
        from: '2016',
        to: '2019',
      },
      downloadFile: {
        id: 'file-3',
        extension: 'csv',
        fileName: 'file-3.csv',
        name: 'File 3',
        size: '100mb',
        type: 'Data',
      },
    },
  ];

  beforeAll(preloadAll);

  test('renders the page correctly with themes and publications', async () => {
    render(<DataCataloguePage themes={testThemes} />);

    expect(screen.getByText('Choose a publication')).toBeInTheDocument();

    expect(screen.getByTestId('wizardStep-1')).toHaveAttribute(
      'aria-current',
      'step',
    );
    expect(screen.getByLabelText('Test publication')).not.toBeVisible();

    userEvent.click(screen.getByRole('button', { name: 'Pupils and schools' }));
    userEvent.click(screen.getByRole('button', { name: 'Admission appeals' }));

    // Check there is only one radio for the publication
    expect(screen.getAllByRole('radio', { hidden: true })).toHaveLength(1);
    expect(screen.getByLabelText('Test publication')).toBeVisible();
  });

  test('can go through all the steps to get to download files', async () => {
    publicationService.listReleases.mockResolvedValue(testReleases);

    render(<DataCataloguePage themes={testThemes} />);

    expect(screen.getByText('Choose a publication')).toBeInTheDocument();
    expect(screen.getByTestId('wizardStep-1')).toHaveAttribute(
      'aria-current',
      'step',
    );
    userEvent.click(screen.getByRole('button', { name: 'Pupils and schools' }));
    userEvent.click(screen.getByRole('button', { name: 'Admission appeals' }));
    userEvent.click(screen.getByRole('radio', { name: 'Test publication' }));
    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    await waitFor(() => {
      expect(screen.getByTestId('wizardStep-1')).not.toHaveAttribute(
        'aria-current',
        'step',
      );
      expect(screen.getByTestId('wizardStep-2')).toHaveAttribute(
        'aria-current',
        'step',
      );
    });

    expect(screen.getByText('Choose a release')).toBeInTheDocument();
    expect(screen.getAllByRole('radio')).toHaveLength(3);
    userEvent.click(
      screen.getByRole('radio', { name: 'Another Release (1 January 2021)' }),
    );
    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    await waitFor(() => {
      expect(screen.getByTestId('wizardStep-2')).not.toHaveAttribute(
        'aria-current',
        'step',
      );
      expect(screen.getByTestId('wizardStep-3')).toHaveAttribute(
        'aria-current',
        'step',
      );
    });

    expect(screen.getByText('Choose files to download')).toBeInTheDocument();
    expect(screen.getAllByRole('checkbox')).toHaveLength(3);
    expect(
      screen.getByRole('button', { name: 'Download selected files' }),
    ).toBeInTheDocument();
  });

  test('direct link to step 2', async () => {
    render(
      <DataCataloguePage
        themes={testThemes}
        selectedPublication={testPublication}
        releases={testReleases}
      />,
    );
    expect(screen.getByTestId('wizardStep-1')).not.toHaveAttribute(
      'aria-current',
      'step',
    );
    expect(screen.getByTestId('wizardStep-2')).toHaveAttribute(
      'aria-current',
      'step',
    );
    expect(screen.getByText('Choose a release')).toBeInTheDocument();
    expect(screen.getAllByRole('radio')).toHaveLength(3);
    expect(
      screen.getByLabelText('Release 1 (30 June 2021)'),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText('Release 2 (30 May 2021)'),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText('Another Release (1 January 2021)'),
    ).toBeInTheDocument();
  });

  test('direct link to step 3', async () => {
    render(
      <DataCataloguePage
        themes={testThemes}
        selectedPublication={testPublication}
        selectedRelease={testReleases[2]}
        releases={testReleases}
        subjects={testSubjects}
      />,
    );
    expect(screen.getByTestId('wizardStep-1')).not.toHaveAttribute(
      'aria-current',
      'step',
    );
    expect(screen.getByTestId('wizardStep-2')).not.toHaveAttribute(
      'aria-current',
      'step',
    );
    expect(screen.getByTestId('wizardStep-3')).toHaveAttribute(
      'aria-current',
      'step',
    );
    expect(screen.getByText('Choose files to download')).toBeInTheDocument();
    expect(screen.getAllByRole('checkbox')).toHaveLength(3);
    expect(
      screen.getByLabelText('Another Subject (csv, 100mb)'),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Subject 1 (csv, 100mb)')).toBeInTheDocument();
    expect(screen.getByLabelText('Subject 3 (csv, 100mb)')).toBeInTheDocument();
  });
});
