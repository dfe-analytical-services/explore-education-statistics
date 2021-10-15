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
      email: 'test1@education.gov.uk',
    },
    {
      email: 'test2@education.gov.uk',
    },
  ];

  test('renders correctly with list of users', async () => {
    preReleaseUserService.getUsers.mockResolvedValue(testUsers);

    render(<PreReleaseUserAccessForm releaseId="release-1" />);

    await waitFor(() => {
      const rows = screen.getAllByRole('row');
      expect(rows).toHaveLength(3);

      const headerCells = within(rows[0]).getAllByRole('columnheader');
      expect(headerCells[0]).toHaveTextContent('User email');

      const row1Cells = within(rows[1]).getAllByRole('cell');

      expect(row1Cells[0]).toHaveTextContent('test1@education.gov.uk');

      const row2Cells = within(rows[2]).getAllByRole('cell');

      expect(row2Cells[0]).toHaveTextContent('test2@education.gov.uk');
    });
  });

  test('renders empty message when there are no users', async () => {
    preReleaseUserService.getUsers.mockResolvedValue([]);

    render(<PreReleaseUserAccessForm releaseId="release-1" />);

    await waitFor(() => {
      expect(screen.queryByRole('table')).not.toBeInTheDocument();
      expect(
        screen.getByText('No pre-release users have been invited.'),
      ).toBeInTheDocument();
    });
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
      expect(screen.queryByRole('table')).not.toBeInTheDocument();
      expect(
        screen.getByText('Could not load pre-release users'),
      ).toBeInTheDocument();
    });
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
      await userEvent.type(
        emailsTextarea,
        `test@education.gov.uk{enter}`.repeat(50),
      );
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.queryAllByText(
            'Enter between 1 and 50 lines of email addresses',
          ).length,
        ).toBe(0);
      });

      // now exceed the limit
      await userEvent.type(emailsTextarea, `test@education.gov.uk`);
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter between 1 and 50 lines of email addresses', {
            selector: '#preReleaseUserAccessForm-emails-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when emails contains badly formatted values', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        'test@education.gov.uk{enter}not a valid email',
      );
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter only @education.gov.uk email addresses', {
            selector: '#preReleaseUserAccessForm-emails-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when emails contains non @education.gov.uk values', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        'test@education.gov.uk{enter}email@example.com',
      );
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.queryByText('Enter only @education.gov.uk email addresses', {
            selector: '#preReleaseUserAccessForm-emails-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when email format has more than one @', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        'test@education.gov.uk@test',
      );
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter only @education.gov.uk email addresses', {
            selector: '#preReleaseUserAccessForm-emails-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('submitting form with invalid values shows validation messages', async () => {
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

    test('whitespace is trimmed and blank lines are filtered without causing validation errors', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new users by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new users by email'),
        ' {enter} {enter} test1@education.gov.uk {enter} {enter} test2@education.gov.uk {enter} {enter} test3@education.gov.uk {enter} ',
      );

      userEvent.click(screen.getByRole('button', { name: 'Invite new users' }));

      await waitFor(() => {
        expect(
          preReleaseUserService.getInvitePlan,
        ).toHaveBeenCalledWith('release-1', [
          'test1@education.gov.uk',
          'test2@education.gov.uk',
          'test3@education.gov.uk',
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
        'test1@education.gov.uk{enter}test2@education.gov.uk{enter}test3@education.gov.uk',
      );

      preReleaseUserService.getInvitePlan.mockResolvedValue({
        alreadyAccepted: [
          'existing.prerelease.user.1@education.gov.uk',
          'existing.prerelease.user.2@education.gov.uk',
        ],
        alreadyInvited: [
          'invited.prerelease.1@education.gov.uk',
          'invited.prerelease.2@education.gov.uk',
        ],
        invitable: [
          'test1@education.gov.uk',
          'test2@education.gov.uk',
          'test3@education.gov.uk',
        ],
      });

      userEvent.click(screen.getByRole('button', { name: 'Invite new users' }));

      await waitFor(() => {
        expect(
          preReleaseUserService.getInvitePlan,
        ).toHaveBeenCalledWith('release-1', [
          'test1@education.gov.uk',
          'test2@education.gov.uk',
          'test3@education.gov.uk',
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

      expect(invitableListItems[0]).toHaveTextContent('test1@education.gov.uk');
      expect(invitableListItems[1]).toHaveTextContent('test2@education.gov.uk');
      expect(invitableListItems[2]).toHaveTextContent('test3@education.gov.uk');

      const acceptedList = modal.getByRole('list', {
        name: 'Already accepted',
      });
      const acceptedListItems = within(acceptedList).getAllByRole('listitem');
      expect(acceptedListItems).toHaveLength(2);

      expect(acceptedListItems[0]).toHaveTextContent(
        'existing.prerelease.user.1@education.gov.uk',
      );
      expect(acceptedListItems[1]).toHaveTextContent(
        'existing.prerelease.user.2@education.gov.uk',
      );

      const invitedList = modal.getByRole('list', {
        name: 'Already invited',
      });
      const invitedListItems = within(invitedList).getAllByRole('listitem');
      expect(invitedListItems).toHaveLength(2);

      expect(invitedListItems[0]).toHaveTextContent(
        'invited.prerelease.1@education.gov.uk',
      );
      expect(invitedListItems[1]).toHaveTextContent(
        'invited.prerelease.2@education.gov.uk',
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
        'test@education.gov.uk',
      );

      preReleaseUserService.getInvitePlan.mockResolvedValue({
        invitable: ['test@education.gov.uk'],
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
        'test@education.gov.uk',
      );

      preReleaseUserService.getInvitePlan.mockResolvedValue({
        invitable: ['test@education.gov.uk'],
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

      await waitFor(() => {
        expect(
          modal.getByText('Email notifications will be sent immediately.'),
        ).toBeInTheDocument();
      });
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
        'test3@education.gov.uk{enter}test4@education.gov.uk{enter}test5@education.gov.uk',
      );

      preReleaseUserService.getInvitePlan.mockResolvedValue({
        invitable: ['test1@education.gov.uk'],
        alreadyAccepted: [],
        alreadyInvited: [],
      });

      userEvent.click(screen.getByRole('button', { name: 'Invite new users' }));

      preReleaseUserService.inviteUsers.mockResolvedValue([
        { email: 'test3@education.gov.uk' },
        { email: 'test4@education.gov.uk' },
        { email: 'test5@education.gov.uk' },
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
          'test3@education.gov.uk',
          'test4@education.gov.uk',
          'test5@education.gov.uk',
        ]);
      });

      expect(
        screen.queryByText('Confirm pre-release invitations'),
      ).not.toBeInTheDocument();

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

      const rows = screen.getAllByRole('row');
      expect(rows).toHaveLength(6);

      const row1Cells = within(rows[1]).getAllByRole('cell');
      expect(row1Cells[0]).toHaveTextContent('test1@education.gov.uk');

      const row2Cells = within(rows[2]).getAllByRole('cell');
      expect(row2Cells[0]).toHaveTextContent('test2@education.gov.uk');

      const row3Cells = within(rows[3]).getAllByRole('cell');
      expect(row3Cells[0]).toHaveTextContent('test3@education.gov.uk');

      const row4Cells = within(rows[4]).getAllByRole('cell');
      expect(row4Cells[0]).toHaveTextContent('test4@education.gov.uk');

      const row5Cells = within(rows[5]).getAllByRole('cell');
      expect(row5Cells[0]).toHaveTextContent('test5@education.gov.uk');
    });
  });

  describe('removing user', () => {
    test('clicking Remove button removes user from the list', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(screen.getByRole('table')).toBeInTheDocument();
      });

      let rows = screen.getAllByRole('row');
      expect(rows).toHaveLength(3);

      userEvent.click(within(rows[2]).getByRole('button', { name: 'Remove' }));

      await waitFor(() => {
        rows = screen.getAllByRole('row');
        expect(rows).toHaveLength(2);

        const row1Cells = within(rows[1]).getAllByRole('cell');

        expect(row1Cells[0]).toHaveTextContent('test1@education.gov.uk');
      });
    });
  });
});
