import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import userInvitesService, {
  PendingInvite,
} from '@admin/services/user-management/userInvitesService';
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
    userInvitesService
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
      <Link to="/administration/users/invites/create" className="govuk-button">
        Invite a new user
      </Link>
      <p>
        <Link to="/administration/" className="govuk-back-link">
          Back
        </Link>
      </p>

      {errorStatus && errorStatus === 404 ? (
        'There are currently no pending user invites'
      ) : (
        <LoadingSpinner loading={isLoading} text="Loading invited users">
          <div className="table-container">
            <table>
              <caption className="govuk-table__caption">Invited users</caption>
              <thead>
                <tr>
                  <th scope="col">Email</th>
                  <th scope="col">Role</th>
                  <th scope="col">Pre-release Roles</th>
                  <th scope="col">Publication Roles</th>
                  <th scope="col">Actions</th>
                </tr>
              </thead>
              {model && (
                <tbody>
                  {model.pendingInvites.map(pendingInvite => (
                    <tr key={pendingInvite.email}>
                      <td>{pendingInvite.email}</td>
                      <td>{pendingInvite.role}</td>
                      <td>
                        {pendingInvite.userPreReleaseRoles.length === 0 ? (
                          'No user pre-release roles'
                        ) : (
                          <ul className="govuk-!-margin-0">
                            {pendingInvite.userPreReleaseRoles.map(
                              preReleaseRole => {
                                return (
                                  <li key={preReleaseRole.id}>
                                    {preReleaseRole.publication}
                                    <ul>
                                      <li>{preReleaseRole.release}</li>
                                    </ul>
                                  </li>
                                );
                              },
                            )}
                          </ul>
                        )}
                      </td>
                      <td>
                        {pendingInvite.userPublicationRoles.length === 0 ? (
                          'No user publication roles'
                        ) : (
                          <ul className="govuk-!-margin-0">
                            {pendingInvite.userPublicationRoles.map(
                              publicationRole => {
                                return (
                                  <li key={publicationRole.id}>
                                    {`${publicationRole.publication} - ${publicationRole.role}`}
                                  </li>
                                );
                              },
                            )}
                          </ul>
                        )}
                      </td>
                      <td>
                        <ButtonText
                          onClick={() => {
                            userInvitesService
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
          </div>
        </LoadingSpinner>
      )}
    </Page>
  );
};

export default InvitedUsersPage;
