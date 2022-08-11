import _preReleaseUserService, {
  PreReleaseUser,
} from '@admin/services/preReleaseUserService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import PreReleaseUserAccessForm from '@admin/pages/release/pre-release/components/PreReleaseUserAccessForm';

const preReleaseUserService = _preReleaseUserService as jest.Mocked<
  typeof _preReleaseUserService
>;

jest.mock('@admin/services/preReleaseUserService');

describe('PreReleaseUserAccessForm', () => {
  const testUsers: PreReleaseUser[] = [
    {
      email: 'test1@test.com',
    },
    {
      email: 'test2@test.com',
    },
  ];

  test('renders correctly with list of users', async () => {
    preReleaseUserService.getUsers.mockResolvedValue(testUsers);

    render(<PreReleaseUserAccessForm releaseId="release-1" />);

    await waitFor(() => {
      expect(
        screen.getByText(
          'These people will have access to a preview of the release 24 hours before the scheduled publish date.',
        ),
      ).toBeInTheDocument();
    });

    expect(screen.getByRole('table')).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    const headerCells = within(rows[0]).getAllByRole('columnheader');
    expect(headerCells[0]).toHaveTextContent('User email');

    const row1Cells = within(rows[1]).getAllByRole('cell');

    expect(row1Cells[0]).toHaveTextContent('test1@test.com');

    const row2Cells = within(rows[2]).getAllByRole('cell');

    expect(row2Cells[0]).toHaveTextContent('test2@test.com');
  });

  test('renders empty message when there are no users', async () => {
    preReleaseUserService.getUsers.mockResolvedValue([]);

    render(<PreReleaseUserAccessForm releaseId="release-1" />);

    await waitFor(() => {
      expect(
        screen.getByText('No pre-release users have been invited.'),
      ).toBeInTheDocument();
    });

    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  test('renders correctly when the release is live', async () => {
    preReleaseUserService.getUsers.mockResolvedValue(testUsers);

    render(<PreReleaseUserAccessForm releaseId="release-1" isReleaseLive />);

    await waitFor(() => {
      expect(
        screen.getByText(
          'This release has been published and can no longer be updated.',
        ),
      ).toBeInTheDocument();

      expect(
        screen.queryByLabelText('Invite new user by email'),
      ).not.toBeInTheDocument();

      const rows = screen.getAllByRole('row');
      expect(rows).toHaveLength(3);

      expect(
        screen.queryByRole('button', { name: 'Remove' }),
      ).not.toBeInTheDocument();
    });
  });

  test('renders error message if users could not be loaded', async () => {
    preReleaseUserService.getUsers.mockRejectedValue(
      new Error('Something went wrong'),
    );

    render(<PreReleaseUserAccessForm releaseId="release-1" />);

    await waitFor(() => {
      expect(
        screen.getByText('Could not load pre-release users'),
      ).toBeInTheDocument();
    });

    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  describe('inviting new users', () => {
    test('shows validation message when there are no email values', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      userEvent.click(screen.getByLabelText('Invite new users by email'));
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter 1 or more email addresses', {
            selector: '#preReleaseUserAccessForm-emails-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when the number of email lines exceeds the upper limit', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      const emailsTextarea = screen.getByLabelText('Invite new users by email');
      // type values up to but not exceeding the limit of lines
      await userEvent.type(emailsTextarea, `test@test.com{enter}`.repeat(50));
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.queryAllByText(
            'Enter between 1 and 50 lines of email addresses',
          ).length,
        ).toBe(0);
      });

      // now exceed the limit
      await userEvent.type(emailsTextarea, `test@test.com`);
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter between 1 and 50 lines of email addresses', {
            selector: '#preReleaseUserAccessForm-emails-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when emails contains invalid values', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        'test@test.com{enter}invalid-1{enter}invalid-2',
      );
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText("'invalid-1' is not a valid email address", {
            selector: '#preReleaseUserAccessForm-emails-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when email has more than one @', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        'test@test.com@test',
      );
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText(
            "'test@test.com@test' is not a valid email address",
            {
              selector: '#preReleaseUserAccessForm-emails-error',
            },
          ),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when email has invalid domain', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        'test@test.',
      );
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText("'test@test.' is not a valid email address", {
            selector: '#preReleaseUserAccessForm-emails-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('submitting form with no values shows a validation error', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      userEvent.click(screen.getByRole('button', { name: 'Invite new users' }));

      await waitFor(() => {
        expect(
          screen.getByText('Enter 1 or more email addresses', {
            selector: '#preReleaseUserAccessForm-emails-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('submitting form with invalid values shows a validation error', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        'test@test.com{enter}invalid-1{enter}invalid-2',
      );

      userEvent.click(screen.getByRole('button', { name: 'Invite new users' }));

      await waitFor(() => {
        expect(
          screen.getByText("'invalid-1' is not a valid email address", {
            selector: '#preReleaseUserAccessForm-emails-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('whitespace is trimmed and blank lines are filtered without causing a validation error', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        ' {enter} {enter} test1@test.com {enter} {enter} test2@test.com {enter} {enter} test3@test.com {enter} ',
      );

      userEvent.click(screen.getByRole('button', { name: 'Invite new users' }));

      await waitFor(() => {
        expect(
          preReleaseUserService.getInvitePlan,
        ).toHaveBeenCalledWith('release-1', [
          'test1@test.com',
          'test2@test.com',
          'test3@test.com',
        ]);
      });
    });

    test('accepts a range of valid values without causing a validation error', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        "special_'%+-.characters@test.com{enter}" +
          'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.com{enter}' +
          'test@test.co.uk{enter}' +
          'test@test.uk{enter}' +
          'test@education.gov.uk',
      );
      userEvent.tab();

      userEvent.click(screen.getByRole('button', { name: 'Invite new users' }));

      await waitFor(() => {
        expect(
          preReleaseUserService.getInvitePlan,
        ).toHaveBeenCalledWith('release-1', [
          "special_'%+-.characters@test.com",
          'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.com',
          'test@test.co.uk',
          'test@test.uk',
          'test@education.gov.uk',
        ]);
      });
    });

    test('submitting the form opens confirmation modal with invite plan', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        'test1@test.com{enter}test2@test.com{enter}test3@test.com',
      );

      preReleaseUserService.getInvitePlan.mockResolvedValue({
        alreadyAccepted: [
          'existing.prerelease.user.1@test.com',
          'existing.prerelease.user.2@test.com',
        ],
        alreadyInvited: [
          'invited.prerelease.1@test.com',
          'invited.prerelease.2@test.com',
        ],
        invitable: ['test1@test.com', 'test2@test.com', 'test3@test.com'],
      });

      userEvent.click(screen.getByRole('button', { name: 'Invite new users' }));

      await waitFor(() => {
        expect(
          preReleaseUserService.getInvitePlan,
        ).toHaveBeenCalledWith('release-1', [
          'test1@test.com',
          'test2@test.com',
          'test3@test.com',
        ]);
      });

      await waitFor(() => {
        expect(
          screen.getByText('Confirm pre-release invitations'),
        ).toBeInTheDocument();
      });

      const modal = within(screen.getByRole('dialog'));

      expect(
        modal.getByRole('heading', { name: 'Confirm pre-release invitations' }),
      ).toBeInTheDocument();

      expect(
        modal.getByText(
          'Email notifications will be sent when the release is approved for publication.',
        ),
      ).toBeInTheDocument();

      const invitableList = modal.getByTestId('invitableList');
      const invitableListItems = within(invitableList).getAllByRole('listitem');
      expect(invitableListItems).toHaveLength(3);

      expect(invitableListItems[0]).toHaveTextContent('test1@test.com');
      expect(invitableListItems[1]).toHaveTextContent('test2@test.com');
      expect(invitableListItems[2]).toHaveTextContent('test3@test.com');

      const acceptedList = modal.getByRole('list', {
        name: 'Already accepted',
      });
      const acceptedListItems = within(acceptedList).getAllByRole('listitem');
      expect(acceptedListItems).toHaveLength(2);

      expect(acceptedListItems[0]).toHaveTextContent(
        'existing.prerelease.user.1@test.com',
      );
      expect(acceptedListItems[1]).toHaveTextContent(
        'existing.prerelease.user.2@test.com',
      );

      const invitedList = modal.getByRole('list', {
        name: 'Already invited',
      });
      const invitedListItems = within(invitedList).getAllByRole('listitem');
      expect(invitedListItems).toHaveLength(2);

      expect(invitedListItems[0]).toHaveTextContent(
        'invited.prerelease.1@test.com',
      );
      expect(invitedListItems[1]).toHaveTextContent(
        'invited.prerelease.2@test.com',
      );
    });

    test('cancelling the confirmation closes the modal', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        'test@test.com',
      );

      preReleaseUserService.getInvitePlan.mockResolvedValue({
        invitable: ['test@test.com'],
        alreadyAccepted: [],
        alreadyInvited: [],
      });

      userEvent.click(screen.getByRole('button', { name: 'Invite new users' }));

      await waitFor(() => {
        expect(
          screen.getByText('Confirm pre-release invitations'),
        ).toBeInTheDocument();
      });

      const modal = within(screen.getByRole('dialog'));

      userEvent.click(modal.getByRole('button', { name: 'Cancel' }));

      await waitFor(() => {
        expect(
          screen.queryByText('Confirm pre-release invitations'),
        ).not.toBeInTheDocument();
      });

      expect(preReleaseUserService.inviteUsers).not.toHaveBeenCalled();

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });

    test('confirmation modal displays correct notifications warning when release is approved', async () => {
      render(
        <PreReleaseUserAccessForm releaseId="release-1" isReleaseApproved />,
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        'test@test.com',
      );

      preReleaseUserService.getInvitePlan.mockResolvedValue({
        invitable: ['test@test.com'],
        alreadyAccepted: [],
        alreadyInvited: [],
      });

      userEvent.click(screen.getByRole('button', { name: 'Invite new users' }));

      await waitFor(() => {
        expect(
          screen.getByText('Confirm pre-release invitations'),
        ).toBeInTheDocument();
      });

      const modal = within(screen.getByRole('dialog'));

      expect(
        modal.getByText('Email notifications will be sent immediately.'),
      ).toBeInTheDocument();
    });

    test('accepting the confirmation modal adds newly invited users to list', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        'test3@test.com{enter}test4@test.com{enter}test5@test.com',
      );

      preReleaseUserService.getInvitePlan.mockResolvedValue({
        invitable: ['test1@test.com'],
        alreadyAccepted: [],
        alreadyInvited: [],
      });

      userEvent.click(screen.getByRole('button', { name: 'Invite new users' }));

      preReleaseUserService.inviteUsers.mockResolvedValue([
        { email: 'test3@test.com' },
        { email: 'test4@test.com' },
        { email: 'test5@test.com' },
      ]);

      await waitFor(() => {
        expect(
          screen.getByText('Confirm pre-release invitations'),
        ).toBeInTheDocument();
      });

      const modal = within(screen.getByRole('dialog'));

      userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(
          preReleaseUserService.inviteUsers,
        ).toHaveBeenCalledWith('release-1', [
          'test3@test.com',
          'test4@test.com',
          'test5@test.com',
        ]);
      });

      expect(
        screen.queryByText('Confirm pre-release invitations'),
      ).not.toBeInTheDocument();

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
      expect(screen.getByRole('table')).toBeInTheDocument();

      const rows = screen.getAllByRole('row');
      expect(rows).toHaveLength(6);

      const row1Cells = within(rows[1]).getAllByRole('cell');
      expect(row1Cells[0]).toHaveTextContent('test1@test.com');

      const row2Cells = within(rows[2]).getAllByRole('cell');
      expect(row2Cells[0]).toHaveTextContent('test2@test.com');

      const row3Cells = within(rows[3]).getAllByRole('cell');
      expect(row3Cells[0]).toHaveTextContent('test3@test.com');

      const row4Cells = within(rows[4]).getAllByRole('cell');
      expect(row4Cells[0]).toHaveTextContent('test4@test.com');

      const row5Cells = within(rows[5]).getAllByRole('cell');
      expect(row5Cells[0]).toHaveTextContent('test5@test.com');
    });
  });

  describe('removing user', () => {
    test('clicking Remove button removes user from the list', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByText(
            'These people will have access to a preview of the release 24 hours before the scheduled publish date.',
          ),
        ).toBeInTheDocument();
      });

      expect(screen.getByRole('table')).toBeInTheDocument();

      let rows = screen.getAllByRole('row');
      expect(rows).toHaveLength(3);

      userEvent.click(within(rows[2]).getByRole('button', { name: 'Remove' }));

      await waitFor(() => {
        expect(preReleaseUserService.removeUser).toHaveBeenCalledWith(
          'release-1',
          'test2@test.com',
        );
      });

      rows = screen.getAllByRole('row');
      expect(rows).toHaveLength(2);

      const row1Cells = within(rows[1]).getAllByRole('cell');

      expect(row1Cells[0]).toHaveTextContent('test1@test.com');
    });
  });
});
