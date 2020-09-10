import _preReleaseUserService, {
  PreReleaseUser,
} from '@admin/services/preReleaseUserService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import PreReleaseUserAccessForm from '../PreReleaseUserAccessForm';

const preReleaseUserService = _preReleaseUserService as jest.Mocked<
  typeof _preReleaseUserService
>;

jest.mock('@admin/services/preReleaseUserService');

describe('PreReleaseUserAccessForm', () => {
  const testUsers: PreReleaseUser[] = [
    {
      email: 'test1@test.com',
      invited: true,
    },
    {
      email: 'test2@test.com',
      invited: false,
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
      expect(headerCells[1]).toHaveTextContent('Invited?');

      const row1Cells = within(rows[1]).getAllByRole('cell');

      expect(row1Cells[0]).toHaveTextContent('test1@test.com');
      expect(row1Cells[1]).toHaveTextContent('Yes');

      const row2Cells = within(rows[2]).getAllByRole('cell');

      expect(row2Cells[0]).toHaveTextContent('test2@test.com');
      expect(row2Cells[1]).toHaveTextContent('No');
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

  describe('inviting new user', () => {
    test('shows validation message when there is no email', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new user by email'),
        ).toBeInTheDocument();
      });

      userEvent.click(screen.getByLabelText('Invite new user by email'));
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter an email address', {
            selector: '#preReleaseUserAccessForm-email-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('shows validation message when email is not valid', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new user by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new user by email'),
        'not a valid email',
      );
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Enter a valid email address', {
            selector: '#preReleaseUserAccessForm-email-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('submitting form with invalid values shows validation messages', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new user by email'),
        ).toBeInTheDocument();
      });

      userEvent.click(screen.getByRole('button', { name: 'Invite new user' }));

      await waitFor(() => {
        expect(
          screen.getByText('Enter an email address', {
            selector: '#preReleaseUserAccessForm-email-error',
          }),
        ).toBeInTheDocument();
      });
    });

    test('submitting form successfully adds newly invited user to list', async () => {
      preReleaseUserService.getUsers.mockResolvedValue(testUsers);

      render(<PreReleaseUserAccessForm releaseId="release-1" />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('Invite new user by email'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Invite new user by email'),
        'test3@test.com',
      );

      preReleaseUserService.inviteUser.mockResolvedValue({
        email: 'test3@test.com',
        invited: true,
      });

      userEvent.click(screen.getByRole('button', { name: 'Invite new user' }));

      await waitFor(() => {
        const rows = screen.getAllByRole('row');
        expect(rows).toHaveLength(4);

        const row1Cells = within(rows[1]).getAllByRole('cell');

        expect(row1Cells[0]).toHaveTextContent('test1@test.com');
        expect(row1Cells[1]).toHaveTextContent('Yes');

        const row2Cells = within(rows[2]).getAllByRole('cell');

        expect(row2Cells[0]).toHaveTextContent('test2@test.com');
        expect(row2Cells[1]).toHaveTextContent('No');

        const row3Cells = within(rows[3]).getAllByRole('cell');

        expect(row3Cells[0]).toHaveTextContent('test3@test.com');
        expect(row3Cells[1]).toHaveTextContent('Yes');
      });
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

        expect(row1Cells[0]).toHaveTextContent('test1@test.com');
        expect(row1Cells[1]).toHaveTextContent('Yes');
      });
    });
  });
});
