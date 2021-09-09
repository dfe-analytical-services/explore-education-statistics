import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import { noop } from 'lodash';
import { MemoryRouter, Router } from 'react-router-dom';
import _releaseService, {
  MyRelease,
  ReleaseSummary,
} from '@admin/services/releaseService';
import userEvent from '@testing-library/user-event';
import NonScheduledReleaseSummary from '@admin/pages/admin-dashboard/components/NonScheduledReleaseSummary';
import produce from 'immer';
import createMemoryHistoryWithMockedPush from '@admin-test/createMemoryHistoryWithMockedPush';

jest.mock('@admin/services/releaseService');

const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

describe('NonScheduledReleaseSummary', () => {
  const testRelease: MyRelease = {
    id: 'rel-3',
    latestRelease: false,
    published: '2021-01-01T11:21:17.7585345',
    slug: 'rel-3-slug',
    title: 'The Release',
    previousVersionId: 'rel-2',
    publicationId: 'publication-id',
    permissions: {
      canUpdateRelease: false,
      canDeleteRelease: false,
      canMakeAmendmentOfRelease: false,
    },
    approvalStatus: 'Draft',
  } as MyRelease;

  test('renders correctly with a release and no permissions', async () => {
    render(
      <MemoryRouter>
        <NonScheduledReleaseSummary
          release={testRelease}
          includeCreateAmendmentControls
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
      screen.getByText('View this release', {
        selector: 'a',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText('Edit this release', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Amend this release',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Cancel amendment',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText('View this release amendment', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText('Edit this release amendment', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText('View original release', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with a release and edit / create amendment permissions', async () => {
    render(
      <MemoryRouter>
        <NonScheduledReleaseSummary
          release={produce(testRelease, draft => {
            draft.permissions.canUpdateRelease = true;
            draft.permissions.canMakeAmendmentOfRelease = true;
          })}
          includeCreateAmendmentControls
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
      screen.queryByText('View this release', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('Edit this release', {
        selector: 'a',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Amend this release',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Cancel amendment',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText('View this release amendment', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText('Edit this release amendment', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText('View original release', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with a release amendment and no permissions', async () => {
    render(
      <MemoryRouter>
        <NonScheduledReleaseSummary
          release={produce(testRelease, draft => {
            draft.amendment = true;
            draft.previousVersionId = 'rel-2';
          })}
          includeCreateAmendmentControls
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
      screen.queryByText('View this release', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText('Edit this release', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Amend this release',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Cancel amendment',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('View this release amendment', {
        selector: 'a',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText('Edit this release amendment', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('View original release', {
        selector: 'a',
      }),
    ).toBeInTheDocument();
  });

  test('renders correctly with a release amendment and full permissions', async () => {
    render(
      <MemoryRouter>
        <NonScheduledReleaseSummary
          release={produce(testRelease, draft => {
            draft.amendment = true;
            draft.previousVersionId = 'rel-2';
            draft.permissions.canUpdateRelease = true;
            draft.permissions.canDeleteRelease = true;
          })}
          includeCreateAmendmentControls
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
      screen.queryByText('View this release', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText('Edit this release', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Amend this release',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Cancel amendment',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText('View this release amendment', {
        selector: 'a',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('Edit this release amendment', {
        selector: 'a',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('View original release', {
        selector: 'a',
      }),
    ).toBeInTheDocument();
  });

  test('handles creating amendments ok', async () => {
    const history = createMemoryHistoryWithMockedPush();

    render(
      <Router history={history}>
        <NonScheduledReleaseSummary
          release={produce(testRelease, draft => {
            draft.permissions.canMakeAmendmentOfRelease = true;
          })}
          includeCreateAmendmentControls
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

    // Now click the "Amend this release" button and check that the warning modal appears.
    userEvent.click(
      screen.getByRole('button', {
        name: 'Amend this release',
      }),
    );

    expect(
      screen.getByText('Confirm you want to amend this live release'),
    ).toBeInTheDocument();

    // Confirm the amending of the Release.
    releaseService.createReleaseAmendment.mockResolvedValue({
      id: 'release-amendment-id',
    } as ReleaseSummary);

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

    expect(history.push).toBeCalledWith(
      `/publication/${testRelease.publicationId}/release/release-amendment-id/summary`,
    );
  });

  test('handles cancelling amendments ok', async () => {
    const amendmentCancelledCallback = jest.fn();

    render(
      <MemoryRouter>
        <NonScheduledReleaseSummary
          release={produce(testRelease, draft => {
            draft.permissions.canDeleteRelease = true;
            draft.amendment = true;
            draft.previousVersionId = 'rel-2';
          })}
          includeCreateAmendmentControls
          onAmendmentCancelled={amendmentCancelledCallback}
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
      releaseId: testRelease.id,
      methodologiesScheduledWithRelease: [
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
      expect(amendmentCancelledCallback).toHaveBeenCalled();
    });

    expect(
      screen.queryByText('Confirm you want to cancel this amended release'),
    ).not.toBeInTheDocument();
  });
});
