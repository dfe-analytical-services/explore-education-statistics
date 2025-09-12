import _publicationService from '@common/services/publicationService';
import { Paging } from '@common/services/types/pagination';
import render from '@common-test/render';
import DataCataloguePage from '@frontend/modules/data-catalogue/DataCataloguePage';
import { testDataSetFileSummaries } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import _dataSetFileService from '@frontend/services/dataSetFileService';
import { testReleases } from '@frontend/modules/data-catalogue/__data__/testReleases';
import { testThemes } from '@frontend/modules/data-catalogue/__data__/testThemes';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import mockRouter from 'next-router-mock';

jest.mock('@frontend/services/dataSetFileService');
jest.mock('@common/services/publicationService');

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
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('DataCataloguePage', () => {
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

    render(<DataCataloguePage />);

    const relatedInformationNav = screen.getByRole('navigation', {
      name: 'Related information',
    });

    const relatedInformationLinks = within(relatedInformationNav).getAllByRole(
      'link',
    );

    expect(relatedInformationLinks).toHaveLength(3);
    expect(relatedInformationLinks[0]).toHaveTextContent(
      'Find statistics and data',
    );
    expect(relatedInformationLinks[1]).toHaveTextContent('Methodology');
    expect(relatedInformationLinks[2]).toHaveTextContent('Glossary');
  });

  test('renders correctly with data sets', async () => {
    dataSetService.listDataSetFiles.mockResolvedValue({
      results: testDataSetFileSummaries,
      paging: testPaging,
    });
    publicationService.getPublicationTree.mockResolvedValue(testThemes);

    render(<DataCataloguePage />);

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
      within(dataSets[0]).getByRole('heading', {
        name: 'Publication 1 Data set 1',
      }),
    ).toBeInTheDocument();
    expect(
      within(dataSets[1]).getByRole('heading', {
        name: 'Publication 1 Data set 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(dataSets[2]).getByRole('heading', {
        name: 'Publication 2 Data set 3',
      }),
    ).toBeInTheDocument();

    const pagination = within(
      screen.getByRole('navigation', { name: 'Pagination' }),
    );
    expect(pagination.getByRole('link', { name: 'Page 1' })).toHaveAttribute(
      'href',
      '?page=1',
    );
    expect(pagination.getByRole('link', { name: 'Page 2' })).toHaveAttribute(
      'href',
      '?page=2',
    );
    expect(pagination.getByRole('link', { name: 'Page 3' })).toHaveAttribute(
      'href',
      '?page=3',
    );
    expect(pagination.getByRole('link', { name: 'Next page' })).toHaveAttribute(
      'href',
      '?page=2',
    );

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

    render(<DataCataloguePage />);

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

    render(<DataCataloguePage />);

    await waitFor(() => {
      expect(screen.getByText('30 data sets')).toBeInTheDocument();
    });

    const themesSelect = screen.getByLabelText('Filter by Theme');
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

    const publicationsSelect = screen.getByLabelText('Filter by Publication');
    expect(publicationsSelect).toBeDisabled();

    const releasesSelect = screen.getByLabelText('Filter by Releases');
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
    test('populates and enables the publications dropdown', async () => {
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Filter by Publication')).toBeDisabled();
      expect(
        within(screen.getByLabelText('Filter by Publication')).getAllByRole(
          'option',
        ),
      ).toHaveLength(1);

      await user.selectOptions(screen.getByLabelText('Filter by Theme'), [
        'theme-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      const publicationsSelect = screen.getByLabelText('Filter by Publication');
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
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.selectOptions(screen.getByLabelText('Filter by Theme'), [
        'theme-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();

      expect(screen.queryAllByTestId(/data-set-file-summary/)).toHaveLength(2);
    });

    test('updates the query params', async () => {
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.selectOptions(screen.getByLabelText('Filter by Theme'), [
        'theme-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(mockRouter).toMatchObject({
        pathname: '/data-catalogue',
        query: { themeId: 'theme-2' },
      });
    });

    test('shows the remove theme filter and reset filters buttons', async () => {
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.selectOptions(screen.getByLabelText('Filter by Theme'), [
        'theme-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Theme: Theme title 2',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Reset filters' }),
      ).toBeInTheDocument();
    });
  });

  describe('filtering by publication', () => {
    test('populates the releases dropdown and selects the latest release', async () => {
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.selectOptions(screen.getByLabelText('Filter by Theme'), [
        'theme-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(publicationService.listReleases).not.toHaveBeenCalled();

      // Select publication
      await user.selectOptions(screen.getByLabelText('Filter by Publication'), [
        'publication-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('1 data set')).toBeInTheDocument();
      });

      expect(publicationService.listReleases).toHaveBeenCalledWith(
        'publication-slug-2',
      );

      const releasesSelect = screen.getByLabelText('Filter by Releases');
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.selectOptions(screen.getByLabelText('Filter by Theme'), [
        'theme-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      // Select publication
      await user.selectOptions(screen.getByLabelText('Filter by Publication'), [
        'publication-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('1 data set')).toBeInTheDocument();
      });

      expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();

      expect(screen.queryAllByTestId(/data-set-file-summary/)).toHaveLength(1);
    });

    test('updates the query params', async () => {
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.selectOptions(screen.getByLabelText('Filter by Theme'), [
        'theme-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      // Select publication
      await user.selectOptions(screen.getByLabelText('Filter by Publication'), [
        'publication-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('1 data set')).toBeInTheDocument();
      });

      expect(mockRouter).toMatchObject({
        pathname: '/data-catalogue',
        query: {
          publicationId: 'publication-2',
          releaseVersionId: 'release-3',
          themeId: 'theme-2',
        },
      });
    });

    test('shows the remove publication and release filter buttons', async () => {
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.selectOptions(screen.getByLabelText('Filter by Theme'), [
        'theme-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      // Select publication
      await user.selectOptions(screen.getByLabelText('Filter by Publication'), [
        'publication-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('1 data set')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Theme: Theme title 2',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Publication: Publication title 2',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Release: Release title 3',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Reset filters' }),
      ).toBeInTheDocument();
    });

    test('shows the release information and download all data sets button', async () => {
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.selectOptions(screen.getByLabelText('Filter by Theme'), [
        'theme-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      // Select publication
      await user.selectOptions(screen.getByLabelText('Filter by Publication'), [
        'publication-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('1 data set')).toBeInTheDocument();
      });

      const releaseInfo = within(screen.getByTestId('release-info'));

      expect(
        releaseInfo.getByRole('heading', {
          name: 'Publication title 2 - Release title 3 downloads',
        }),
      ).toBeInTheDocument();
      expect(
        releaseInfo.getByText('Accredited official statistics'),
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      const releasesSelect = screen.getByLabelText('Filter by Releases');

      // Select theme
      await user.selectOptions(screen.getByLabelText('Filter by Theme'), [
        'theme-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      // Select publication
      await user.selectOptions(screen.getByLabelText('Filter by Publication'), [
        'publication-2',
      ]);

      await waitFor(() => {
        expect(screen.getByText('1 data set')).toBeInTheDocument();
      });

      await user.selectOptions(releasesSelect, ['release-1']);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(mockRouter).toMatchObject({
        pathname: '/data-catalogue',
        query: {
          publicationId: 'publication-2',
          releaseVersionId: 'release-1',
          themeId: 'theme-2',
        },
      });

      expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Theme: Theme title 2',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Publication: Publication title 2',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Release: Release title 1',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Reset filters' }),
      ).toBeInTheDocument();

      await waitFor(() =>
        expect(screen.getByTestId('release-info')).toBeInTheDocument(),
      );

      const releaseInfo = within(screen.getByTestId('release-info'));

      expect(
        releaseInfo.getByRole('heading', {
          name: 'Publication title 2 - Release title 1 downloads',
        }),
      ).toBeInTheDocument();
      expect(
        releaseInfo.getByText('Accredited official statistics'),
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      const releasesSelect = screen.getByLabelText('Filter by Releases');

      await user.selectOptions(releasesSelect, ['all']);

      await waitFor(() => {
        expect(releasesSelect).toHaveValue('all');
      });

      expect(mockRouter).toMatchObject({
        pathname: '/data-catalogue',
        query: {
          latestOnly: 'false',
        },
      });

      await user.selectOptions(releasesSelect, ['latest']);

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
    test('updates the data sets list', async () => {
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      dataSetService.listDataSetFiles.mockResolvedValue({
        results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.type(screen.getByLabelText('Search data sets'), 'find me');
      await user.click(screen.getByRole('button', { name: 'Search' }));

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();

      expect(screen.queryAllByTestId(/data-set-file-summary/)).toHaveLength(2);
    });

    test('updates the query params', async () => {
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      dataSetService.listDataSetFiles.mockResolvedValue({
        results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.type(screen.getByLabelText('Search data sets'), 'find me');
      await user.click(screen.getByRole('button', { name: 'Search' }));

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(mockRouter).toMatchObject({
        pathname: '/data-catalogue',
        query: { searchTerm: 'find me' },
      });
    });

    test('shows the remove search filter and reset filters buttons', async () => {
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      dataSetService.listDataSetFiles.mockResolvedValue({
        results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.type(screen.getByLabelText('Search data sets'), 'find me');
      await user.click(screen.getByRole('button', { name: 'Search' }));

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Search: find me',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Reset filters' }),
      ).toBeInTheDocument();
    });

    test('Does not show "Download all X data sets (ZIP)" button', async () => {
      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      dataSetService.listDataSetFiles.mockResolvedValue({
        results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.type(screen.getByLabelText('Search data sets'), 'find me');
      await user.click(screen.getByRole('button', { name: 'Search' }));

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(
        screen.queryByRole('button', {
          name: 'Download all 2 data sets (ZIP)',
        }),
      ).not.toBeInTheDocument();
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

      render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Filter by Theme')).toHaveValue('theme-2');

      const publicationsSelect = screen.getByLabelText('Filter by Publication');
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

      expect(screen.getByLabelText('Filter by Releases')).toHaveValue('latest');

      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Theme: Theme title 2',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Reset filters' }),
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

      render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Filter by Theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Filter by Publication')).toHaveValue(
        'publication-2',
      );

      const releasesSelect = screen.getByLabelText('Filter by Releases');
      const releases = within(releasesSelect).getAllByRole(
        'option',
      ) as HTMLOptionElement[];
      expect(releases).toHaveLength(4);

      await waitFor(() => {
        expect(screen.getByLabelText('Filter by Releases')).toHaveValue(
          'release-3',
        );
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
          name: 'Remove filter: Theme: Theme title 2',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Publication: Publication title 2',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Release: Release title 3',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Reset filters' }),
      ).toBeInTheDocument();
    });

    test('filters by theme id, publication id and release id', async () => {
      mockRouter.setCurrentUrl(
        '/data-catalogue?themeId=theme-2&publicationId=publication-2&releaseVersionId=release-1',
      );

      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);
      publicationService.listReleases.mockResolvedValue(testReleases);

      render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Filter by Theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Filter by Publication')).toHaveValue(
        'publication-2',
      );

      const releasesSelect = screen.getByLabelText('Filter by Releases');
      const releases = within(releasesSelect).getAllByRole(
        'option',
      ) as HTMLOptionElement[];
      expect(releases).toHaveLength(4);

      await waitFor(() => {
        expect(screen.getByLabelText('Filter by Releases')).toHaveValue(
          'release-1',
        );
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
          name: 'Remove filter: Theme: Theme title 2',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Publication: Publication title 2',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Release: Release title 1',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Reset filters' }),
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

      render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(mockRouter).toMatchObject({
        pathname: '/data-catalogue',
        query: { publicationId: 'publication-2', themeId: 'theme-2' },
      });

      expect(screen.getByLabelText('Filter by Theme')).toHaveValue('theme-2');
      expect(screen.getByLabelText('Filter by Publication')).toHaveValue(
        'publication-2',
      );

      const releasesSelect = screen.getByLabelText('Filter by Releases');
      const releases = within(releasesSelect).getAllByRole(
        'option',
      ) as HTMLOptionElement[];
      expect(releases).toHaveLength(4);

      await waitFor(() => {
        expect(screen.getByLabelText('Filter by Releases')).toHaveValue(
          'release-3',
        );
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
          name: 'Remove filter: Theme: Theme title 2',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Publication: Publication title 2',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Release: Release title 3',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Reset filters' }),
      ).toBeInTheDocument();
    });

    test('filters by geographic level', async () => {
      mockRouter.setCurrentUrl('/data-catalogue?geographicLevel=LA');

      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 1 },
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);
      publicationService.listReleases.mockResolvedValue(testReleases);

      render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('1 data set')).toBeInTheDocument();
      });

      expect(mockRouter).toMatchObject({
        pathname: '/data-catalogue',
        query: { geographicLevel: 'LA' },
      });

      expect(screen.getByLabelText('Filter by Geographic level')).toHaveValue(
        'LA',
      );
    });

    test('filters by search term', async () => {
      mockRouter.setCurrentUrl('/data-catalogue?searchTerm=find+me');

      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: [testDataSetFileSummaries[1], testDataSetFileSummaries[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Filter by Theme')).toHaveValue('all');
      expect(screen.getByLabelText('Filter by Publication')).toHaveValue('all');
      expect(screen.getByLabelText('Filter by Releases')).toHaveValue('latest');

      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Search: find me',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Reset filters' }),
      ).toBeInTheDocument();
    });

    test('does not filter by release id only', async () => {
      mockRouter.setCurrentUrl('/data-catalogue?releaseVersionId=release-1');

      dataSetService.listDataSetFiles.mockResolvedValueOnce({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      expect(screen.getByLabelText('Filter by Theme')).toHaveValue('all');
      expect(screen.getByLabelText('Filter by Publication')).toHaveValue('all');
      expect(screen.getByLabelText('Filter by Releases')).toHaveValue('latest');

      expect(
        screen.queryByRole('button', { name: 'Reset filters' }),
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      const themesSelect = screen.getByLabelText('Filter by Theme');
      expect(themesSelect).toHaveValue('theme-2');

      const publicationsSelect = screen.getByLabelText('Filter by Publication');
      expect(publicationsSelect).not.toBeDisabled();
      expect(publicationsSelect).toHaveValue('all');
      expect(within(publicationsSelect).getAllByRole('option')).toHaveLength(3);

      await user.click(
        screen.getByRole('button', {
          name: 'Remove filter: Theme: Theme title 2',
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
          name: 'Remove filter: Theme: Theme 2',
        }),
      ).not.toBeInTheDocument();

      expect(themesSelect).toHaveValue('all');
      expect(publicationsSelect).toHaveValue('all');
      expect(publicationsSelect).toBeDisabled();
    });

    test('removing theme filter when also filtering by publication and release', async () => {
      mockRouter.setCurrentUrl(
        '/data-catalogue?themeId=theme-2&publicationId=publication-2&releaseVersionId=release-1',
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      const themesSelect = screen.getByLabelText('Filter by Theme');
      expect(themesSelect).toHaveValue('theme-2');

      const publicationsSelect = screen.getByLabelText('Filter by Publication');
      expect(publicationsSelect).not.toBeDisabled();
      expect(publicationsSelect).toHaveValue('publication-2');
      expect(within(publicationsSelect).getAllByRole('option')).toHaveLength(3);

      const releasesSelect = screen.getByLabelText('Filter by Releases');
      await waitFor(() => {
        expect(releasesSelect).toHaveValue('release-1');
      });

      expect(
        screen.queryByRole('button', {
          name: 'Remove filter: Publication: Publication title 2',
        }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('button', {
          name: 'Remove filter: Release: Release title 1',
        }),
      ).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', {
          name: 'Remove filter: Theme: Theme title 2',
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
          screen.queryByText('Publication title 2 - Release title 1 downloads'),
        ).not.toBeInTheDocument(),
      );
      expect(
        screen.queryByRole('button', {
          name: 'Remove filter: Theme: Theme title 2',
        }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', {
          name: 'Remove filter: Publication: Publication title 2',
        }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', {
          name: 'Remove filter: Release: Release title 1',
        }),
      ).not.toBeInTheDocument();

      expect(themesSelect).toHaveValue('all');
      expect(publicationsSelect).toHaveValue('all');
      expect(publicationsSelect).toBeDisabled();
      expect(releasesSelect).toHaveValue('latest');
    });

    test('removing publication filter when also filtering by release', async () => {
      mockRouter.setCurrentUrl(
        '/data-catalogue?themeId=theme-2&publicationId=publication-2&releaseVersionId=release-1',
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      const publicationsSelect = screen.getByLabelText('Filter by Publication');
      expect(publicationsSelect).toHaveValue('publication-2');

      const releasesSelect = screen.getByLabelText('Filter by Releases');
      await waitFor(() => {
        expect(releasesSelect).toHaveValue('release-1');
      });

      expect(
        screen.queryByRole('button', {
          name: 'Remove filter: Release: Release title 1',
        }),
      ).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', {
          name: 'Remove filter: Publication: Publication title 2',
        }),
      );

      expect(await screen.getByTestId('total-results')).toHaveTextContent(
        '30 data sets',
      );

      expect(mockRouter).toMatchObject({
        pathname: '/data-catalogue',
        query: { themeId: 'theme-2' },
      });

      expect(
        screen.queryByRole('button', {
          name: 'Remove filter: Publication: Publication title 2',
        }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', {
          name: 'Remove filter: Release: Release title 1',
        }),
      ).not.toBeInTheDocument();

      expect(publicationsSelect).toHaveValue('all');
      expect(releasesSelect).toHaveValue('latest');
    });

    test('removing release filter', async () => {
      mockRouter.setCurrentUrl(
        '/data-catalogue?themeId=theme-2&publicationId=publication-2&releaseVersionId=release-1',
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      const releasesSelect = screen.getByLabelText('Filter by Releases');
      await waitFor(() => {
        expect(releasesSelect).toHaveValue('release-1');
      });

      await user.click(
        screen.getByRole('button', {
          name: 'Remove filter: Release: Release title 1',
        }),
      );

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      expect(mockRouter).toMatchObject({
        pathname: '/data-catalogue',
        query: { themeId: 'theme-2', publicationId: 'publication-2' },
      });

      expect(
        screen.queryByRole('button', {
          name: 'Remove filter: Release: Release title 1',
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      await user.click(
        screen.getByRole('button', {
          name: 'Remove filter: Search: find me',
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
          name: 'Remove filter: Search: find me',
        }),
      ).not.toBeInTheDocument();
    });

    test('removing all filters', async () => {
      mockRouter.setCurrentUrl(
        '/data-catalogue?themeId=theme-2&publicationId=publication-2&releaseVersionId=release-1',
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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('2 data sets')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Publication: Publication title 2',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Theme: Theme title 2',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', {
          name: 'Remove filter: Release: Release title 1',
        }),
      ).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', {
          name: 'Reset filters',
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
          name: 'Remove filter: Publication: Publication title 2',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', {
          name: 'Remove filter: Theme: Theme title 2',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', {
          name: 'Remove filter: Release: Release title 1',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', {
          name: 'Reset filters',
        }),
      ).not.toBeInTheDocument();
    });
  });

  describe('sorting', () => {
    test('changing sort order', async () => {
      dataSetService.listDataSetFiles.mockResolvedValue({
        results: testDataSetFileSummaries,
        paging: testPaging,
      });
      publicationService.getPublicationTree.mockResolvedValue(testThemes);

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.click(screen.getByLabelText('A to Z'));

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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('30 data sets')).toBeInTheDocument();
      });

      await user.type(screen.getByLabelText('Search data sets'), 'Find me');
      await user.click(screen.getByRole('button', { name: 'Search' }));

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

      const { user } = render(<DataCataloguePage />);

      await waitFor(() => {
        expect(screen.getByText('1 data set')).toBeInTheDocument();
      });

      expect(mockRouter).toMatchObject({
        pathname: '/data-catalogue',
        query: { orderBy: 'relevance' },
      });

      await user.click(
        screen.getByRole('button', {
          name: 'Remove filter: Search: Find me',
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

    const { user } = render(<DataCataloguePage />);

    expect(await screen.findByText('30 data sets')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Filter results' }));

    expect(await screen.findByText('Back to results')).toBeInTheDocument();

    const modal = within(screen.getByRole('dialog'));

    expect(modal.getByLabelText('Filter by Theme')).toBeInTheDocument();
    expect(modal.getByLabelText('Filter by Publication')).toBeInTheDocument();
    expect(modal.getByLabelText('Filter by Releases')).toBeInTheDocument();
    expect(
      modal.getByRole('button', { name: 'Back to results' }),
    ).toBeInTheDocument();

    mockIsMedia = false;
  });
});
