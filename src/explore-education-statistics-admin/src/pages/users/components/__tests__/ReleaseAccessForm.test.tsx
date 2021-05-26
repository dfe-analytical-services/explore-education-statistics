import ReleaseAccessForm from '@admin/pages/users/components/ReleaseAccessForm';
import { User } from '@admin/services/userService';
import {
  render,
  screen,
  waitFor,
  fireEvent,
  within,
} from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';
import { IdTitlePair } from 'src/services/types/common';

describe('ReleaseAccessForm', () => {
  const testUser: User = {
    id: 'user-guid-1',
    name: 'Florian Schneider',
    email: 'test@test.com',
    role: 'role-guid-1',
    userPublicationRoles: [],
    userReleaseRoles: [
      {
        id: 'rr-id-1',
        publication: 'pub-1',
        release: 'release-1',
        role: 'Viewer',
      },
      {
        id: 'rr-id-2',
        publication: 'pub-2',
        release: 'release-2',
        role: 'Approver',
      },
    ],
  };

  const testRoles: string[] = ['Approver', 'Contributer', 'Viewer'];

  const testReleases: IdTitlePair[] = [
    {
      id: 'release-guid-1',
      title: 'title 1',
    },
    {
      id: 'release-guid-2',
      title: 'title 2',
    },
  ];

  test('renders the form', () => {
    render(
      <ReleaseAccessForm
        releases={testReleases}
        releaseRoles={testRoles}
        user={testUser}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    const releaseSelect = screen.getByLabelText('Release');
    const releases = within(releaseSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(releases).toHaveLength(2);
    expect(releases[0]).toHaveTextContent(testReleases[0].title);
    expect(releases[1]).toHaveTextContent(testReleases[1].title);

    const roleSelect = screen.getByLabelText('Release role');
    const roles = within(roleSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(roles).toHaveLength(3);
    expect(roles[0]).toHaveTextContent(testRoles[0]);
    expect(roles[1]).toHaveTextContent(testRoles[1]);
    expect(roles[2]).toHaveTextContent(testRoles[2]);

    expect(
      screen.getByRole('button', { name: 'Add release access' }),
    ).toBeInTheDocument();
  });

  test('can submit the form with the selected release and role', async () => {
    const handleSubmit = jest.fn();
    render(
      <ReleaseAccessForm
        releases={testReleases}
        releaseRoles={testRoles}
        user={testUser}
        onRemove={noop}
        onSubmit={handleSubmit}
      />,
    );

    fireEvent.change(screen.getByLabelText('Release'), {
      target: { value: testReleases[1].id },
    });
    fireEvent.change(screen.getByLabelText('Release role'), {
      target: { value: testRoles[2] },
    });

    userEvent.click(screen.getByRole('button', { name: 'Add release access' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(
        {
          selectedReleaseId: testReleases[1].id,
          selectedReleaseRole: testRoles[2],
        },
        expect.anything(),
      );
    });
  });

  test('can remove a role', async () => {
    const handleRemove = jest.fn();
    render(
      <ReleaseAccessForm
        releases={testReleases}
        releaseRoles={testRoles}
        user={testUser}
        onRemove={handleRemove}
        onSubmit={noop}
      />,
    );

    const removeButtons = screen.getAllByRole('button', { name: 'Remove' });

    userEvent.click(removeButtons[0]);

    await waitFor(() => {
      expect(handleRemove).toHaveBeenCalledWith(testUser.userReleaseRoles[0]);
    });
  });

  test('displays a table of releases', () => {
    render(
      <ReleaseAccessForm
        releases={testReleases}
        releaseRoles={testRoles}
        user={testUser}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(3);
    expect(rows[1]).toHaveTextContent(testUser.userReleaseRoles[0].publication);
    expect(rows[1]).toHaveTextContent(testUser.userReleaseRoles[0].release);
    expect(rows[1]).toHaveTextContent(testUser.userReleaseRoles[0].role);
  });
});
