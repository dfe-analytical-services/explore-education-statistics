import ButtonText from '@common/components/ButtonText';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import userService from '@admin/services/users/service';
import { UserStatus } from '@admin/services/users/types';
import React, { useEffect, useState } from 'react';

interface Model {
  users: UserStatus[];
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

  const cancelInviteHandler = () => {};

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Users', link: '/administration/users' },
        { name: 'Pending invites' },
      ]}
    >
      <h1 className="govuk-heading-xl">
        <span className="govuk-caption-xl">Manage access to the service</span>
        Pending user invites
      </h1>

      <table className="govuk-table">
        <caption className="govuk-table__caption">Invited users</caption>
        <thead className="govuk-table__head">
          <tr className="govuk-table__row">
            <th scope="col" className="govuk-table__header">
              Email
            </th>
            <th scope="col" className="govuk-table__header">
              Role
            </th>
            <th scope="col" className="govuk-table__header">
              Actions
            </th>
          </tr>
        </thead>
        {model && (
          <tbody className="govuk-table__body">
            {model.users.map(user => (
              <tr className="govuk-table__row" key={user.email}>
                <td className="govuk-table__cell">{user.email}</td>
                <td className="govuk-table__cell">{user.role}</td>
                <td className="govuk-table__cell">
                  <ButtonText
                    onClick={() => {
                      userService.cancelInvite(user.email).then(() => {
                        userService.getPendingInvites().then(updatedInvites =>
                          setModel({
                            users: updatedInvites,
                          }),
                        );
                      });
                    }}
                  >
                    Cancel invite
                  </ButtonText>
                </td>
              </tr>
            ))}
          </tbody>
        )}
      </table>
      <Link to="/administration/users/invite" className="govuk-button">
        Invite a new user
      </Link>
      <p>
        <Link to="/administration/users/" className="govuk-back-link">
          Back
        </Link>
      </p>
    </Page>
  );
};

export default PendingInvitesPage;
