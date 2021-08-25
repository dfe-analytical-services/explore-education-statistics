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
      <table className="govuk-table">
        <caption className="govuk-table__caption">
          Users with pre-release access
        </caption>
        <thead className="govuk-table__head">
          <tr className="govuk-table__row">
            <th scope="col" className="govuk-table__header">
              Name
            </th>
            <th scope="col" className="govuk-table__header">
              Email
            </th>
            <th scope="col" className="govuk-table__header">
              Actions
            </th>
          </tr>
        </thead>
        <LoadingSpinner loading={isLoading} text="Loading pre-release users">
          {value && (
            <tbody className="govuk-table__body">
              {value.map(user => (
                <tr className="govuk-table__row" key={user.id}>
                  <th scope="row" className="govuk-table__header">
                    {user.name}
                  </th>
                  <td className="govuk-table__cell">{user.email}</td>
                  <td className="govuk-table__cell">
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
