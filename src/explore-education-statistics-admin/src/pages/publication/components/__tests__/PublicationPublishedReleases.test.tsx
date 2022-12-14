import PublicationPublishedReleases from '@admin/pages/publication/components/PublicationPublishedReleases';
import _releaseService, {
  Release,
  ReleaseSummaryWithPermissions,
} from '@admin/services/releaseService';
import _publicationService from '@admin/services/publicationService';
import baseRender from '@common-test/render';
import { PaginatedList } from '@common/services/types/pagination';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory } from 'history';
import produce from 'immer';
import React, { ReactElement } from 'react';
import { MemoryRouter, Router } from 'react-router-dom';

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationPublishedReleases', () => {
  const testPublicationId = 'publication-1';

  const testRelease1: ReleaseSummaryWithPermissions = {
    amendment: false,
    approvalStatus: 'Approved',
    id: 'release-1',
    live: true,
    permissions: {
      canAddPrereleaseUsers: false,
      canUpdateRelease: false,
      canDeleteRelease: false,
      canMakeAmendmentOfRelease: true,
      canViewRelease: true,
    },
    published: '2022-01-01T00:00:00',
    slug: 'release-1-slug',
    title: 'Release 1',
    timePeriodCoverage: {
      label: 'Academic year',
      value: 'AY',
    },
    type: 'AdHocStatistics',
    year: 2021,
    yearTitle: '2021/22',
  };

  const testRelease2: ReleaseSummaryWithPermissions = {
    ...testRelease1,
    id: 'release-2',
    published: '2022-01-02T00:00:00',
    slug: 'release-2-slug',
    title: 'Release 2',
    year: 2020,
    yearTitle: '2020/21',
  };

  const testRelease3: ReleaseSummaryWithPermissions = {
    ...testRelease1,
    id: 'release-3',
    published: '2022-01-03T00:00:00',
    slug: 'release-3-slug',
    title: 'Release 3',
    year: 2019,
    yearTitle: '2019/20',
  };

  const testRelease4: ReleaseSummaryWithPermissions = {
    ...testRelease1,
    id: 'release-4',
    published: '2022-01-04T00:00:00',
    slug: 'release-4-slug',
    title: 'Release 4',
    year: 2018,
    yearTitle: '2018/19',
  };

  const testRelease5: ReleaseSummaryWithPermissions = {
    ...testRelease1,
    id: 'release-5',
    published: '2022-01-05T00:00:00',
    slug: 'release-5-slug',
    title: 'Release 5',
    year: 2017,
    yearTitle: '2017/18',
  };

  const testRelease6: ReleaseSummaryWithPermissions = {
    ...testRelease1,
    id: 'release-6',
    published: '2022-01-06T00:00:00',
    slug: 'release-6-slug',
    title: 'Release 6',
    year: 2016,
    yearTitle: '2016/17',
  };

  const testRelease7: ReleaseSummaryWithPermissions = {
    ...testRelease1,
    id: 'release-7',
    published: '2022-01-07T00:00:00',
    slug: 'release-7-slug',
    title: 'Release 7',
    year: 2015,
    yearTitle: '2015/16',
  };

  const testReleases: ReleaseSummaryWithPermissions[] = [
    testRelease1,
    testRelease2,
    testRelease3,
    testRelease4,
    testRelease5,
    testRelease6,
    testRelease7,
  ];

  const testReleasesPage1: PaginatedList<ReleaseSummaryWithPermissions> = {
    paging: {
      page: 1,
      pageSize: 5,
      totalPages: 2,
      totalResults: 7,
    },
    results: testReleases.slice(0, 5),
  };
  const testReleasesPage2: PaginatedList<ReleaseSummaryWithPermissions> = {
    paging: {
      page: 2,
      pageSize: 5,
      totalPages: 2,
      totalResults: 7,
    },
    results: testReleases.slice(5),
  };

  test('renders the published releases table once loaded', async () => {
    publicationService.listReleases.mockResolvedValue(testReleasesPage1);

    render(<PublicationPublishedReleases publicationId={testPublicationId} />);

    await waitFor(() => {
      expect(
        screen.queryByText('Loading published releases'),
      ).not.toBeInTheDocument();
    });

    expect(screen.getByText('Published releases (5 of 7)')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Show 2 more published releases' }),
    ).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(6);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(within(row1Cells[0]).getByText('Release 1')).toBeInTheDocument();
    expect(within(row1Cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row1Cells[2]).getByText('1 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row1Cells[3]).getByRole('button', { name: 'Amend Release 1' }),
    ).toBeInTheDocument();
    expect(
      within(row1Cells[3]).getByRole('link', { name: 'View Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/summary',
    );

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(within(row2Cells[0]).getByText('Release 2')).toBeInTheDocument();
    expect(within(row2Cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row2Cells[2]).getByText('2 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row2Cells[3]).getByRole('button', { name: 'Amend Release 2' }),
    ).toBeInTheDocument();
    expect(
      within(row2Cells[3]).getByRole('link', { name: 'View Release 2' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2/summary',
    );

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(within(row3Cells[0]).getByText('Release 3')).toBeInTheDocument();
    expect(within(row3Cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row3Cells[2]).getByText('3 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row3Cells[3]).getByRole('button', { name: 'Amend Release 3' }),
    ).toBeInTheDocument();
    expect(
      within(row3Cells[3]).getByRole('link', { name: 'View Release 3' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-3/summary',
    );

    const row4Cells = within(rows[4]).getAllByRole('cell');
    expect(within(row4Cells[0]).getByText('Release 4')).toBeInTheDocument();
    expect(within(row4Cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row4Cells[2]).getByText('4 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row4Cells[3]).getByRole('button', { name: 'Amend Release 4' }),
    ).toBeInTheDocument();
    expect(
      within(row4Cells[3]).getByRole('link', { name: 'View Release 4' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-4/summary',
    );

    const row5Cells = within(rows[5]).getAllByRole('cell');
    expect(within(row5Cells[0]).getByText('Release 5')).toBeInTheDocument();
    expect(within(row5Cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row5Cells[2]).getByText('5 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row5Cells[3]).getByRole('button', { name: 'Amend Release 5' }),
    ).toBeInTheDocument();
    expect(
      within(row5Cells[3]).getByRole('link', { name: 'View Release 5' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-5/summary',
    );
  });

  test('renders show more button with a maximum of the default `pageSize`', async () => {
    publicationService.listReleases.mockResolvedValue(
      produce(testReleasesPage1, draft => {
        draft.paging.totalPages = 3;
        draft.paging.totalResults = 12;
      }),
    );

    render(<PublicationPublishedReleases publicationId={testPublicationId} />);

    await waitFor(() => {
      expect(
        screen.queryByText('Loading published releases'),
      ).not.toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', { name: 'Show 5 more published releases' }),
    ).toBeInTheDocument();
  });

  test('displays more releases when the show more button is clicked', async () => {
    publicationService.listReleases
      .mockResolvedValueOnce(testReleasesPage1)
      .mockResolvedValueOnce(testReleasesPage2);

    render(<PublicationPublishedReleases publicationId={testPublicationId} />);

    await waitFor(() => {
      expect(
        screen.queryByText('Loading published releases'),
      ).not.toBeInTheDocument();
    });

    expect(screen.getByText('Published releases (5 of 7)'));
    expect(screen.getAllByRole('row')).toHaveLength(6);

    userEvent.click(
      screen.getByRole('button', { name: 'Show 2 more published releases' }),
    );
    await waitFor(() => {
      expect(
        screen.queryByText('Show 2 more published releases'),
      ).not.toBeInTheDocument();
    });

    expect(screen.getByText('Published releases (7 of 7)'));

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(8);

    const row6Cells = within(rows[6]).getAllByRole('cell');
    expect(within(row6Cells[0]).getByText('Release 6')).toBeInTheDocument();
    expect(within(row6Cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row6Cells[2]).getByText('6 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row6Cells[3]).getByRole('button', { name: 'Amend Release 6' }),
    ).toBeInTheDocument();
    expect(
      within(row6Cells[3]).getByRole('link', { name: 'View Release 6' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-6/summary',
    );

    const row7Cells = within(rows[7]).getAllByRole('cell');
    expect(within(row7Cells[0]).getByText('Release 7')).toBeInTheDocument();
    expect(within(row7Cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row7Cells[2]).getByText('7 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row7Cells[3]).getByRole('button', { name: 'Amend Release 7' }),
    ).toBeInTheDocument();
    expect(
      within(row7Cells[3]).getByRole('link', { name: 'View Release 7' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-7/summary',
    );

    expect(
      screen.queryByRole('button', { name: 'Show 2 more published releases' }),
    ).not.toBeInTheDocument();
  });

  test('creating an amendment works correctly', async () => {
    publicationService.listReleases.mockResolvedValue(testReleasesPage1);

    const history = createMemoryHistory();

    releaseService.createReleaseAmendment.mockResolvedValue({
      id: 'release-amendment-id',
    } as Release);

    baseRender(
      <Router history={history}>
        <PublicationPublishedReleases publicationId={testPublicationId} />,
      </Router>,
    );

    await waitFor(() => {
      expect(
        screen.queryByText('Loading published releases'),
      ).not.toBeInTheDocument();
    });

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(6);

    userEvent.click(
      within(rows[1]).getByRole('button', { name: 'Amend Release 1' }),
    );

    expect(
      screen.getByText('Confirm you want to amend this published release'),
    ).toBeInTheDocument();

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByText('Confirm you want to amend this published release'),
    ).toBeInTheDocument();

    userEvent.click(
      modal.getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(releaseService.createReleaseAmendment).toHaveBeenCalledWith(
        testRelease1.id,
      );
    });

    expect(history.location.pathname).toBe(
      '/publication/publication-1/release/release-amendment-id/summary',
    );
  });

  test('does not show the amendment button if you do not have the correct permissions', async () => {
    publicationService.listReleases.mockResolvedValue(
      produce(testReleasesPage1, draft => {
        draft.results[0].permissions.canMakeAmendmentOfRelease = false;
      }),
    );

    render(<PublicationPublishedReleases publicationId={testPublicationId} />);

    await waitFor(() => {
      expect(
        screen.queryByText('Loading published releases'),
      ).not.toBeInTheDocument();
    });

    const rows = screen.getAllByRole('row');

    expect(
      within(rows[1]).queryByRole('button', { name: 'Amend Release 1' }),
    ).not.toBeInTheDocument();
  });
});

function render(element: ReactElement) {
  baseRender(<MemoryRouter>{element}</MemoryRouter>);
}
