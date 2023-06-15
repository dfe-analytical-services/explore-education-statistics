import { render, screen, waitFor } from '@testing-library/react';
import PublicationReleaseContributorsForm from '@admin/pages/publication/components/PublicationReleaseContributorsForm';
import _releasePermissionService, {
  UserReleaseRole,
} from '@admin/services/releasePermissionService';
import userEvent from '@testing-library/user-event';
import React from 'react';

jest.mock('@admin/services/releasePermissionService');

describe('PublicationReleaseContributorsForm', () => {
  const testPublicationContributors: UserReleaseRole[] = [
    {
      userId: 'user-1',
      userDisplayName: 'User Name 1',
      userEmail: 'user1@test.com',
      role: 'Contributor',
    },
    {
      userId: 'user-2',
      userDisplayName: 'User Name 2',
      userEmail: 'user2@test.com',
      role: 'Contributor',
    },
    {
      userId: 'user-3',
      userDisplayName: 'User Name 3',
      userEmail: 'user3@test.com',
      role: 'Contributor',
    },
    {
      userId: 'user-4',
      userDisplayName: 'User Name 4',
      userEmail: 'user4@test.com',
      role: 'Contributor',
    },
  ];
  const testReleaseContributors: UserReleaseRole[] = [
    {
      userId: 'user-1',
      userDisplayName: 'User Name 1',
      userEmail: 'user1@test.com',
      role: 'Contributor',
    },
    {
      userId: 'user-3',
      userDisplayName: 'User Name 3',
      userEmail: 'user3@test.com',
      role: 'Contributor',
    },
  ];
  test('submits correct userIds to backend', async () => {
    render(
      <PublicationReleaseContributorsForm
        publicationId="publication-id"
        releaseId="release-id"
        publicationContributors={testPublicationContributors}
        releaseContributors={testReleaseContributors}
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

    userEvent.click(checkboxes[0]);
    userEvent.click(checkboxes[1]);

    await waitFor(() => {
      expect(checkboxes[0].checked).toBe(false);
      expect(checkboxes[1].checked).toBe(true);
      expect(checkboxes[2].checked).toBe(true);
      expect(checkboxes[3].checked).toBe(false);
    });

    expect(
      _releasePermissionService.updateReleaseContributors,
    ).not.toHaveBeenCalled();

    userEvent.click(
      screen.getByRole('button', { name: 'Update contributors' }),
    );

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
