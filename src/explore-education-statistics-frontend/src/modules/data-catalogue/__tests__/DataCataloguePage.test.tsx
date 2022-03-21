import _downloadService from '@common/services/downloadService';
import _tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import DataCataloguePage from '@frontend/modules/data-catalogue/DataCataloguePage';
import { PublicationSummary, Theme } from '@common/services/themeService';
import _publicationService, {
  ReleaseSummary,
} from '@common/services/publicationService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import preloadAll from 'jest-next-dynamic';
import React from 'react';

jest.mock('@common/services/downloadService');
jest.mock('@common/services/publicationService');
jest.mock('@common/services/tableBuilderService');

const downloadService = _downloadService as jest.Mocked<
  typeof _downloadService
>;
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('DataCataloguePage', () => {
  const testThemes: Theme[] = [
    {
      id: 'theme-1',
      title: 'Pupils and schools',
      summary: '',
      topics: [
        {
          id: 'topic-1',
          title: 'Admission appeals',
          summary: '',
          publications: [
            {
              id: 'publication-1',
              title: 'Test publication',
              slug: 'test-publication',
              type: 'NationalAndOfficial',
            },
          ],
        },
      ],
    },
  ];

  const testPublication = {
    id: 'publication-1',
    title: 'Test publication',
    slug: 'test-publication',
  } as PublicationSummary;

  const testReleases: ReleaseSummary[] = [
    {
      id: 'release-3',
      latestRelease: true,
      slug: 'release-3-slug',
      title: 'Academic Year 2021/22',
    } as ReleaseSummary,
    {
      id: 'release-2',
      latestRelease: false,
      slug: 'release-2-slug',
      title: 'Academic Year 2020/21',
    } as ReleaseSummary,
    {
      id: 'release-1',
      latestRelease: false,
      slug: 'release-1-slug',
      title: 'Academic Year 2019/20',
    } as ReleaseSummary,
  ];

  const testSubjects: Subject[] = [
    {
      id: 'subject-1',
      name: 'Subject 1',
      content: 'Some content here',
      geographicLevels: ['SoYo'],
      timePeriods: {
        from: '2016',
        to: '2019',
      },
      file: {
        id: 'file-1',
        name: 'Subject 1',
        fileName: 'file-1.csv',
        extension: 'csv',
        size: '10 Mb',
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
      file: {
        id: 'file-2',
        name: 'Another Subject',
        fileName: 'file-2.csv',
        extension: 'csv',
        size: '20 Mb',
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
      file: {
        id: 'file-3',
        name: 'Subject 3',
        fileName: 'file-3.csv',
        extension: 'csv',
        size: '30 Mb',
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

  test('can go through all the steps to download files', async () => {
    publicationService.listReleases.mockResolvedValue(testReleases);
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);

    render(<DataCataloguePage themes={testThemes} />);

    expect(screen.getByTestId('wizardStep-1')).toHaveAttribute(
      'aria-current',
      'step',
    );

    // Step 1

    const step1 = within(screen.getByTestId('wizardStep-1'));

    expect(step1.getByText('Choose a publication')).toBeInTheDocument();

    userEvent.click(step1.getByRole('button', { name: 'Pupils and schools' }));
    userEvent.click(step1.getByRole('button', { name: 'Admission appeals' }));
    userEvent.click(step1.getByRole('radio', { name: 'Test publication' }));
    userEvent.click(step1.getByRole('button', { name: 'Next step' }));

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

    // Step 2

    const step2 = within(screen.getByTestId('wizardStep-2'));

    expect(step2.getByText('Choose a release')).toBeInTheDocument();

    const releaseRadios = step2.getAllByRole('radio');
    expect(releaseRadios).toHaveLength(3);
    expect(releaseRadios[0]).toEqual(
      step2.getByLabelText('Academic Year 2021/22'),
    );
    expect(releaseRadios[1]).toEqual(
      step2.getByLabelText('Academic Year 2020/21'),
    );
    expect(releaseRadios[2]).toEqual(
      step2.getByLabelText('Academic Year 2019/20'),
    );

    userEvent.click(releaseRadios[0]);
    userEvent.click(step2.getByRole('button', { name: 'Next step' }));

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

    // Step 3

    const step3 = within(screen.getByTestId('wizardStep-3'));

    expect(screen.getByText('Choose files to download')).toBeInTheDocument();

    const fileCheckboxes = step3.getAllByRole('checkbox');

    expect(fileCheckboxes).toHaveLength(3);
    expect(fileCheckboxes[0]).toEqual(
      step3.getByLabelText('Another Subject (csv, 20 Mb)'),
    );
    expect(fileCheckboxes[1]).toEqual(
      step3.getByLabelText('Subject 1 (csv, 10 Mb)'),
    );
    expect(fileCheckboxes[2]).toEqual(
      step3.getByLabelText('Subject 3 (csv, 30 Mb)'),
    );

    userEvent.click(fileCheckboxes[1]);
    userEvent.click(fileCheckboxes[2]);

    userEvent.click(
      step3.getByRole('button', { name: 'Download selected files' }),
    );

    await waitFor(() => {
      expect(downloadService.downloadFiles).toHaveBeenCalledWith<
        Parameters<typeof downloadService.downloadFiles>
      >('release-3', ['file-1', 'file-3']);
    });
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

    const step2 = within(screen.getByTestId('wizardStep-2'));

    expect(step2.getByText('Choose a release')).toBeInTheDocument();

    const releaseRadios = step2.getAllByRole('radio');
    expect(releaseRadios).toHaveLength(3);
    expect(releaseRadios[0]).toEqual(
      step2.getByLabelText('Academic Year 2021/22'),
    );
    expect(releaseRadios[1]).toEqual(
      step2.getByLabelText('Academic Year 2020/21'),
    );
    expect(releaseRadios[2]).toEqual(
      step2.getByLabelText('Academic Year 2019/20'),
    );
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
    const step3 = within(screen.getByTestId('wizardStep-3'));

    expect(screen.getByText('Choose files to download')).toBeInTheDocument();

    const fileCheckboxes = step3.getAllByRole('checkbox');

    expect(fileCheckboxes).toHaveLength(3);
    expect(fileCheckboxes[0]).toEqual(
      step3.getByLabelText('Another Subject (csv, 20 Mb)'),
    );
    expect(fileCheckboxes[1]).toEqual(
      step3.getByLabelText('Subject 1 (csv, 10 Mb)'),
    );
    expect(fileCheckboxes[2]).toEqual(
      step3.getByLabelText('Subject 3 (csv, 30 Mb)'),
    );
  });
});
