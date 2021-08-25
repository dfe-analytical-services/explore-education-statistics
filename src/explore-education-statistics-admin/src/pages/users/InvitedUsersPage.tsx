import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import userService, { UserStatus } from '@admin/services/userService';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { useCallback, useEffect, useState } from 'react';

interface Model {
  users: UserStatus[];
}

const InvitedUsersPage = () => {
  const [model, setModel] = useState<Model>();
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [errorStatus, setErrorStatus] = useState<number>();

  const getPendingInvites = useCallback(() => {
    setIsLoading(true);
    userService
      .getInvitedUsers()
      .then(updatedInvites => {
        setModel({
          users: updatedInvites,
        });
      })
      .catch(error => {
        setErrorStatus(error.response.status);
      })
      .then(() => setIsLoading(false));
  }, []);

  useEffect(() => {
    getPendingInvites();
  }, [getPendingInvites]);

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Pending invites' },
      ]}
      title="Pending invites"
    >
      <h1 className="govuk-heading-xl">
        <span className="govuk-caption-xl">Manage access to the service</span>
        Pending user invites
      </h1>

      {errorStatus && errorStatus === 404 ? (
        <p>There are currently no pending user invites</p>
      ) : (
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
          <LoadingSpinner loading={isLoading} text="Loading invited users">
            {model && (
              <tbody className="govuk-table__body">
                {model.users.map(user => (
                  <tr className="govuk-table__row" key={user.email}>
                    <td className="govuk-table__cell">{user.email}</td>
                    <td className="govuk-table__cell">{user.role}</td>
                    <td className="govuk-table__cell">
                      <ButtonText
                        onClick={() => {
                          userService
                            .cancelInvite(user.email)
                            .then(() => getPendingInvites());
                        }}
                      >
                        Cancel invite
                      </ButtonText>
                    </td>
                  </tr>
                ))}
              </tbody>
            )}
          </LoadingSpinner>
        </table>
      )}
      <Link to="/administration/users/invites/create" className="govuk-button">
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

export default InvitedUsersPage;
