import Link from '@admin/components/Link';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Page from '@admin/components/Page';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import userService from '@admin/services/users/service';
import { UserStatus } from '@admin/services/users/types';
import React, { useEffect, useState } from 'react';

const BauUsersPage = () => {
  const { value, isLoading, error } = useAsyncRetry(() =>
    userService.getUsers(),
  );

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Users' },
      ]}
    >
      <h1 className="govuk-heading-xl">
        <span className="govuk-caption-xl">Manage access to the service</span>
        Users
      </h1>
      <table className="govuk-table">
        <caption className="govuk-table__caption">Active user accounts</caption>
        <thead className="govuk-table__head">
          <tr className="govuk-table__row">
            <th scope="col" className="govuk-table__header">
              Name
            </th>
            <th scope="col" className="govuk-table__header">
              Email
            </th>
            <th>Role</th>
            <th>Actions</th>
          </tr>
        </thead>
        <LoadingSpinner loading={isLoading} text="Loading users">
          {value && (
            <tbody className="govuk-table__body">
              {value.map(user => (
                <tr className="govuk-table__row" key={user.id}>
                  <th scope="row" className="govuk-table__header">
                    {user.name}
                  </th>
                  <td className="govuk-table__cell">{user.email}</td>
                  <td className="govuk-table__cell">{user.role}</td>
                  <td className="govuk-table__cell">
                    {/* <Link to={`/administration/users/${user.id}`}>Manage</Link> */}
                  </td>
                </tr>
              ))}
            </tbody>
          )}
        </LoadingSpinner>
      </table>
      <Link
        to="/administration/users/pending"
        className="govuk-button govuk-button--secondary"
      >
        View pending invites
      </Link>{' '}
      <Link to="/administration/users/invite" className="govuk-button">
        Invite a new user
      </Link>
      <p>
        <Link to="/administration/" className="govuk-back-link">
          Back
        </Link>
      </p>
    </Page>
  );
};

export default BauUsersPage;
