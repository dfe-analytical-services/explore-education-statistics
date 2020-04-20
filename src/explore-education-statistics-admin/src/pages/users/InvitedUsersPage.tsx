import ButtonText from '@common/components/ButtonText';
import Link from '@admin/components/Link';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Page from '@admin/components/Page';
import userService from '@admin/services/users/service';
import { UserStatus } from '@admin/services/users/types';
import React, { useEffect, useState } from 'react';
import { useErrorControl } from '@common/contexts/ErrorControlContext';

interface Model {
  users: UserStatus[];
}

const PendingInvitesPage = () => {
  const [model, setModel] = useState<Model>();
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [errorStatus, setErrorStatus] = useState<number>();
  const { withoutErrorHandling } = useErrorControl();

  const getPendingInvites = () => {
    setIsLoading(true);
    withoutErrorHandling(() =>
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
        .then(() => setIsLoading(false)),
    );
  };

  useEffect(() => {
    getPendingInvites();
  }, []);

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
