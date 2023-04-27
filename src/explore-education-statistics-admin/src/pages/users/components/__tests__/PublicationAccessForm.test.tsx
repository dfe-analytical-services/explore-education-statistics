import PublicationAccessForm from '@admin/pages/users/components/PublicationAccessForm';
import { User } from '@admin/services/userService';
import {
  render,
  screen,
  waitFor,
  fireEvent,
  within,
} from '@testing-library/react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';
import { IdTitlePair } from '@admin/services/types/common';
import React from 'react';

describe('PublicationAccessForm', () => {
  const testUser: User = {
    id: 'user-guid-1',
    name: 'Florian Schneider',
    email: 'test@test.com',
    role: 'role-guid-1',
    userPublicationRoles: [
      {
        id: 'pr-id-1',
        publication: 'pub-1',
        role: 'Viewer',
        userName: 'Analyst1 User1',
        email: 'analyst1@example.com',
      },
      {
        id: 'pr-id-2',
        publication: 'pub-2',
        role: 'Owner',
        userName: 'Analyst2 User2',
        email: 'analyst2@example.com',
      },
    ],
    userReleaseRoles: [],
  };

  const testRoles: string[] = ['Contributer', 'Owner', 'Viewer'];

  const testPublications: IdTitlePair[] = [
    {
      id: 'publication-guid-1',
      title: 'title 1',
    },
    {
      id: 'publication-guid-2',
      title: 'title 2',
    },
  ];

  test('renders the form', () => {
    render(
      <PublicationAccessForm
        publications={testPublications}
        publicationRoles={testRoles}
        user={testUser}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    const publicationSelect = screen.getByLabelText('Publication');
    const publications = within(publicationSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(publications).toHaveLength(2);
    expect(publications[0]).toHaveTextContent(testPublications[0].title);
    expect(publications[1]).toHaveTextContent(testPublications[1].title);

    const roleSelect = screen.getByLabelText('Publication role');
    const roles = within(roleSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(roles).toHaveLength(3);
    expect(roles[0]).toHaveTextContent(testRoles[0]);
    expect(roles[1]).toHaveTextContent(testRoles[1]);
    expect(roles[2]).toHaveTextContent(testRoles[2]);

    expect(
      screen.getByRole('button', { name: 'Add publication access' }),
    ).toBeInTheDocument();
  });

  test('can submit the form with the selected publication and role', async () => {
    const handleSubmit = jest.fn();
    render(
      <PublicationAccessForm
        publications={testPublications}
        publicationRoles={testRoles}
        user={testUser}
        onRemove={noop}
        onSubmit={handleSubmit}
      />,
    );

    fireEvent.change(screen.getByLabelText('Publication'), {
      target: { value: testPublications[1].id },
    });
    fireEvent.change(screen.getByLabelText('Publication role'), {
      target: { value: testRoles[2] },
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Add publication access' }),
    );

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(
        {
          selectedPublicationId: testPublications[1].id,
          selectedPublicationRole: testRoles[2],
        },
        expect.anything(),
      );
    });
  });

  test('can remove a role', async () => {
    const handleRemove = jest.fn();
    render(
      <PublicationAccessForm
        publications={testPublications}
        publicationRoles={testRoles}
        user={testUser}
        onRemove={handleRemove}
        onSubmit={noop}
      />,
    );

    const removeButtons = screen.getAllByRole('button', { name: 'Remove' });

    userEvent.click(removeButtons[0]);

    await waitFor(() => {
      expect(handleRemove).toHaveBeenCalledWith(
        testUser.userPublicationRoles[0],
      );
    });
  });

  test('displays a table of publications', () => {
    render(
      <PublicationAccessForm
        publications={testPublications}
        publicationRoles={testRoles}
        user={testUser}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(3);
    expect(rows[1]).toHaveTextContent(
      testUser.userPublicationRoles[0].publication,
    );
    expect(rows[1]).toHaveTextContent(testUser.userPublicationRoles[0].role);
  });
});
