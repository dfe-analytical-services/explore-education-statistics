import _userService, {
  RemoveUser,
  UserStatus,
} from '@admin/services/userService';
import { MemoryRouter } from 'react-router';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import render from '@common-test/render';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { waitFor } from '@testing-library/dom';
import BauUsersPage from '../BauUsersPage';

jest.mock('@admin/services/userService');

const userService = _userService as jest.Mocked<typeof _userService>;

const user: UserStatus[] = [
  {
    id: '1',
    name: 'TestUser1',
    email: 'test@hotmail.com',
    role: 'test',
  },
];

const removedUser: RemoveUser = {
  userId: '1',
};

describe('BauUsersPage', () => {
  test('renders delete action when user a user is present', async () => {
    userService.getUsers.mockResolvedValue(user);

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Delete')).toBeInTheDocument();
    });
  });

  test('calls user service when delete user button is clicked', async () => {
    userService.getUsers.mockResolvedValue(user);
    userService.deleteUser.mockResolvedValue(Promise.resolve(removedUser));
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Delete')).toBeInTheDocument();
    });
    await userEvent.click(screen.getByRole('button', { name: 'Delete' }));

    await waitFor(() => {
      expect(
        screen.getByText('Confirm you want to delete this user'),
      ).toBeInTheDocument();
    });
    await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

    expect(userService.deleteUser).toHaveBeenCalled();
  });

  function renderPage() {
    return render(
      <MemoryRouter>
        <TestConfigContextProvider>
          <BauUsersPage />
        </TestConfigContextProvider>
      </MemoryRouter>,
    );
  }
});
