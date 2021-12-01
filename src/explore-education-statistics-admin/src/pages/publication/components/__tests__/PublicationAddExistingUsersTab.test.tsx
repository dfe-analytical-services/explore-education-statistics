import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import PublicationAddExistingUsersTab from '@admin/pages/publication/components/PublicationAddExistingUsersTab';
import _releasePermissionService, {
  ManageAccessPageContributor,
} from '@admin/services/releasePermissionService';

jest.mock('@admin/services/releasePermissionService');

describe('PublicationAddExistingUsersTab', () => {
  const testContributors: ManageAccessPageContributor[] = [
    {
      userId: 'user-1',
      userFullName: 'User Name 1',
      userEmail: 'user1@test.com',
      releaseRoleId: 'release-role-1',
    },
    {
      userId: 'user-2',
      userFullName: 'User Name 2',
      userEmail: 'user2@test.com',
      releaseRoleId: undefined,
    },
    {
      userId: 'user-3',
      userFullName: 'User Name 3',
      userEmail: 'user3@test.com',
      releaseRoleId: 'release-role-3',
    },
    {
      userId: 'user-4',
      userFullName: 'User Name 4',
      userEmail: 'user4@test.com',
      releaseRoleId: undefined,
    },
  ];
  test('Submits correct userIds to backend', async () => {
    render(
      <PublicationAddExistingUsersTab
        publicationId="publication-id"
        release={{
          id: 'release-id',
          title: 'Test release title',
          slug: '',
          approvalStatus: 'Approved',
          latestRelease: true,
          live: true,
          amendment: true,
          releaseName: '',
          publicationId: '',
          publicationTitle: '',
          publicationSlug: '',
          timePeriodCoverage: { value: '', label: '' },
          type: { id: '', title: '' },
          contact: {
            id: '',
            contactName: '',
            contactTelNo: '',
            teamEmail: '',
          },
          previousVersionId: '',
          preReleaseAccessList: '',
        }}
        contributors={testContributors}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByLabelText('User Name 1 (user1@test.com)'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('User Name 2 (user2@test.com)'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('User Name 3 (user3@test.com)'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('User Name 4 (user4@test.com)'),
      ).toBeInTheDocument();
    });

    const checkboxes = screen.getAllByLabelText(
      /User Name /,
    ) as HTMLInputElement[];
    expect(checkboxes).toHaveLength(4);

    expect(checkboxes[0].checked).toBe(true);
    expect(checkboxes[0]).toHaveAttribute('value', 'user-1');

    expect(checkboxes[1].checked).toBe(false);
    expect(checkboxes[1]).toHaveAttribute('value', undefined);

    expect(checkboxes[2].checked).toBe(true);
    expect(checkboxes[2]).toHaveAttribute('value', 'user-3');

    expect(checkboxes[3].checked).toBe(false);
    expect(checkboxes[3]).toHaveAttribute('value', undefined);

    fireEvent.click(checkboxes[0]);
    fireEvent.click(checkboxes[1]);

    await waitFor(() => {
      expect(checkboxes[0].checked).toBe(false);
      expect(checkboxes[1].checked).toBe(true);
      expect(checkboxes[2].checked).toBe(true);
      expect(checkboxes[3].checked).toBe(false);
    });

    expect(
      _releasePermissionService.updateReleaseContributors,
    ).toHaveBeenCalledTimes(0);

    fireEvent.click(screen.getByRole('button', { name: 'Update permissions' }));

    await waitFor(() => {
      expect(
        _releasePermissionService.updateReleaseContributors,
      ).toHaveBeenCalledTimes(1);
      expect(
        _releasePermissionService.updateReleaseContributors,
      ).toHaveBeenCalledWith(
        'release-id',
        expect.arrayContaining(['user-2', 'user-3']),
      );
    });
  });
});
