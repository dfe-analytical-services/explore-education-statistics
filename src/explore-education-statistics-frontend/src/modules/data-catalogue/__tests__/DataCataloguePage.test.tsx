import _downloadService from '@common/services/downloadService';
import _tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import _publicationService, {
  ReleaseSummary,
  PublicationTreeSummary,
  Theme,
} from '@common/services/publicationService';
import DataCataloguePage from '@frontend/modules/data-catalogue/DataCataloguePage';
import { testDataSetSummaries } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import _dataSetService from '@frontend/services/dataSetService';
import { screen, waitFor, within } from '@testing-library/react';
import render from '@common-test/render';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { produce } from 'immer';
import { Paging } from '@common/services/types/pagination';
import mockRouter from 'next-router-mock';

jest.mock('@frontend/services/dataSetService');
jest.mock('@common/services/downloadService');
jest.mock('@common/services/publicationService');
jest.mock('@common/services/tableBuilderService');

const mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

const dataSetService = _dataSetService as jest.Mocked<typeof _dataSetService>;
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
              isSuperseded: false,
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
  } as PublicationTreeSummary;

  const testReleases: ReleaseSummary[] = [
    {
      id: 'release-3',
      latestRelease: true,
      slug: 'release-3-slug',
      title: 'Academic year 2021/22',
    } as ReleaseSummary,
    {
      id: 'release-2',
      latestRelease: false,
      slug: 'release-2-slug',
      title: 'Academic year 2020/21',
    } as ReleaseSummary,
    {
      id: 'release-1',
      latestRelease: false,
      slug: 'release-1-slug',
      title: 'Academic year 2019/20',
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
      filters: ['Filter 1'],
      indicators: ['Indicator 1'],
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
      filters: ['Filter 1'],
      indicators: ['Indicator 1'],
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
      filters: ['Filter 1'],
      indicators: ['Indicator 1'],
    },
  ];

  test('renders the page correctly with themes and publications', async () => {
    render(<DataCataloguePage themes={testThemes} />);

    expect(screen.getByText('Choose a publication')).toBeInTheDocument();

    expect(screen.getByTestId('wizardStep-1')).toHaveAttribute(
      'aria-current',
      'step',
    );

    const themeRadios = within(
      screen.getByRole('group', { name: 'Select a theme' }),
    ).getAllByRole('radio');
    expect(themeRadios).toHaveLength(1);
    expect(themeRadios[0]).toEqual(
      screen.getByRole('radio', { name: 'Pupils and schools' }),
    );

    expect(
      screen.queryByRole('radio', {
        name: 'Test publication',
      }),
    ).not.toBeInTheDocument();

    userEvent.click(screen.getByRole('radio', { name: 'Pupils and schools' }));

    // Check there is only one radio for the publication
    await waitFor(() => {
      expect(
        screen.queryByText('Search or select a theme to view publications'),
      ).not.toBeInTheDocument();
    });

    const publicationRadios = within(
      screen.getByRole('group', {
        name: /Select a publication/,
      }),
    ).queryAllByRole('radio');
    expect(publicationRadios).toHaveLength(1);
    expect(publicationRadios[0]).toEqual(
      screen.getByRole('radio', {
        name: 'Test publication',
      }),
    );
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

    userEvent.click(step1.getByRole('radio', { name: 'Pupils and schools' }));
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
      step2.getByLabelText('Academic year 2021/22'),
    );

    expect(
      within(
        screen.getByTestId('Radio item for Academic year 2021/22'),
      ).getByText('This is the latest data'),
    );

    expect(releaseRadios[1]).toEqual(
      step2.getByLabelText('Academic year 2020/21'),
    );
    expect(releaseRadios[2]).toEqual(
      step2.getByLabelText('Academic year 2019/20'),
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
      step3.getByLabelText('Subject 1 (csv, 10 Mb)'),
    );
    expect(fileCheckboxes[1]).toEqual(
      step3.getByLabelText('Another Subject (csv, 20 Mb)'),
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
      >('release-3', ['file-2', 'file-3']);
    });
  });

  test('does not render Latest data tag if isSuperseded is true', async () => {
    publicationService.listReleases.mockResolvedValue(testReleases);

    const testThemesSuperseded = produce(testThemes, draft => {
      draft[0].topics[0].publications[0].isSuperseded = true;
      draft[0].topics[0].publications[0].supersededBy = {
        id: 'test-publication-2',
        title: 'Test publication 2',
        slug: 'test-publication-2',
      };
    });
    render(<DataCataloguePage themes={testThemesSuperseded} />);

    // Step 1

    const step1 = within(screen.getByTestId('wizardStep-1'));
    userEvent.click(step1.getByRole('radio', { name: 'Pupils and schools' }));
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

    expect(
      screen.queryByText('This is the latest data'),
    ).not.toBeInTheDocument();
  });

  test('renders superseded warning text on step 1 if isSuperseded is true', async () => {
    publicationService.listReleases.mockResolvedValue(testReleases);
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);

    const testThemesSuperseded = produce(testThemes, draft => {
      draft[0].topics[0].publications[0].isSuperseded = true;
      draft[0].topics[0].publications[0].supersededBy = {
        id: 'test-publication-2',
        title: 'Test publication 2',
        slug: 'test-publication-2',
      };
    });

    render(<DataCataloguePage themes={testThemesSuperseded} />);

    expect(screen.getByTestId('wizardStep-1')).toHaveAttribute(
      'aria-current',
      'step',
    );

    // Step 1

    const step1 = within(screen.getByTestId('wizardStep-1'));

    userEvent.click(step1.getByRole('radio', { name: 'Pupils and schools' }));
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

    const supersededWarning = screen.getByTestId('superseded-warning');

    expect(supersededWarning).toBeInTheDocument();

    const supersededWarningLink = within(supersededWarning).getByRole('link', {
      name: 'Test publication 2',
    });

    expect(supersededWarningLink).toHaveAttribute(
      'href',
      '/data-catalogue?publicationSlug=test-publication-2',
    );
  });

  test('does not render superseded warning text on step 1 if isSuperseded is false', async () => {
    publicationService.listReleases.mockResolvedValue(testReleases);
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);

    render(<DataCataloguePage themes={testThemes} />);

    expect(screen.getByTestId('wizardStep-1')).toHaveAttribute(
      'aria-current',
      'step',
    );

    // Step 1
    expect(screen.queryByTestId('superseded-warning')).not.toBeInTheDocument();
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
      step2.getByLabelText('Academic year 2021/22'),
    );
    expect(releaseRadios[1]).toEqual(
      step2.getByLabelText('Academic year 2020/21'),
    );
    expect(releaseRadios[2]).toEqual(
      step2.getByLabelText('Academic year 2019/20'),
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
      step3.getByLabelText('Subject 1 (csv, 10 Mb)'),
    );
    expect(fileCheckboxes[1]).toEqual(
      step3.getByLabelText('Another Subject (csv, 20 Mb)'),
    );
    expect(fileCheckboxes[2]).toEqual(
      step3.getByLabelText('Subject 3 (csv, 30 Mb)'),
    );
  });

  // TO DO EES-4781 remove tests for old version
  describe('new version', () => {
    const testPaging: Paging = {
      page: 1,
      pageSize: 10,
      totalResults: 30,
      totalPages: 3,
    };

    beforeEach(() => {
      mockRouter.setCurrentUrl('/data-catalogue');
    });

    test('renders related information links', async () => {
      dataSetService.listDataSets.mockResolvedValue({
        results: testDataSetSummaries,
        paging: testPaging,
      });

      render(<DataCataloguePage newDesign />);

      const relatedInformationNav = screen.getByRole('navigation', {
        name: 'Related information',
      });

      const relatedInformationLinks = within(
        relatedInformationNav,
      ).getAllByRole('link');

      expect(relatedInformationLinks).toHaveLength(2);
      expect(relatedInformationLinks[0]).toHaveTextContent(
        'Find statistics and data',
      );
      expect(relatedInformationLinks[1]).toHaveTextContent('Glossary');
    });

    test('renders correctly with data sets', async () => {
      dataSetService.listDataSets.mockResolvedValue({
        results: testDataSetSummaries,
        paging: testPaging,
      });

      render(<DataCataloguePage newDesign />);

      expect(
        screen.getByRole('heading', { name: 'Data catalogue' }),
      ).toBeInTheDocument();

      expect(
        screen.getByText(
          'Find and download data sets with associated guidance files.',
        ),
      ).toBeInTheDocument();

      await waitFor(() => {
        expect(screen.getByText('30 results')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('heading', { name: '30 results' }),
      ).toBeInTheDocument();

      expect(
        screen.getByText('Page 1 of 3, showing all available data sets'),
      ).toBeInTheDocument();

      const dataSets = screen.queryAllByTestId(/data-set-summary/);
      expect(dataSets).toHaveLength(3);

      expect(
        within(dataSets[0]).getByRole('heading', { name: 'Data set 1' }),
      ).toBeInTheDocument();
      expect(
        within(dataSets[1]).getByRole('heading', { name: 'Data set 2' }),
      ).toBeInTheDocument();
      expect(
        within(dataSets[2]).getByRole('heading', { name: 'Data set 3' }),
      ).toBeInTheDocument();

      const pagination = within(
        screen.getByRole('navigation', { name: 'Pagination' }),
      );
      expect(pagination.getByRole('link', { name: 'Page 1' })).toHaveAttribute(
        'href',
        '/data-catalogue?page=1',
      );
      expect(pagination.getByRole('link', { name: 'Page 2' })).toHaveAttribute(
        'href',
        '/data-catalogue?page=2',
      );
      expect(pagination.getByRole('link', { name: 'Page 3' })).toHaveAttribute(
        'href',
        '/data-catalogue?page=3',
      );
      expect(pagination.getByRole('link', { name: 'Next' })).toHaveAttribute(
        'href',
        '/data-catalogue?page=2',
      );

      expect(
        screen.queryByText('No data currently published.'),
      ).not.toBeInTheDocument();
    });

    test('renders correctly with no data sets', async () => {
      dataSetService.listDataSets.mockResolvedValue({
        results: [],
        paging: {
          ...testPaging,
          totalPages: 0,
          totalResults: 0,
        },
      });
      render(<DataCataloguePage newDesign />);

      await waitFor(() => {
        expect(
          screen.getByText('No data currently published.'),
        ).toBeInTheDocument();
      });

      expect(screen.queryByTestId('dataSetsList')).not.toBeInTheDocument();

      expect(
        screen.queryByRole('navigation', { name: 'Pagination navigation' }),
      ).not.toBeInTheDocument();
    });
  });
});
