import PublicationDraftReleases from '@admin/pages/publication/components/PublicationDraftReleases';
import _releaseService, {
  ReleaseSummaryWithPermissions,
} from '@admin/services/releaseService';
import {
  render as baseRender,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React, { ReactElement } from 'react';
import { MemoryRouter } from 'react-router-dom';

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

describe('PublicationDraftReleases', () => {
  const testPublicationId = 'publication-1';

  const testRelease1: ReleaseSummaryWithPermissions = {
    amendment: false,
    approvalStatus: 'Draft',
    id: 'release-1',
    live: false,
    permissions: {
      canAddPrereleaseUsers: false,
      canUpdateRelease: true,
      canDeleteRelease: false,
      canMakeAmendmentOfRelease: true,
      canViewRelease: false,
    },
    slug: 'release-1-slug',
    title: 'Release 1',
    timePeriodCoverage: {
      label: 'Academic year',
      value: 'AY',
    },
    type: 'AdHocStatistics',
    year: 2022,
    yearTitle: '2022/23',
  };

  const testRelease2: ReleaseSummaryWithPermissions = {
    ...testRelease1,
    approvalStatus: 'HigherLevelReview',
    id: 'release-2',
    published: '2022-01-02T00:00:00',
    slug: 'release-2-slug',
    title: 'Release 2',
  };

  const testRelease3: ReleaseSummaryWithPermissions = {
    ...testRelease1,
    amendment: true,
    id: 'release-3',
    permissions: {
      canAddPrereleaseUsers: false,
      canUpdateRelease: true,
      canDeleteRelease: true,
      canMakeAmendmentOfRelease: true,
      canViewRelease: false,
    },
    previousVersionId: 'release-previous-id',
    published: '2022-01-03T00:00:00',
    slug: 'release-3-slug',
    title: 'Release 3',
  };

  const testReleases = [testRelease1, testRelease2, testRelease3];

  beforeAll(() => {
    jest.useFakeTimers();
  });

  afterAll(() => {
    jest.useRealTimers();
  });

  beforeEach(() => {
    releaseService.getReleaseChecklist.mockResolvedValue({
      errors: [
        {
          code: 'DataFileImportsMustBeCompleted',
        },
        {
          code: 'EmptyContentSectionExists',
        },
      ],
      valid: false,
      warnings: [{ code: 'NoMethodology' }],
    });
  });

  test('renders the draft releases table correctly', async () => {
    render(
      <PublicationDraftReleases
        publicationId={testPublicationId}
        releases={testReleases}
        onAmendmentDelete={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(4);

    // Draft
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(within(row1Cells[0]).getByText('Release 1')).toBeInTheDocument();
    expect(within(row1Cells[1]).getByText('Draft')).toBeInTheDocument();

    await waitFor(() => {
      // Errors
      expect(within(row1Cells[2]).getByText('2')).toBeInTheDocument();
      // Warnings
      expect(within(row1Cells[3]).getByText('1')).toBeInTheDocument();
    });

    expect(
      within(row1Cells[4]).getByRole('link', { name: 'Edit Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/summary',
    );

    expect(
      within(row1Cells[4]).queryByRole('button', {
        name: 'Cancel amendment for Release 1',
      }),
    ).not.toBeInTheDocument();

    expect(
      within(row1Cells[4]).queryByRole('link', {
        name: 'View existing version for Release 1',
      }),
    ).not.toBeInTheDocument();

    // In review
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(within(row2Cells[0]).getByText('Release 2')).toBeInTheDocument();
    expect(within(row2Cells[1]).getByText('In Review')).toBeInTheDocument();

    await waitFor(() => {
      // Errors
      expect(within(row2Cells[2]).getByText('2')).toBeInTheDocument();
      // Warnings
      expect(within(row2Cells[3]).getByText('1')).toBeInTheDocument();
    });

    expect(
      within(row2Cells[4]).getByRole('link', { name: 'Edit Release 2' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2/summary',
    );

    expect(
      within(row2Cells[4]).queryByRole('button', {
        name: 'Cancel amendment for Release 2',
      }),
    ).not.toBeInTheDocument();

    expect(
      within(row2Cells[4]).queryByRole('link', {
        name: 'View existing for Release 2',
      }),
    ).not.toBeInTheDocument();

    // Amendment
    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(within(row3Cells[0]).getByText('Release 3')).toBeInTheDocument();
    expect(
      within(row3Cells[1]).getByText('Draft Amendment'),
    ).toBeInTheDocument();

    await waitFor(() => {
      // Errors
      expect(within(row3Cells[2]).getByText('2')).toBeInTheDocument();
      // Warnings
      expect(within(row3Cells[3]).getByText('1')).toBeInTheDocument();
    });

    expect(
      within(row3Cells[4]).getByRole('link', { name: 'Edit Release 3' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-3/summary',
    );

    expect(
      within(row3Cells[4]).getByRole('button', {
        name: 'Cancel amendment for Release 3',
      }),
    ).toBeInTheDocument();

    expect(
      within(row3Cells[4]).getByRole('link', {
        name: 'View existing version for Release 3',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-previous-id/summary',
    );
  });

  test('shows an empty message when there are no releases', () => {
    render(
      <PublicationDraftReleases
        publicationId={testPublicationId}
        releases={[]}
        onAmendmentDelete={noop}
      />,
    );

    expect(screen.getByText('You have no draft releases.')).toBeInTheDocument();
  });

  test('shows a view instead of edit link if you do not have permission to edit the release', () => {
    render(
      <PublicationDraftReleases
        publicationId={testPublicationId}
        releases={[
          {
            ...testRelease1,
            permissions: {
              ...testRelease1.permissions,
              canUpdateRelease: false,
            },
          },
          testRelease2,
          testRelease3,
        ]}
        onAmendmentDelete={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    const row1Cells = within(rows[1]).getAllByRole('cell');

    expect(
      within(row1Cells[4]).getByRole('link', { name: 'View Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/summary',
    );
  });

  test('does not show the cancel button if you do not have permission to cancel the amendment', () => {
    render(
      <PublicationDraftReleases
        publicationId={testPublicationId}
        releases={[
          testRelease1,
          testRelease2,
          {
            ...testRelease3,
            permissions: {
              ...testRelease2.permissions,
              canDeleteRelease: false,
            },
          },
        ]}
        onAmendmentDelete={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(
      within(row3Cells[4]).queryByRole('button', {
        name: 'Cancel amendment for Release 3',
      }),
    ).not.toBeInTheDocument();
  });

  test('handles cancelling an amendment correctly', async () => {
    const handleOnChange = jest.fn();
    releaseService.getDeleteReleasePlan.mockResolvedValue({
      scheduledMethodologies: [
        {
          id: 'methodology-1',
          title: 'Methodology 1',
        },
        {
          id: 'methodology-2',
          title: 'Methodology 2',
        },
      ],
    });

    render(
      <PublicationDraftReleases
        publicationId={testPublicationId}
        releases={testReleases}
        onAmendmentDelete={handleOnChange}
      />,
    );

    const rows = screen.getAllByRole('row');
    const row3Cells = within(rows[3]).getAllByRole('cell');

    userEvent.click(
      within(row3Cells[4]).getByRole('button', {
        name: 'Cancel amendment for Release 3',
      }),
    );

    await waitFor(() => {
      expect(releaseService.getDeleteReleasePlan).toHaveBeenCalledWith(
        testRelease3.id,
      );
    });

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByText('Confirm you want to cancel this amended release'),
    ).toBeInTheDocument();
    expect(modal.getByText('Methodology 1')).toBeInTheDocument();
    expect(modal.getByText('Methodology 2')).toBeInTheDocument();

    userEvent.click(
      modal.getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(releaseService.deleteRelease).toHaveBeenCalledWith(
        testRelease3.id,
      );
    });

    expect(handleOnChange).toHaveBeenCalled();

    expect(
      screen.queryByText('Confirm you want to cancel this amended release'),
    ).not.toBeInTheDocument();
  });
});

function render(element: ReactElement) {
  baseRender(<MemoryRouter>{element}</MemoryRouter>);
}
