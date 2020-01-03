import React, { useEffect, useState } from 'react';
import Page from '@admin/components/Page';
import Link from '@admin/components/Link';
import userService from '@admin/services/users/service';
import { UserStatus } from '@admin/services/users/types';

interface Model {
  users: string[];
}

const PendingInvitesPage = () => {
  const [model, setModel] = useState<Model>();

  useEffect(() => {
    userService.getPendingInvites().then(users => {
      setModel({
        users,
      });
    });
  }, []);
  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Pending invites' },
      ]}
    >
      <h1 className="govuk-heading-xl">
        <span className="govuk-caption-xl">Manage access to the service</span>
        Pending invites
      </h1>

      <table className="govuk-table">
        <caption className="govuk-table__caption">Invited users</caption>
        <thead className="govuk-table__head">
          <tr className="govuk-table__row">
            <th scope="col" className="govuk-table__header">
              Email
            </th>
          </tr>
        </thead>
        {model && (
          <tbody className="govuk-table__body">
            {model.users.map(user => (
              <tr className="govuk-table__row" key={user}>
                <td className="govuk-table__cell">{user}</td>
              </tr>
            ))}
          </tbody>
        )}
      </table>
      <Link to="/administration/users/invite" className="govuk-button">
        Invite a new user
      </Link>
    </Page>
  );
};

export default PendingInvitesPage;
