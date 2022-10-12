import NonScheduledReleaseSummary from '@admin/pages/admin-dashboard/components/NonScheduledReleaseSummary';
import _releaseService, {
  Release,
  ReleaseStageStatuses,
  ReleaseWithPermissions,
} from '@admin/services/releaseService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory } from 'history';
import produce from 'immer';
import { noop } from 'lodash';
import React from 'react';
import { MemoryRouter, Router } from 'react-router-dom';

jest.mock('@admin/services/releaseService');

const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

describe('NonScheduledReleaseSummary', () => {
  const testRelease: ReleaseWithPermissions = {
    id: 'rel-3',
    latestRelease: false,
    published: '2021-01-01T11:21:17',
    slug: 'rel-3-slug',
    title: 'The Release',
    previousVersionId: 'rel-2',
    publicationId: 'publication-id',
    permissions: {
      canUpdateRelease: false,
      canDeleteRelease: false,
      canMakeAmendmentOfRelease: false,
      canAddPrereleaseUsers: false,
    },
    approvalStatus: 'Draft',
  } as ReleaseWithPermissions;

  const completeReleaseStatus: ReleaseStageStatuses = {
    overallStage: 'Complete',
  };

  beforeEach(() => {
    releaseService.getReleaseStatus.mockResolvedValue(completeReleaseStatus);
  });

  test('renders correctly with a release and no permissions', async () => {
    render(
      <MemoryRouter>
        <NonScheduledReleaseSummary
          includeCreateAmendmentControls
          release={testRelease}
          onAmendmentCancelled={noop}
        />
      </MemoryRouter>,
    );

    // Expand the Release to see its details.
    userEvent.click(
      screen.getByRole('button', {
        name: `${testRelease.title} (not Live) Draft`,
      }),
    );

    expect(
      screen.getByRole('link', {
        name: 'View release',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Edit release',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Amend release',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Cancel amendment',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'View release amendment',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Edit release amendment',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'View existing version',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with a release and edit / create amendment permissions', async () => {
    render(
      <MemoryRouter>
        <NonScheduledReleaseSummary
          includeCreateAmendmentControls
          release={produce(testRelease, draft => {
            draft.permissions.canUpdateRelease = true;
            draft.permissions.canMakeAmendmentOfRelease = true;
          })}
          onAmendmentCancelled={noop}
        />
      </MemoryRouter>,
    );

    // Expand the Release to see its details.
    userEvent.click(
      screen.getByRole('button', {
        name: `${testRelease.title} (not Live) Draft`,
      }),
    );

    expect(
      screen.queryByRole('link', {
        name: 'View release',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Edit release',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Amend release',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Cancel amendment',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'View release amendment',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Edit release amendment',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'View existing version',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with a release amendment and no permissions', async () => {
    render(
      <MemoryRouter>
        <NonScheduledReleaseSummary
          includeCreateAmendmentControls
          release={produce(testRelease, draft => {
            draft.amendment = true;
            draft.previousVersionId = 'rel-2';
          })}
          onAmendmentCancelled={noop}
        />
      </MemoryRouter>,
    );

    // Expand the Release to see its details.
    userEvent.click(
      screen.getByRole('button', {
        name: `${testRelease.title} (not Live) Draft Amendment`,
      }),
    );

    expect(
      screen.queryByRole('link', {
        name: 'View release',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Edit release',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Amend release',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Cancel amendment',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'View release amendment',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Edit release amendment',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'View existing version',
      }),
    ).toBeInTheDocument();
  });

  test('renders correctly with a release amendment and full permissions', async () => {
    render(
      <MemoryRouter>
        <NonScheduledReleaseSummary
          includeCreateAmendmentControls
          release={produce(testRelease, draft => {
            draft.amendment = true;
            draft.previousVersionId = 'rel-2';
            draft.permissions.canUpdateRelease = true;
            draft.permissions.canDeleteRelease = true;
          })}
          onAmendmentCancelled={noop}
        />
      </MemoryRouter>,
    );

    // Expand the Release to see its details.
    userEvent.click(
      screen.getByRole('button', {
        name: `${testRelease.title} (not Live) Draft Amendment`,
      }),
    );

    expect(
      screen.queryByRole('link', {
        name: 'View release',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Edit release',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Amend release',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Cancel amendment',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'View release amendment',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Edit release amendment',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'View existing version',
      }),
    ).toBeInTheDocument();
  });

  test('handles creating amendments ok', async () => {
    const history = createMemoryHistory();

    render(
      <Router history={history}>
        <NonScheduledReleaseSummary
          includeCreateAmendmentControls
          release={produce(testRelease, draft => {
            draft.permissions.canMakeAmendmentOfRelease = true;
          })}
          onAmendmentCancelled={noop}
        />
      </Router>,
    );

    // Expand the Release to see the controls within it.
    userEvent.click(
      screen.getByRole('button', {
        name: `${testRelease.title} (not Live) Draft`,
      }),
    );

    // Now click the "Amend release" button and check that the warning modal appears.
    userEvent.click(
      screen.getByRole('button', {
        name: 'Amend release',
      }),
    );

    expect(
      screen.getByText('Confirm you want to amend this live release'),
    ).toBeInTheDocument();

    // Confirm the amending of the Release.
    releaseService.createReleaseAmendment.mockResolvedValue({
      id: 'release-amendment-id',
    } as Release);

    userEvent.click(
      screen.getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(releaseService.createReleaseAmendment).toHaveBeenCalledWith(
        testRelease.id,
      );
    });

    expect(history.location.pathname).toBe(
      `/publication/${testRelease.publicationId}/release/release-amendment-id/summary`,
    );
  });

  test('handles cancelling amendments ok', async () => {
    const onAmendmentCancelled = jest.fn();

    render(
      <MemoryRouter>
        <NonScheduledReleaseSummary
          includeCreateAmendmentControls
          release={produce(testRelease, draft => {
            draft.permissions.canDeleteRelease = true;
            draft.amendment = true;
            draft.previousVersionId = 'rel-2';
          })}
          onAmendmentCancelled={onAmendmentCancelled}
        />
      </MemoryRouter>,
    );

    // Expand the Release Amendment to see the controls within it.
    userEvent.click(
      screen.getByRole('button', {
        name: `${testRelease.title} (not Live) Draft Amendment`,
      }),
    );

    // Now click the "Cancel amendment" button and check that the warning modal appears.
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

    userEvent.click(
      screen.getByRole('button', {
        name: 'Cancel amendment',
      }),
    );

    await waitFor(() => {
      expect(releaseService.getDeleteReleasePlan).toHaveBeenCalledWith(
        testRelease.id,
      );
    });

    expect(
      screen.getByText('Confirm you want to cancel this amended release'),
    ).toBeInTheDocument();
    expect(screen.getByText('Methodology 1')).toBeInTheDocument();
    expect(screen.getByText('Methodology 2')).toBeInTheDocument();

    // Confirm the cancelling of the Release Amendment.
    userEvent.click(
      screen.getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(releaseService.deleteRelease).toHaveBeenCalledWith(testRelease.id);
      expect(onAmendmentCancelled).toHaveBeenCalled();
    });

    expect(
      screen.queryByText('Confirm you want to cancel this amended release'),
    ).not.toBeInTheDocument();
  });

  test('false "includeCreateAmendmentControls" flag hides Create Amendment controls', async () => {
    render(
      <MemoryRouter>
        <NonScheduledReleaseSummary
          release={produce(testRelease, draft => {
            draft.permissions.canMakeAmendmentOfRelease = true;
          })}
          onAmendmentCancelled={noop}
        />
      </MemoryRouter>,
    );

    // Expand the Release to see the controls within it.
    userEvent.click(
      screen.getByRole('button', {
        name: `${testRelease.title} (not Live) Draft`,
      }),
    );

    // Assert that the Amend release controls are not displayed as per our usage if the "includeCreateAmendmentControls"
    // flag.
    expect(
      screen.queryByRole('button', {
        name: 'Amend release',
      }),
    ).not.toBeInTheDocument();
  });
});
