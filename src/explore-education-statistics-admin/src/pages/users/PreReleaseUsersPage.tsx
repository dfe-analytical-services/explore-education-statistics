import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import userService from '@admin/services/userService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import ButtonText from '@common/components/ButtonText';
import styles from '@admin/pages/bau/BauUsersPage.module.scss';
import ModalConfirm from '@common/components/ModalConfirm';
import logger from '@common/services/logger';

const PreReleaseUsersPage = () => {
  const { value, isLoading } = useAsyncRetry(() =>
    userService.getPreReleaseUsers(),
  );

  const handleDeleteUser = async (userEmail: string) => {
    await userService
      .deleteUser(userEmail)
      .then(() => {
        window.location.reload();
      })
      .catch(error => {
        logger.info(`Error encountered when deleting the user - ${error}`);
      });
  };

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
                    <Link
                      className={styles.manageUserLink}
                      to={`/administration/users/${user.id}`}
                    >
                      Manage
                    </Link>
                    <ModalConfirm
                      title="Confirm you want to delete this user"
                      triggerButton={
                        <ButtonText className={styles.deleteUserButton}>
                          Delete
                        </ButtonText>
                      }
                      onConfirm={() => handleDeleteUser(user.email)}
                    >
                      <p>
                        By deleting this User you will remove all access and
                        permissions they have on the service. This change cannot
                        be reversed. Users who are removed and then need access
                        at a later point will need to be re-invited to the
                        service as a new user.
                      </p>
                    </ModalConfirm>
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
