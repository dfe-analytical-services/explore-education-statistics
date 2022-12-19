import baseRender from '@common-test/render';
import PublicationUnpublishedReleases from '@admin/pages/publication/components/PublicationUnpublishedReleases';
import _publicationService from '@admin/services/publicationService';
import _releaseService, {
  ReleasePermissions,
  ReleaseSummaryWithPermissions,
} from '@admin/services/releaseService';
import { PaginatedList } from '@common/services/types/pagination';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React, { ReactElement } from 'react';
import { MemoryRouter } from 'react-router-dom';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

describe('PublicationUnpublishedReleases', () => {
  const testPublicationId = 'publication-1';

  const testPermissions: ReleasePermissions = {
    canAddPrereleaseUsers: false,
    canUpdateRelease: true,
    canDeleteRelease: true,
    canMakeAmendmentOfRelease: true,
    canViewRelease: true,
  };

  const testRelease1: ReleaseSummaryWithPermissions = {
    amendment: false,
    approvalStatus: 'Approved',
    id: 'release-1',
    live: false,
    permissions: testPermissions,
    publishScheduled: '2022-01-01T00:00:00',
    slug: 'release-1-slug',
    title: 'Release 1',
    timePeriodCoverage: {
      label: 'Academic year',
      value: 'AY',
    },
    type: 'NationalStatistics',
    year: 2022,
    yearTitle: '2022/23',
  };

  const testRelease2: ReleaseSummaryWithPermissions = {
    amendment: false,
    approvalStatus: 'Draft',
    id: 'release-2',
    live: false,
    permissions: testPermissions,
    slug: 'release-2-slug',
    title: 'Release 2',
    timePeriodCoverage: {
      label: 'Academic year',
      value: 'AY',
    },
    type: 'NationalStatistics',
    year: 2024,
    yearTitle: '2024/25',
  };

  const testRelease3: ReleaseSummaryWithPermissions = {
    amendment: true,
    approvalStatus: 'HigherLevelReview',
    id: 'release-3',
    live: false,
    permissions: testPermissions,
    slug: 'release-3-slug',
    title: 'Release 3',
    timePeriodCoverage: {
      label: 'Academic year',
      value: 'AY',
    },
    type: 'NationalStatistics',
    year: 2023,
    yearTitle: '2023/24',
  };

  const testReleasesPage1: PaginatedList<ReleaseSummaryWithPermissions> = {
    paging: {
      page: 1,
      pageSize: 20,
      totalPages: 1,
      totalResults: 3,
    },
    results: [testRelease1, testRelease2, testRelease3],
  };

  beforeEach(() => {
    releaseService.getReleaseStatus.mockResolvedValue({
      overallStage: 'Complete',
    });
    releaseService.getReleaseChecklist.mockResolvedValue({
      errors: [],
      valid: true,
      warnings: [],
    });
  });

  test('renders tables of unpublished releases once loaded', async () => {
    publicationService.listReleases.mockResolvedValue(testReleasesPage1);

    render(
      <PublicationUnpublishedReleases publicationId={testPublicationId} />,
    );

    await waitFor(() => {
      expect(
        screen.queryByText('Loading scheduled releases'),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByText('Loading draft releases'),
      ).not.toBeInTheDocument();
    });

    const scheduledTable = screen.getByTestId('publication-scheduled-releases');
    const scheduledRows = within(scheduledTable).getAllByRole('row');
    expect(scheduledRows).toHaveLength(2);

    const scheduledRow1Cells = within(scheduledRows[1]).getAllByRole('cell');
    expect(
      within(scheduledRow1Cells[0]).getByText('Release 1'),
    ).toBeInTheDocument();

    expect(screen.getByText('Draft releases')).toBeInTheDocument();

    const draftTable = screen.getByTestId('publication-draft-releases');
    const draftRows = within(draftTable).getAllByRole('row');
    expect(draftRows).toHaveLength(3);

    const draftRow1Cells = within(draftRows[1]).getAllByRole('cell');
    expect(
      within(draftRow1Cells[0]).getByText('Release 2'),
    ).toBeInTheDocument();

    const draftRow2Cells = within(draftRows[2]).getAllByRole('cell');
    expect(
      within(draftRow2Cells[0]).getByText('Release 3'),
    ).toBeInTheDocument();
  });

  test('shows error messages if unpublished releases could not be loaded', async () => {
    publicationService.listReleases.mockRejectedValue(
      new Error('Something went wrong'),
    );

    render(
      <PublicationUnpublishedReleases publicationId={testPublicationId} />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('There was a problem loading the scheduled releases.'),
      ).toBeInTheDocument();
      expect(
        screen.getByText('There was a problem loading the draft releases.'),
      ).toBeInTheDocument();
    });
  });

  test('shows empty messages if there are no unpublished releases', async () => {
    publicationService.listReleases.mockResolvedValue({
      paging: {
        page: 1,
        pageSize: 20,
        totalPages: 1,
        totalResults: 0,
      },
      results: [],
    });

    render(
      <PublicationUnpublishedReleases publicationId={testPublicationId} />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('You have no scheduled releases.'),
      ).toBeInTheDocument();
      expect(
        screen.getByText('You have no draft releases.'),
      ).toBeInTheDocument();
    });

    expect(
      screen.queryByTestId('publication-scheduled-releases'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('publication-draft-releases'),
    ).not.toBeInTheDocument();
  });

  test("shows empty messages if don't have permission to view draft or scheduled releases", async () => {
    publicationService.listReleases.mockResolvedValue({
      paging: {
        page: 1,
        pageSize: 20,
        totalPages: 1,
        totalResults: 0,
      },
      results: [
        {
          ...testRelease1,
          approvalStatus: 'Approved',
          permissions: { ...testPermissions, canViewRelease: false },
        },
        {
          ...testRelease2,
          approvalStatus: 'Draft',
          permissions: { ...testPermissions, canViewRelease: false },
        },
        {
          ...testRelease3,
          approvalStatus: 'HigherLevelReview',
          permissions: { ...testPermissions, canViewRelease: false },
        },
      ],
    });

    render(
      <PublicationUnpublishedReleases publicationId={testPublicationId} />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('You have no scheduled releases.'),
      ).toBeInTheDocument();
      expect(
        screen.getByText('You have no draft releases.'),
      ).toBeInTheDocument();
    });

    expect(
      screen.queryByTestId('publication-scheduled-releases'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('publication-draft-releases'),
    ).not.toBeInTheDocument();
  });

  test('calls `onAmendmentDelete` handler when amendment is deleted', async () => {
    publicationService.listReleases.mockResolvedValue(testReleasesPage1);
    releaseService.getDeleteReleasePlan.mockResolvedValue({
      scheduledMethodologies: [],
    });

    const handleAmendmentDelete = jest.fn();

    render(
      <PublicationUnpublishedReleases
        publicationId={testPublicationId}
        onAmendmentDelete={handleAmendmentDelete}
      />,
    );

    await waitFor(() => {
      expect(
        screen.queryByText('Loading draft releases'),
      ).not.toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Cancel amendment for Release 3' }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Confirm you want to cancel this amended release'),
      ).toBeInTheDocument();
    });

    const modal = within(screen.getByRole('dialog'));

    expect(handleAmendmentDelete).not.toHaveBeenCalled();

    userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(handleAmendmentDelete).toHaveBeenCalled();
    });
  });
});

function render(element: ReactElement) {
  return baseRender(<MemoryRouter>{element}</MemoryRouter>);
}
