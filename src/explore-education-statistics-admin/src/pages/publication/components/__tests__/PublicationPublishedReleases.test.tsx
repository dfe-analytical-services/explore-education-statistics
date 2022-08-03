import PublicationPublishedReleases from '@admin/pages/publication/components/PublicationPublishedReleases';
import { testContact } from '@admin/pages/publication/__data__/testPublication';
import _releaseService, {
  MyRelease,
  ReleaseSummary,
} from '@admin/services/releaseService';
import {
  render as baseRender,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory } from 'history';
import React, { ReactElement } from 'react';
import { MemoryRouter, Router } from 'react-router-dom';

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

describe('PublicationPublishedReleases', () => {
  const testPublicationId = 'publication-1';

  const testRelease1: MyRelease = {
    amendment: false,
    approvalStatus: 'Approved',
    id: 'release-1',
    latestRelease: false,
    live: true,
    permissions: {
      canAddPrereleaseUsers: false,
      canUpdateRelease: false,
      canDeleteRelease: false,
      canMakeAmendmentOfRelease: true,
    },
    previousVersionId: '',
    publicationId: 'publication-1',
    publicationSlug: 'publication-slug-1',
    publicationTitle: 'Publication 1',
    published: '2022-01-01T00:00:00',
    releaseName: 'Release name',
    slug: 'release-1-slug',
    title: 'Release 1',
    timePeriodCoverage: {
      label: 'Academic year',
      value: 'AY',
    },
    type: 'AdHocStatistics',
    contact: testContact,
    preReleaseAccessList: '',
  };

  const testRelease2: MyRelease = {
    ...testRelease1,
    id: 'release-2',
    published: '2022-01-02T00:00:00',
    slug: 'release-2-slug',
    title: 'Release 2',
  };

  const testRelease3: MyRelease = {
    ...testRelease1,
    id: 'release-3',
    published: '2022-01-03T00:00:00',
    slug: 'release-3-slug',
    title: 'Release 3',
  };

  const testRelease4: MyRelease = {
    ...testRelease1,
    id: 'release-4',
    published: '2022-01-04T00:00:00',
    slug: 'release-4-slug',
    title: 'Release 4',
  };

  const testRelease5: MyRelease = {
    ...testRelease1,
    id: 'release-5',
    published: '2022-01-05T00:00:00',
    slug: 'release-5-slug',
    title: 'Release 5',
  };

  const testRelease6: MyRelease = {
    ...testRelease1,
    id: 'release-6',
    published: '2022-01-06T00:00:00',
    slug: 'release-6-slug',
    title: 'Release 6',
  };

  const testRelease7: MyRelease = {
    ...testRelease1,
    id: 'release-7',
    published: '2022-01-07T00:00:00',
    slug: 'release-7-slug',
    title: 'Release 7',
  };

  const testReleases = [
    testRelease1,
    testRelease2,
    testRelease3,
    testRelease4,
    testRelease5,
    testRelease6,
    testRelease7,
  ];

  test('renders the published releases table correctly', () => {
    render(
      <PublicationPublishedReleases
        publicationId={testPublicationId}
        releases={testReleases}
      />,
    );

    expect(screen.getByText('Published releases (5 of 7)')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Show 2 more published releases' }),
    ).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(6);

    const row1cells = within(rows[1]).getAllByRole('cell');
    expect(within(row1cells[0]).getByText('Release 1')).toBeInTheDocument();
    expect(within(row1cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row1cells[2]).getByText('1 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row1cells[3]).getByRole('button', { name: 'Amend Release 1' }),
    ).toBeInTheDocument();
    expect(
      within(row1cells[3]).getByRole('link', { name: 'View Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/summary',
    );

    const row2cells = within(rows[2]).getAllByRole('cell');
    expect(within(row2cells[0]).getByText('Release 2')).toBeInTheDocument();
    expect(within(row2cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row2cells[2]).getByText('2 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row2cells[3]).getByRole('button', { name: 'Amend Release 2' }),
    ).toBeInTheDocument();
    expect(
      within(row2cells[3]).getByRole('link', { name: 'View Release 2' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2/summary',
    );

    const row3cells = within(rows[3]).getAllByRole('cell');
    expect(within(row3cells[0]).getByText('Release 3')).toBeInTheDocument();
    expect(within(row3cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row3cells[2]).getByText('3 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row3cells[3]).getByRole('button', { name: 'Amend Release 3' }),
    ).toBeInTheDocument();
    expect(
      within(row3cells[3]).getByRole('link', { name: 'View Release 3' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-3/summary',
    );

    const row4cells = within(rows[4]).getAllByRole('cell');
    expect(within(row4cells[0]).getByText('Release 4')).toBeInTheDocument();
    expect(within(row4cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row4cells[2]).getByText('4 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row4cells[3]).getByRole('button', { name: 'Amend Release 4' }),
    ).toBeInTheDocument();
    expect(
      within(row4cells[3]).getByRole('link', { name: 'View Release 4' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-4/summary',
    );

    const row5cells = within(rows[5]).getAllByRole('cell');
    expect(within(row5cells[0]).getByText('Release 5')).toBeInTheDocument();
    expect(within(row5cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row5cells[2]).getByText('5 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row5cells[3]).getByRole('button', { name: 'Amend Release 5' }),
    ).toBeInTheDocument();
    expect(
      within(row5cells[3]).getByRole('link', { name: 'View Release 5' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-5/summary',
    );
  });

  test('displays more releases when the show more button is clicked', () => {
    render(
      <PublicationPublishedReleases
        publicationId={testPublicationId}
        releases={testReleases}
      />,
    );

    expect(screen.getAllByRole('row')).toHaveLength(6);

    userEvent.click(
      screen.getByRole('button', { name: 'Show 2 more published releases' }),
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(8);

    const row6cells = within(rows[6]).getAllByRole('cell');
    expect(within(row6cells[0]).getByText('Release 6')).toBeInTheDocument();
    expect(within(row6cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row6cells[2]).getByText('6 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row6cells[3]).getByRole('button', { name: 'Amend Release 6' }),
    ).toBeInTheDocument();
    expect(
      within(row6cells[3]).getByRole('link', { name: 'View Release 6' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-6/summary',
    );

    const row7cells = within(rows[7]).getAllByRole('cell');
    expect(within(row7cells[0]).getByText('Release 7')).toBeInTheDocument();
    expect(within(row7cells[1]).getByText('Published')).toBeInTheDocument();
    expect(
      within(row7cells[2]).getByText('7 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row7cells[3]).getByRole('button', { name: 'Amend Release 7' }),
    ).toBeInTheDocument();
    expect(
      within(row7cells[3]).getByRole('link', { name: 'View Release 7' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-7/summary',
    );

    expect(
      screen.queryByRole('button', { name: 'Show 2 more published releases' }),
    ).not.toBeInTheDocument();
  });

  test('creating an amendment works correctly', async () => {
    const history = createMemoryHistory();

    releaseService.createReleaseAmendment.mockResolvedValue({
      id: 'release-amendment-id',
    } as ReleaseSummary);

    baseRender(
      <Router history={history}>
        <PublicationPublishedReleases
          publicationId={testPublicationId}
          releases={testReleases}
        />
        ,
      </Router>,
    );

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

  test('does not show the amendment button if you do not have the correct permissions', () => {
    render(
      <PublicationPublishedReleases
        publicationId={testPublicationId}
        releases={[
          {
            ...testRelease1,
            permissions: {
              ...testRelease1.permissions,
              canMakeAmendmentOfRelease: false,
            },
          },
          testRelease2,
          testRelease3,
          testRelease4,
          testRelease5,
          testRelease6,
          testRelease7,
        ]}
      />,
    );

    const rows = screen.getAllByRole('row');

    expect(
      within(rows[1]).queryByRole('button', { name: 'Amend Release 1' }),
    ).not.toBeInTheDocument();
  });
});

function render(element: ReactElement) {
  baseRender(<MemoryRouter>{element}</MemoryRouter>);
}
