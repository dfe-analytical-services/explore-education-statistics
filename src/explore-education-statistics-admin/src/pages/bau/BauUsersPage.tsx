import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import userService from '@admin/services/userService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';

const BauUsersPage = () => {
  const { value, isLoading } = useAsyncRetry(() => userService.getUsers());

  return (
    <Page
      title="Users"
      caption="Manage access to the service"
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Users' },
      ]}
    >
      <LoadingSpinner loading={isLoading} text="Loading users">
        <table>
          <caption>Active user accounts</caption>
          <thead>
            <tr>
              <th scope="col">Name</th>
              <th scope="col">Email</th>
              <th>Role</th>
              <th>Actions</th>
            </tr>
          </thead>
          {value && (
            <tbody>
              {value.map(user => (
                <tr key={user.id}>
                  <th scope="row">{user.name}</th>
                  <td>{user.email}</td>
                  <td>{user.role ?? 'No role'}</td>
                  <td>
                    <Link to={`/administration/users/${user.id}`}>Manage</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          )}
        </table>
      </LoadingSpinner>
      <p>
        <Link to="/administration/" className="govuk-back-link">
          Back
        </Link>
      </p>
    </Page>
  );
};

export default BauUsersPage;
