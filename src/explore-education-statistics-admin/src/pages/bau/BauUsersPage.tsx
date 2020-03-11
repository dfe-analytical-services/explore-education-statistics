import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import { ErrorControlState } from '@admin/contexts/ErrorControlContext';
import withErrorControl from '@admin/hocs/withErrorControl';
import userService from '@admin/services/users/service';
import { UserStatus } from '@admin/services/users/types';
import React, { useEffect, useState } from 'react';

interface Model {
  users: UserStatus[];
}

const BauUsersPage = ({ handleApiErrors }: ErrorControlState) => {
  const [model, setModel] = useState<Model>();

  useEffect(() => {
    userService
      .getUsers()
      .then(users => {
        setModel({
          users,
        });
      })
      .catch(handleApiErrors);
  }, [handleApiErrors]);
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
              User
            </th>
            <th scope="col" className="govuk-table__header">
              Email
            </th>
          </tr>
        </thead>
        {model && (
          <tbody className="govuk-table__body">
            {model.users.map(user => (
              <tr className="govuk-table__row" key={user.id}>
                <th scope="row" className="govuk-table__header">
                  {user.name}
                </th>
                <td className="govuk-table__cell">{user.email}</td>
              </tr>
            ))}
          </tbody>
        )}
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
    </Page>
  );
};

export default withErrorControl(BauUsersPage);
