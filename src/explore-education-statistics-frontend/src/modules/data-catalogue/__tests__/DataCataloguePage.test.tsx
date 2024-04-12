import _downloadService from '@common/services/downloadService';
import _tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import _publicationService, {
  PublicationTreeSummary,
} from '@common/services/publicationService';
import { Paging } from '@common/services/types/pagination';
import render from '@common-test/render';
import DataCataloguePage from '@frontend/modules/data-catalogue/DataCataloguePage';
import { testDataSetFileSummaries } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import _dataSetFileService from '@frontend/services/dataSetFileService';
import { testReleases } from '@frontend/modules/data-catalogue/__data__/testReleases';
import { testThemes } from '@frontend/modules/data-catalogue/__data__/testThemes';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { produce } from 'immer';
import mockRouter from 'next-router-mock';

jest.mock('@frontend/services/dataSetFileService');
jest.mock('@common/services/downloadService');
jest.mock('@common/services/publicationService');
jest.mock('@common/services/tableBuilderService');

let mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

const dataSetService = _dataSetFileService as jest.Mocked<
  typeof _dataSetFileService
>;
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
  const testPublication = {
    id: 'publication-1',
    title: 'Publication title 1',
    slug: 'test-publication',
  } as PublicationTreeSummary;

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
    expect(themeRadios).toHaveLength(2);
    expect(themeRadios[0]).toEqual(
      screen.getByRole('radio', { name: 'Theme title 1' }),
    );

    expect(
      screen.queryByRole('radio', {
        name: 'Publication title 1',
      }),
    ).not.toBeInTheDocument();

    await userEvent.click(screen.getByRole('radio', { name: 'Theme title 1' }));

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
        name: 'Publication title 1',
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

    await userEvent.click(step1.getByRole('radio', { name: 'Theme title 1' }));
    await userEvent.click(
      step1.getByRole('radio', { name: 'Publication title 1' }),
    );
    await userEvent.click(step1.getByRole('button', { name: 'Next step' }));

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

    expect(releaseRadios[0]).toEqual(step2.getByLabelText('Release title 3'));

    expect(
      within(screen.getByTestId('Radio item for Release title 3')).getByText(
        'This is the latest data',
      ),
    );

    expect(releaseRadios[1]).toEqual(step2.getByLabelText('Release title 2'));
    expect(releaseRadios[2]).toEqual(step2.getByLabelText('Release title 1'));

    await userEvent.click(releaseRadios[0]);
    await userEvent.click(step2.getByRole('button', { name: 'Next step' }));

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

    await userEvent.click(fileCheckboxes[1]);
    await userEvent.click(fileCheckboxes[2]);

    await userEvent.click(
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
        title: 'Publication title 1 2',
        slug: 'test-publication-2',
      };
    });
    render(<DataCataloguePage themes={testThemesSuperseded} />);

    // Step 1

    const step1 = within(screen.getByTestId('wizardStep-1'));
    await userEvent.click(step1.getByRole('radio', { name: 'Theme title 1' }));
    await userEvent.click(
      step1.getByRole('radio', { name: 'Publication title 1' }),
    );
    await userEvent.click(step1.getByRole('button', { name: 'Next step' }));

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
        title: 'Publication title 1 2',
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

    await userEvent.click(step1.getByRole('radio', { name: 'Theme title 1' }));
    await userEvent.click(
      step1.getByRole('radio', { name: 'Publication title 1' }),
    );
    await userEvent.click(step1.getByRole('button', { name: 'Next step' }));

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
      name: 'Publication title 1 2',
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
    expect(releaseRadios[0]).toEqual(step2.getByLabelText('Release title 3'));
    expect(releaseRadios[1]).toEqual(step2.getByLabelText('Release title 2'));
    expect(releaseRadios[2]).toEqual(step2.getByLabelText('Release title 1'));
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
      dataSetService.listDataSetFiles.mockResolvedValue({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

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
      dataSetService.listDataSetFiles.mockResolvedValue({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

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
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      expect(
        screen.getByText('Page 1 of 3, showing all available data sets'),
      ).toBeInTheDocument();

      const dataSets = screen.queryAllByTestId(/data-set-file-summary/);
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
      expect(
        pagination.getByRole('link', { name: 'Next page' }),
      ).toHaveAttribute('href', '/data-catalogue?page=2');

      expect(
        screen.queryByText('No data currently published.'),
      ).not.toBeInTheDocument();
    });

    test('renders correctly with no data sets', async () => {
      dataSetService.listDataSetFiles.mockResolvedValue({
        results: [],
        paging: {
          ...testPaging,
          totalPages: 0,
          totalResults: 0,
        },
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

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

    test('renders the initial filters', async () => {
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      render(<DataCataloguePage newDesign />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      const themesSelect = screen.getByLabelText('Theme');
      const themes = within(themesSelect).getAllByRole(
        'option',
      ) as HTMLOptionElement[];
      expect(themes).toHaveLength(3);

      expect(themes[0]).toHaveTextContent('All');
      expect(themes[0]).toHaveValue('all');
      expect(themes[0].selected).toBe(true);

      expect(themes[1]).toHaveTextContent('Theme title 1');
      expect(themes[1]).toHaveValue('theme-1');
      expect(themes[1].selected).toBe(false);

      expect(themes[2]).toHaveTextContent('Theme title 2');
      expect(themes[2]).toHaveValue('theme-2');
      expect(themes[2].selected).toBe(false);

      const publicationsSelect = screen.getByLabelText('Publication');
      expect(publicationsSelect).toBeDisabled();

      const releasesSelect = screen.getByLabelText('Releases');
      const releases = within(releasesSelect).getAllByRole(
        'option',
      ) as HTMLOptionElement[];
      expect(releases).toHaveLength(2);

      expect(releases[0]).toHaveTextContent('Latest releases');
      expect(releases[0]).toHaveValue('latest');
      expect(releases[0].selected).toBe(true);

      expect(releases[1]).toHaveTextContent('All releases');
      expect(releases[1]).toHaveValue('all');
      expect(releases[1].selected).toBe(false);
    });

    describe('filtering by theme', () => {
      beforeEach(async () => {
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        expect(screen.getByLabelText('Publication')).toBeDisabled();
        expect(
          within(screen.getByLabelText('Publication')).getAllByRole('option'),
        ).toHaveLength(1);

        userEvent.selectOptions(screen.getByLabelText('Theme'), ['theme-2']);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });
      });
      test('populates and enables the publications dropdown', async () => {
        const publicationsSelect = screen.getByLabelText('Publication');
        expect(publicationsSelect).not.toBeDisabled();

        const publications = within(publicationsSelect).getAllByRole(
          'option',
        ) as HTMLOptionElement[];
        expect(publications).toHaveLength(3);

        expect(publications[0]).toHaveTextContent('All');
        expect(publications[0]).toHaveValue('all');
        expect(publications[0].selected).toBe(true);

        expect(publications[1]).toHaveTextContent('Publication title 2');
        expect(publications[1]).toHaveValue('publication-2');
        expect(publications[1].selected).toBe(false);

        expect(publications[2]).toHaveTextContent('Publication title 3');
        expect(publications[2]).toHaveValue('publication-3');
        expect(publications[2].selected).toBe(false);
      });

      test('updates the data sets list', async () => {
        expect(
          screen.getByText('Page 1 of 1, filtered by:'),
        ).toBeInTheDocument();

        expect(screen.queryAllByTestId(/data-set-file-summary/)).toHaveLength(
          2,
        );
      });

      test('updates the query params', async () => {
        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: { themeId: 'theme-2' },
        });
      });

      test('shows the remove theme filter and clear filters buttons', async () => {
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Theme Theme title 2',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', { name: 'Clear filters' }),
        ).toBeInTheDocument();
      });
    });

    describe('filtering by publication', () => {
      beforeEach(async () => {
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 1 },
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);
        publicationService.listReleases.mockResolvedValue(testReleases);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        userEvent.selectOptions(screen.getByLabelText('Theme'), ['theme-2']);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        expect(publicationService.listReleases).not.toHaveBeenCalled();

        // Select publication
        userEvent.selectOptions(screen.getByLabelText('Publication'), [
          'publication-2',
        ]);

        await waitFor(() => {
          expect(screen.getByText('1 data set')).toBeInTheDocument();
        });
      });

      test('populates the releases dropdown and selects the latest release', async () => {
        expect(publicationService.listReleases).toHaveBeenCalledWith(
          'publication-slug-2',
        );

        const releasesSelect = screen.getByLabelText('Releases');
        const updatedReleases = within(releasesSelect).getAllByRole(
          'option',
        ) as HTMLOptionElement[];
        expect(updatedReleases).toHaveLength(4);

        expect(updatedReleases[0]).toHaveTextContent('All releases');
        expect(updatedReleases[0]).toHaveValue('all');
        expect(updatedReleases[0].selected).toBe(false);

        // Latest release
        expect(updatedReleases[1]).toHaveTextContent('Release title 3');
        expect(updatedReleases[1]).toHaveValue('release-3');
        expect(updatedReleases[1].selected).toBe(true);

        expect(updatedReleases[2]).toHaveTextContent('Release title 2');
        expect(updatedReleases[2]).toHaveValue('release-2');
        expect(updatedReleases[2].selected).toBe(false);

        expect(updatedReleases[3]).toHaveTextContent('Release title 1');
        expect(updatedReleases[3]).toHaveValue('release-1');
        expect(updatedReleases[3].selected).toBe(false);
      });

      test('updates the data sets list', async () => {
        expect(
          screen.getByText('Page 1 of 1, filtered by:'),
        ).toBeInTheDocument();

        expect(screen.queryAllByTestId(/data-set-file-summary/)).toHaveLength(
          1,
        );
      });

      test('updates the query params', async () => {
        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: {
            publicationId: 'publication-2',
            releaseId: 'release-3',
            themeId: 'theme-2',
          },
        });
      });

      test('shows the remove publication and release filter buttons', async () => {
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Theme Theme title 2',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Publication Publication title 2',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Release Release title 3',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', { name: 'Clear filters' }),
        ).toBeInTheDocument();
      });

      test('shows the release information and download all data sets button', () => {
        const releaseInfo = within(screen.getByTestId('release-info'));

        expect(
          releaseInfo.getByRole('heading', {
            name: 'Publication title 2 - Release title 3 downloads',
          }),
        ).toBeInTheDocument();
        expect(
          releaseInfo.getByText('National statistics'),
        ).toBeInTheDocument();
        expect(
          releaseInfo.getByText('This is the latest data'),
        ).toBeInTheDocument();
        expect(
          releaseInfo.getByRole('button', {
            name: 'Download 1 data set (ZIP)',
          }),
        ).toBeInTheDocument();
        expect(
          releaseInfo.getByRole('link', { name: 'View this release' }),
        ).toHaveAttribute(
          'href',
          '/find-statistics/publication-slug-2/release-slug-3',
        );
      });
    });

    describe('filtering by release', () => {
      test('filtering by another release', async () => {
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 1 },
        });
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[0]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);
        publicationService.listReleases.mockResolvedValue(testReleases);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        const releasesSelect = screen.getByLabelText('Releases');

        // Select theme
        userEvent.selectOptions(screen.getByLabelText('Theme'), ['theme-2']);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        // Select publication
        userEvent.selectOptions(screen.getByLabelText('Publication'), [
          'publication-2',
        ]);

        await waitFor(() => {
          expect(screen.getByText('1 data set')).toBeInTheDocument();
        });

        userEvent.selectOptions(releasesSelect, ['release-1']);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: {
            publicationId: 'publication-2',
            releaseId: 'release-1',
            themeId: 'theme-2',
          },
        });

        expect(
          screen.getByText('Page 1 of 1, filtered by:'),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Theme Theme title 2',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Publication Publication title 2',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Release Release title 1',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', { name: 'Clear filters' }),
        ).toBeInTheDocument();

        const releaseInfo = within(screen.getByTestId('release-info'));

        expect(
          releaseInfo.getByRole('heading', {
            name: 'Publication title 2 - Release title 1 downloads',
          }),
        ).toBeInTheDocument();
        expect(
          releaseInfo.getByText('National statistics'),
        ).toBeInTheDocument();
        expect(
          releaseInfo.getByText('This is not the latest data'),
        ).toBeInTheDocument();
        expect(
          releaseInfo.getByRole('button', {
            name: 'Download all 2 data sets (ZIP)',
          }),
        ).toBeInTheDocument();
        expect(
          releaseInfo.getByRole('link', { name: 'View this release' }),
        ).toHaveAttribute(
          'href',
          '/find-statistics/publication-slug-2/release-slug-1',
        );
      });

      test('switching between showing all and latest releases', async () => {
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        const releasesSelect = screen.getByLabelText('Releases');

        userEvent.selectOptions(releasesSelect, ['all']);

        await waitFor(() => {
          expect(releasesSelect).toHaveValue('all');
        });

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: {
            latestOnly: 'false',
          },
        });

        userEvent.selectOptions(releasesSelect, ['latest']);

        await waitFor(() => {
          expect(releasesSelect).toHaveValue('latest');
        });

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: {
            latestOnly: 'true',
          },
        });
      });
    });

    describe('filtering by search term', () => {
      beforeEach(async () => {
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        dataSetService.listDataSetFiles.mockResolvedValue({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        await userEvent.type(
          screen.getByLabelText('Search data sets'),
          'find me',
        );
        await userEvent.click(screen.getByRole('button', { name: 'Search' }));

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });
      });

      test('updates the data sets list', async () => {
        expect(
          screen.getByText('Page 1 of 1, filtered by:'),
        ).toBeInTheDocument();

        expect(screen.queryAllByTestId(/data-set-file-summary/)).toHaveLength(
          2,
        );
      });

      test('updates the query params', async () => {
        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: { searchTerm: 'find me' },
        });
      });

      test('shows the remove search filter and clear filters buttons', async () => {
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Search find me',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', { name: 'Clear filters' }),
        ).toBeInTheDocument();
      });
    });

    describe('filtering by query params on page load', () => {
      test('filters by theme id', async () => {
        mockRouter.setCurrentUrl('/data-catalogue?themeId=theme-2');

        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        expect(screen.getByLabelText('Theme')).toHaveValue('theme-2');

        const publicationsSelect = screen.getByLabelText('Publication');
        expect(publicationsSelect).not.toBeDisabled();

        const publications = within(publicationsSelect).getAllByRole(
          'option',
        ) as HTMLOptionElement[];
        expect(publications).toHaveLength(3);

        expect(publications[0]).toHaveTextContent('All');
        expect(publications[0]).toHaveValue('all');
        expect(publications[0].selected).toBe(true);

        expect(publications[1]).toHaveTextContent('Publication title 2');
        expect(publications[1]).toHaveValue('publication-2');
        expect(publications[1].selected).toBe(false);

        expect(publications[2]).toHaveTextContent('Publication title 3');
        expect(publications[2]).toHaveValue('publication-3');
        expect(publications[2].selected).toBe(false);

        expect(screen.getByLabelText('Releases')).toHaveValue('latest');

        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Theme Theme title 2',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', { name: 'Clear filters' }),
        ).toBeInTheDocument();
      });

      test('filters by theme id and publication id', async () => {
        mockRouter.setCurrentUrl(
          '/data-catalogue?themeId=theme-2&publicationId=publication-2',
        );

        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);
        publicationService.listReleases.mockResolvedValue(testReleases);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        expect(screen.getByLabelText('Theme')).toHaveValue('theme-2');
        expect(screen.getByLabelText('Publication')).toHaveValue(
          'publication-2',
        );

        const releasesSelect = screen.getByLabelText('Releases');
        const releases = within(releasesSelect).getAllByRole(
          'option',
        ) as HTMLOptionElement[];
        expect(releases).toHaveLength(4);

        await waitFor(() => {
          expect(screen.getByLabelText('Releases')).toHaveValue('release-3');
        });

        expect(releases[0]).toHaveTextContent('All releases');
        expect(releases[0]).toHaveValue('all');
        expect(releases[0].selected).toBe(false);

        expect(releases[1]).toHaveTextContent('Release title 3');
        expect(releases[1]).toHaveValue('release-3');
        expect(releases[1].selected).toBe(true);

        expect(releases[2]).toHaveTextContent('Release title 2');
        expect(releases[2]).toHaveValue('release-2');
        expect(releases[2].selected).toBe(false);

        expect(releases[3]).toHaveTextContent('Release title 1');
        expect(releases[3]).toHaveValue('release-1');
        expect(releases[3].selected).toBe(false);

        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Theme Theme title 2',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Publication Publication title 2',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Release Release title 3',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByRole('button', { name: 'Clear filters' }),
        ).toBeInTheDocument();
      });

      test('filters by theme id, publication id and release id', async () => {
        mockRouter.setCurrentUrl(
          '/data-catalogue?themeId=theme-2&publicationId=publication-2&releaseId=release-1',
        );

        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);
        publicationService.listReleases.mockResolvedValue(testReleases);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        expect(screen.getByLabelText('Theme')).toHaveValue('theme-2');
        expect(screen.getByLabelText('Publication')).toHaveValue(
          'publication-2',
        );

        const releasesSelect = screen.getByLabelText('Releases');
        const releases = within(releasesSelect).getAllByRole(
          'option',
        ) as HTMLOptionElement[];
        expect(releases).toHaveLength(4);

        await waitFor(() => {
          expect(screen.getByLabelText('Releases')).toHaveValue('release-1');
        });

        expect(releases[0]).toHaveTextContent('All releases');
        expect(releases[0]).toHaveValue('all');
        expect(releases[0].selected).toBe(false);

        expect(releases[1]).toHaveTextContent('Release title 3');
        expect(releases[1]).toHaveValue('release-3');
        expect(releases[1].selected).toBe(false);

        expect(releases[2]).toHaveTextContent('Release title 2');
        expect(releases[2]).toHaveValue('release-2');
        expect(releases[2].selected).toBe(false);

        expect(releases[3]).toHaveTextContent('Release title 1');
        expect(releases[3]).toHaveValue('release-1');
        expect(releases[3].selected).toBe(true);

        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Theme Theme title 2',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Publication Publication title 2',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Release Release title 1',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByRole('button', { name: 'Clear filters' }),
        ).toBeInTheDocument();
      });

      test('filters by publication id', async () => {
        mockRouter.setCurrentUrl('/data-catalogue?publicationId=publication-2');

        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);
        publicationService.listReleases.mockResolvedValue(testReleases);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: { publicationId: 'publication-2', themeId: 'theme-2' },
        });

        expect(screen.getByLabelText('Theme')).toHaveValue('theme-2');
        expect(screen.getByLabelText('Publication')).toHaveValue(
          'publication-2',
        );

        const releasesSelect = screen.getByLabelText('Releases');
        const releases = within(releasesSelect).getAllByRole(
          'option',
        ) as HTMLOptionElement[];
        expect(releases).toHaveLength(4);

        await waitFor(() => {
          expect(screen.getByLabelText('Releases')).toHaveValue('release-3');
        });

        expect(releases[0]).toHaveTextContent('All releases');
        expect(releases[0]).toHaveValue('all');
        expect(releases[0].selected).toBe(false);

        expect(releases[1]).toHaveTextContent('Release title 3');
        expect(releases[1]).toHaveValue('release-3');
        expect(releases[1].selected).toBe(true);

        expect(releases[2]).toHaveTextContent('Release title 2');
        expect(releases[2]).toHaveValue('release-2');
        expect(releases[2].selected).toBe(false);

        expect(releases[3]).toHaveTextContent('Release title 1');
        expect(releases[3]).toHaveValue('release-1');
        expect(releases[3].selected).toBe(false);

        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Theme Theme title 2',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Publication Publication title 2',
          }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Release Release title 3',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByRole('button', { name: 'Clear filters' }),
        ).toBeInTheDocument();
      });

      test('filters by search term', async () => {
        mockRouter.setCurrentUrl('/data-catalogue?searchTerm=find+me');

        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        expect(screen.getByLabelText('Theme')).toHaveValue('all');
        expect(screen.getByLabelText('Publication')).toHaveValue('all');
        expect(screen.getByLabelText('Releases')).toHaveValue('latest');

        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Search find me',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByRole('button', { name: 'Clear filters' }),
        ).toBeInTheDocument();
      });

      test('does not filter by release id only', async () => {
        mockRouter.setCurrentUrl('/data-catalogue?releaseId=release-1');

        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        expect(screen.getByLabelText('Theme')).toHaveValue('all');
        expect(screen.getByLabelText('Publication')).toHaveValue('all');
        expect(screen.getByLabelText('Releases')).toHaveValue('latest');

        expect(
          screen.queryByRole('button', { name: 'Clear filters' }),
        ).not.toBeInTheDocument();
      });
    });

    describe('removing filters', () => {
      test('removing theme filter', async () => {
        mockRouter.setCurrentUrl('/data-catalogue?themeId=theme-2');

        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        const themesSelect = screen.getByLabelText('Theme');
        expect(themesSelect).toHaveValue('theme-2');

        const publicationsSelect = screen.getByLabelText('Publication');
        expect(publicationsSelect).not.toBeDisabled();
        expect(publicationsSelect).toHaveValue('all');
        expect(within(publicationsSelect).getAllByRole('option')).toHaveLength(
          3,
        );

        userEvent.click(
          screen.getByRole('button', {
            name: 'Remove filter: Theme Theme title 2',
          }),
        );

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: {},
        });

        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Theme Theme 2',
          }),
        ).not.toBeInTheDocument();

        expect(themesSelect).toHaveValue('all');
        expect(publicationsSelect).toHaveValue('all');
        expect(publicationsSelect).toBeDisabled();
      });

      test('removing theme filter when also filtering by publication and release', async () => {
        mockRouter.setCurrentUrl(
          '/data-catalogue?themeId=theme-2&publicationId=publication-2&releaseId=release-1',
        );

        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        dataSetService.listDataSetFiles.mockResolvedValue({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);
        publicationService.listReleases.mockResolvedValue(testReleases);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        const themesSelect = screen.getByLabelText('Theme');
        expect(themesSelect).toHaveValue('theme-2');

        const publicationsSelect = screen.getByLabelText('Publication');
        expect(publicationsSelect).not.toBeDisabled();
        expect(publicationsSelect).toHaveValue('publication-2');
        expect(within(publicationsSelect).getAllByRole('option')).toHaveLength(
          3,
        );

        const releasesSelect = screen.getByLabelText('Releases');
        await waitFor(() => {
          expect(releasesSelect).toHaveValue('release-1');
        });

        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Publication Publication title 2',
          }),
        ).toBeInTheDocument();

        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Release Release title 1',
          }),
        ).toBeInTheDocument();

        userEvent.click(
          screen.getByRole('button', {
            name: 'Remove filter: Theme Theme title 2',
          }),
        );

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: {},
        });

        await waitFor(() =>
          expect(
            screen.queryByText(
              'Publication title 2 - Release title 1 downloads',
            ),
          ).not.toBeInTheDocument(),
        );
        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Theme Theme title 2',
          }),
        ).not.toBeInTheDocument();
        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Publication Publication title 2',
          }),
        ).not.toBeInTheDocument();
        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Release Release title 1',
          }),
        ).not.toBeInTheDocument();

        expect(themesSelect).toHaveValue('all');
        expect(publicationsSelect).toHaveValue('all');
        expect(publicationsSelect).toBeDisabled();
        expect(releasesSelect).toHaveValue('latest');
      });

      test('removing publication filter when also filtering by release', async () => {
        mockRouter.setCurrentUrl(
          '/data-catalogue?themeId=theme-2&publicationId=publication-2&releaseId=release-1',
        );

        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        dataSetService.listDataSetFiles.mockResolvedValue({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);
        publicationService.listReleases.mockResolvedValue(testReleases);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        const publicationsSelect = screen.getByLabelText('Publication');
        expect(publicationsSelect).toHaveValue('publication-2');

        const releasesSelect = screen.getByLabelText('Releases');
        await waitFor(() => {
          expect(releasesSelect).toHaveValue('release-1');
        });

        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Release Release title 1',
          }),
        ).toBeInTheDocument();

        await userEvent.click(
          screen.getByRole('button', {
            name: 'Remove filter: Publication Publication title 2',
          }),
        );

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: { themeId: 'theme-2' },
        });

        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Publication Publication title 2',
          }),
        ).not.toBeInTheDocument();
        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Release Release title 1',
          }),
        ).not.toBeInTheDocument();

        expect(publicationsSelect).toHaveValue('all');
        expect(releasesSelect).toHaveValue('latest');
      });

      test('removing release filter', async () => {
        mockRouter.setCurrentUrl(
          '/data-catalogue?themeId=theme-2&publicationId=publication-2&releaseId=release-1',
        );

        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        dataSetService.listDataSetFiles.mockResolvedValue({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);
        publicationService.listReleases.mockResolvedValue(testReleases);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        const releasesSelect = screen.getByLabelText('Releases');
        await waitFor(() => {
          expect(releasesSelect).toHaveValue('release-1');
        });

        await userEvent.click(
          screen.getByRole('button', {
            name: 'Remove filter: Release Release title 1',
          }),
        );

        expect(await screen.findByText('30 data sets')).toBeInTheDocument();

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: { themeId: 'theme-2', publicationId: 'publication-2' },
        });

        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Release Release title 1',
          }),
        ).not.toBeInTheDocument();

        expect(releasesSelect).toHaveValue('all');
      });

      test('removing search filter', async () => {
        mockRouter.setCurrentUrl('/data-catalogue?searchTerm=find+me');

        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        userEvent.click(
          screen.getByRole('button', {
            name: 'Remove filter: Search find me',
          }),
        );

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: {},
        });

        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Search find me',
          }),
        ).not.toBeInTheDocument();
      });

      test('removing all filters', async () => {
        mockRouter.setCurrentUrl(
          '/data-catalogue?themeId=theme-2&publicationId=publication-2&releaseId=release-1',
        );

        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);
        publicationService.listReleases.mockResolvedValue(testReleases);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Publication Publication title 2',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Theme Theme title 2',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByRole('button', {
            name: 'Remove filter: Release Release title 1',
          }),
        ).toBeInTheDocument();

        await userEvent.click(
          screen.getByRole('button', {
            name: 'Clear filters',
          }),
        );

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: {},
        });

        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Publication Publication title 2',
          }),
        ).toBeInTheDocument();

        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Theme Theme title 2',
          }),
        ).toBeInTheDocument();

        expect(
          screen.queryByRole('button', {
            name: 'Remove filter: Release Release title 1',
          }),
        ).toBeInTheDocument();

        expect(
          screen.queryByRole('button', {
            name: 'Clear filters',
          }),
        ).toBeInTheDocument();
      });
    });

    describe('sorting', () => {
      test('changing sort order', async () => {
        dataSetService.listDataSetFiles.mockResolvedValue({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        userEvent.click(screen.getByLabelText('A to Z'));

        await waitFor(() => {
          expect(mockRouter).toMatchObject({
            pathname: '/data-catalogue',
            query: { sortBy: 'title' },
          });
        });
      });

      test('sorts by relevance when have a search filter', async () => {
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
          paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        await userEvent.type(
          screen.getByLabelText('Search data sets'),
          'Find me',
        );
        await await userEvent.click(
          screen.getByRole('button', { name: 'Search' }),
        );

        await waitFor(() => {
          expect(screen.getByText('2 data sets')).toBeInTheDocument();
        });

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: { sortBy: 'relevance' },
        });
      });

      test('reverts the sorting to `newest` when the search filter is removed', async () => {
        mockRouter.setCurrentUrl(
          '/data-catalogue?searchTerm=Find+me&orderBy=relevance',
        );
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: [testDataSetFileSummaries[1]],
          paging: { ...testPaging, totalPages: 1, totalResults: 1 },
        });
        dataSetService.listDataSetFiles.mockResolvedValueOnce({
          results: testDataSetFileSummaries,
          paging: testPaging,
        });
        publicationService.getPublicationTree.mockResolvedValue(testThemes);

        render(<DataCataloguePage newDesign />);

        await waitFor(() => {
          expect(screen.getByText('1 data set')).toBeInTheDocument();
        });

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: { orderBy: 'relevance' },
        });

        await userEvent.click(
          screen.getByRole('button', {
            name: 'Remove filter: Search Find me',
          }),
        );

        await waitFor(() => {
          expect(screen.getByText('30 data sets')).toBeInTheDocument();
        });

        expect(mockRouter).toMatchObject({
          pathname: '/data-catalogue',
          query: { sortBy: 'newest' },
        });
      });
    });

    test('renders the mobile filters', async () => {
      mockIsMedia = true;
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      render(<DataCataloguePage newDesign />);

      expect(await screen.findByText('30 data sets')).toBeInTheDocument();

      await userEvent.click(
        screen.getByRole('button', { name: 'Filter results' }),
      );

      expect(await screen.findByText('Back to results')).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      expect(modal.getByLabelText('Theme')).toBeInTheDocument();
      expect(modal.getByLabelText('Publication')).toBeInTheDocument();
      expect(modal.getByLabelText('Releases')).toBeInTheDocument();
      expect(
        modal.getByRole('button', { name: 'Back to results' }),
      ).toBeInTheDocument();

      mockIsMedia = false;
    });
  });
});
