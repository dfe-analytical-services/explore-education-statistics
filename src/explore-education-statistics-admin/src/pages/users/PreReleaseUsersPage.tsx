import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import userService from '@admin/services/userService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';

const PreReleaseUsersPage = () => {
  const { value, isLoading } = useAsyncRetry(() =>
    userService.getPreReleaseUsers(),
  );

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Pre-release users' },
      ]}
      title="Pre-release users"
    >
      <h1 className="govuk-heading-xl">
        <span className="govuk-caption-xl">Manage pre-release users</span>
        Pre-release users
      </h1>
      <table>
        <caption className="govuk-table__caption">
          Users with pre-release access
        </caption>
        <thead>
          <tr>
            <th scope="col">Name</th>
            <th scope="col">Email</th>
            <th scope="col">Actions</th>
          </tr>
        </thead>
        <LoadingSpinner loading={isLoading} text="Loading pre-release users">
          {value && (
            <tbody>
              {value.map(user => (
                <tr key={user.id}>
                  <th scope="row">{user.name}</th>
                  <td>{user.email}</td>
                  <td>
                    <Link to={`/administration/users/${user.id}`}>Manage</Link>
                    {/* <ButtonText onClick={removeAccessHander}>Remove</ButtonText> */}
                  </td>
                </tr>
              ))}
            </tbody>
          )}
        </LoadingSpinner>
      </table>
      <p>
        <Link to="/administration/" className="govuk-back-link">
          Back
        </Link>
      </p>
    </Page>
  );
};

export default PreReleaseUsersPage;
