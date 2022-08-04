import PublicationScheduledReleases from '@admin/pages/publication/components/PublicationScheduledReleases';
import { testContact } from '@admin/pages/publication/__data__/testPublication';
import _releaseService, { MyRelease } from '@admin/services/releaseService';
import {
  render as baseRender,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import React, { ReactElement } from 'react';
import { MemoryRouter } from 'react-router-dom';

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

describe('PublicationScheduledReleases', () => {
  const testRelease1: MyRelease = {
    amendment: false,
    approvalStatus: 'Approved',
    id: 'release-1',
    latestRelease: false,
    live: false,
    permissions: {
      canAddPrereleaseUsers: false,
      canUpdateRelease: true,
      canDeleteRelease: false,
      canMakeAmendmentOfRelease: true,
    },
    previousVersionId: 'release-previous-id',
    publicationId: 'publication-1',
    publicationSlug: 'publication-slug-1',
    publicationTitle: 'Publication 1',
    published: '2022-01-01T00:00:00',
    publishScheduled: '2022-01-01T00:00:00',
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
    approvalStatus: 'Approved',
    id: 'release-2',
    published: '2022-01-02T00:00:00',
    publishScheduled: '2022-01-02T00:00:00',
    slug: 'release-2-slug',
    title: 'Release 2',
  };

  const testReleases = [testRelease1, testRelease2];

  beforeEach(() => {
    releaseService.getReleaseStatus.mockResolvedValue({
      overallStage: 'Scheduled',
    });
  });

  test('renders the scheduled releases table correctly', async () => {
    render(<PublicationScheduledReleases releases={testReleases} />);

    expect(screen.getByText('Scheduled releases')).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    const row1cells = within(rows[1]).getAllByRole('cell');
    expect(within(row1cells[0]).getByText('Release 1')).toBeInTheDocument();

    await waitFor(() => {
      expect(within(row1cells[1]).getByText('Scheduled')).toBeInTheDocument();
    });

    expect(
      within(row1cells[2]).getByRole('button', { name: 'View stages' }),
    ).toBeInTheDocument();
    expect(
      within(row1cells[3]).getByText('1 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row1cells[4]).getByRole('link', { name: 'Edit Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/summary',
    );

    const row2cells = within(rows[2]).getAllByRole('cell');
    expect(within(row2cells[0]).getByText('Release 2')).toBeInTheDocument();
    expect(within(row2cells[1]).getByText('Scheduled')).toBeInTheDocument();
    expect(
      within(row2cells[2]).getByRole('button', { name: 'View stages' }),
    ).toBeInTheDocument();
    expect(
      within(row2cells[3]).getByText('2 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row2cells[4]).getByRole('link', { name: 'Edit Release 2' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2/summary',
    );
  });

  test('shows a view instead of edit link if you do not have permission to edit the release', () => {
    render(
      <PublicationScheduledReleases
        releases={[
          {
            ...testRelease1,
            permissions: {
              ...testRelease1.permissions,
              canUpdateRelease: false,
            },
          },
          testRelease2,
        ]}
      />,
    );

    const rows = screen.getAllByRole('row');
    const row1cells = within(rows[1]).getAllByRole('cell');

    expect(within(row1cells[0]).getByText('Release 1')).toBeInTheDocument();

    expect(
      within(row1cells[4]).getByRole('link', { name: 'View Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/summary',
    );
  });
});

function render(element: ReactElement) {
  baseRender(<MemoryRouter>{element}</MemoryRouter>);
}
