import baseRender from '@common-test/render';
import PublicationUnpublishedReleases from '@admin/pages/publication/components/PublicationUnpublishedReleases';
import _publicationService from '@admin/services/publicationService';
import _releaseVersionService, {
  ReleaseVersionPermissions,
  ReleaseVersionSummaryWithPermissions,
} from '@admin/services/releaseVersionService';
import { PaginatedList } from '@common/services/types/pagination';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React, { ReactNode } from 'react';
import { MemoryRouter } from 'react-router-dom';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

jest.mock('@admin/services/releaseVersionService');
const releaseVersionService = _releaseVersionService as jest.Mocked<
  typeof _releaseVersionService
>;

describe('PublicationUnpublishedReleases', () => {
  const testPublicationId = 'publication-1';

  const testPermissions: ReleaseVersionPermissions = {
    canAddPrereleaseUsers: false,
    canUpdateRelease: true,
    canUpdateReleaseVersion: true,
    canDeleteReleaseVersion: true,
    canMakeAmendmentOfReleaseVersion: true,
    canViewReleaseVersion: true,
  };

  const testRelease1: ReleaseVersionSummaryWithPermissions = {
    amendment: false,
    approvalStatus: 'Approved',
    id: 'release-1-version-1',
    releaseId: 'release-1',
    live: false,
    permissions: testPermissions,
    publishScheduled: '2022-01-01T00:00:00',
    slug: 'release-1-slug',
    title: 'Release 1',
    timePeriodCoverage: {
      label: 'Academic year',
      value: 'AY',
    },
    type: 'AccreditedOfficialStatistics',
    year: 2022,
    yearTitle: '2022/23',
    latestRelease: false,
  };

  const testRelease2: ReleaseVersionSummaryWithPermissions = {
    amendment: false,
    approvalStatus: 'Draft',
    id: 'release-2-version-1',
    releaseId: 'release-2',
    live: false,
    permissions: testPermissions,
    slug: 'release-2-slug',
    title: 'Release 2',
    timePeriodCoverage: {
      label: 'Academic year',
      value: 'AY',
    },
    type: 'AccreditedOfficialStatistics',
    year: 2024,
    yearTitle: '2024/25',
    latestRelease: false,
  };

  const testRelease3: ReleaseVersionSummaryWithPermissions = {
    amendment: true,
    approvalStatus: 'HigherLevelReview',
    id: 'release-version-1',
    releaseId: 'release-3',
    live: false,
    permissions: testPermissions,
    slug: 'release-3-slug',
    title: 'Release 3',
    timePeriodCoverage: {
      label: 'Academic year',
      value: 'AY',
    },
    type: 'AccreditedOfficialStatistics',
    year: 2023,
    yearTitle: '2023/24',
    latestRelease: false,
  };

  const testReleasesPage1: PaginatedList<ReleaseVersionSummaryWithPermissions> =
    {
      paging: {
        page: 1,
        pageSize: 20,
        totalPages: 1,
        totalResults: 3,
      },
      results: [testRelease1, testRelease2, testRelease3],
    };

  beforeEach(() => {
    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
      overallStage: 'Complete',
    });
    releaseVersionService.getReleaseVersionChecklist.mockResolvedValue({
      errors: [],
      valid: true,
      warnings: [],
    });
  });

  test('renders tables of unpublished releases once loaded', async () => {
    publicationService.listReleaseVersions.mockResolvedValue(testReleasesPage1);

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
    expect(within(draftRow1Cells[1]).getByText('Draft')).toBeInTheDocument();
    expect(
      within(draftRow1Cells[4]).getByRole('link', {
        name: 'Edit draft Release 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(draftRow1Cells[4]).getByRole('button', {
        name: 'Delete Release 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(draftRow1Cells[4]).queryByRole('button', {
        name: 'Cancel amendment for Release 2',
      }),
    ).not.toBeInTheDocument();

    const draftRow2Cells = within(draftRows[2]).getAllByRole('cell');
    expect(
      within(draftRow2Cells[0]).getByText('Release 3'),
    ).toBeInTheDocument();
    expect(
      within(draftRow2Cells[1]).getByText('In Review Amendment'),
    ).toBeInTheDocument();
    expect(
      within(draftRow2Cells[4]).getByRole('link', {
        name: 'Edit draft Release 3',
      }),
    ).toBeInTheDocument();
    expect(
      within(draftRow2Cells[4]).queryByRole('button', {
        name: 'Delete Release 3',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(draftRow2Cells[4]).getByRole('button', {
        name: 'Cancel amendment for Release 3',
      }),
    ).toBeInTheDocument();
  });

  test('shows error messages if unpublished releases could not be loaded', async () => {
    publicationService.listReleaseVersions.mockRejectedValue(
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
    publicationService.listReleaseVersions.mockResolvedValue({
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
    publicationService.listReleaseVersions.mockResolvedValue({
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
          permissions: { ...testPermissions, canViewReleaseVersion: false },
        },
        {
          ...testRelease2,
          approvalStatus: 'Draft',
          permissions: { ...testPermissions, canViewReleaseVersion: false },
        },
        {
          ...testRelease3,
          approvalStatus: 'HigherLevelReview',
          permissions: { ...testPermissions, canViewReleaseVersion: false },
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
    publicationService.listReleaseVersions.mockResolvedValue(testReleasesPage1);
    releaseVersionService.getDeleteReleaseVersionPlan.mockResolvedValue({
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

    await userEvent.click(
      screen.getByRole('button', { name: 'Cancel amendment for Release 3' }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Confirm you want to cancel this amended release'),
      ).toBeInTheDocument();
    });

    const modal = within(screen.getByRole('dialog'));

    expect(handleAmendmentDelete).not.toHaveBeenCalled();

    await userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(handleAmendmentDelete).toHaveBeenCalled();
    });
  });

  test('shows Back to top link when showBackToTopLink prop is true', async () => {
    const handleAmendmentDelete = jest.fn();
    publicationService.listReleaseVersions.mockResolvedValue(testReleasesPage1);
    releaseVersionService.getDeleteReleaseVersionPlan.mockResolvedValue({
      scheduledMethodologies: [],
    });

    render(
      <PublicationUnpublishedReleases
        publicationId={testPublicationId}
        onAmendmentDelete={handleAmendmentDelete}
        showBackToTopLink
      />,
    );
    await waitFor(() => {
      const backToTopLinks = screen.getAllByRole('link', {
        name: 'Back to top',
      });
      expect(backToTopLinks.length).toBe(2); // Scheduled and Draft links both have 'Back to top' links
    });
  });

  test('does not show Back to top link when showBackToTopLink prop is not provided', async () => {
    const handleAmendmentDelete = jest.fn();

    render(
      <PublicationUnpublishedReleases
        publicationId={testPublicationId}
        onAmendmentDelete={handleAmendmentDelete}
      />,
    );
    await waitFor(() => {
      expect(
        screen.queryByRole('link', { name: 'Back to top' }),
      ).not.toBeInTheDocument();
    });
  });
});

function render(element: ReactNode) {
  return baseRender(<MemoryRouter>{element}</MemoryRouter>);
}
