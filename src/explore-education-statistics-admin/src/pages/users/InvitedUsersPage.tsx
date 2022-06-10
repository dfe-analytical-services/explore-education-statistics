import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import userService, { PendingInvite } from '@admin/services/userService';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { useCallback, useEffect, useState } from 'react';

interface Model {
  pendingInvites: PendingInvite[];
}

const InvitedUsersPage = () => {
  const [model, setModel] = useState<Model>();
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [errorStatus, setErrorStatus] = useState<number>();

  const getPendingInvites = useCallback(() => {
    setIsLoading(true);
    userService
      .getPendingInvites()
      .then(updatedInvites => {
        setModel({
          pendingInvites: updatedInvites,
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
      caption="Manage invites to the service"
    >
      {errorStatus && errorStatus === 404 ? (
        <p>There are currently no pending user invites</p>
      ) : (
        <LoadingSpinner loading={isLoading} text="Loading invited users">
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
                  Release Roles
                </th>
                <th scope="col" className="govuk-table__header">
                  Publication Roles
                </th>
                <th scope="col" className="govuk-table__header">
                  Actions
                </th>
              </tr>
            </thead>
            {model && (
              <tbody className="govuk-table__body">
                {model.pendingInvites.map(pendingInvite => (
                  <tr className="govuk-table__row" key={pendingInvite.email}>
                    <th className="govuk-table__cell">{pendingInvite.email}</th>
                    <td className="govuk-table__cell">{pendingInvite.role}</td>
                    <td className="govuk-table__cell">
                      {pendingInvite.userReleaseRoles.length === 0 ? (
                        <p className="govuk-!-margin-0">
                          No user release roles
                        </p>
                      ) : (
                        <ul className="govuk-!-margin-0">
                          {pendingInvite.userReleaseRoles.map(releaseRole => {
                            return (
                              <li
                                key={`${releaseRole.publication}_${releaseRole.release}_${releaseRole.role}`}
                              >
                                {releaseRole.publication}
                                <ul>
                                  <li>{releaseRole.release}</li>
                                  <li>{releaseRole.role}</li>
                                </ul>
                              </li>
                            );
                          })}
                        </ul>
                      )}
                    </td>
                    <td className="govuk-table__cell">
                      {pendingInvite.userPublicationRoles.length === 0 ? (
                        <p className="govuk-!-margin-0">
                          No user publication roles
                        </p>
                      ) : (
                        <ul className="govuk-!-margin-0">
                          {pendingInvite.userPublicationRoles.map(
                            publicationRole => {
                              return (
                                <li
                                  key={`${publicationRole.publication}_${publicationRole.role}`}
                                >
                                  {`${publicationRole.publication} - ${publicationRole.role}`}
                                </li>
                              );
                            },
                          )}
                        </ul>
                      )}
                    </td>
                    <td className="govuk-table__cell">
                      <ButtonText
                        onClick={() => {
                          userService
                            .cancelInvite(pendingInvite.email)
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
          </table>
        </LoadingSpinner>
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
