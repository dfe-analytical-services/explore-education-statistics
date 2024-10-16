import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import userService from '@admin/services/userService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import styles from './BauUsersPage.module.scss';

const BauUsersPage = () => {
  const { value, isLoading } = useAsyncRetry(() => userService.getUsers());

  // const handleDeleteUser = async (userEmail: string) => { // EES-5573
  //   await userService
  //     .deleteUser(userEmail)
  //     .then(() => {
  //       window.location.reload();
  //     })
  //     .catch(error => {
  //       logger.info(`Error encountered when deleting the user - ${error}`);
  //     });
  // };

  return (
    <Page
      title="Users"
      caption="Manage access to the service"
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Users' },
      ]}
    >
      <LoadingSpinner loading={isLoading} text="Loading users">
        <table>
          <caption>Active user accounts</caption>
          <thead>
            <tr>
              <th scope="col">Name</th>
              <th scope="col">Email</th>
              <th>Role</th>
              <th>Actions</th>
            </tr>
          </thead>
          {value && (
            <tbody>
              {value.map(user => (
                <tr key={user.id}>
                  <th scope="row">{user.name}</th>
                  <td>{user.email}</td>
                  <td>{user.role ?? 'No role'}</td>
                  <td>
                    <Link
                      className={styles.manageUserLink}
                      to={`/administration/users/${user.id}`}
                    >
                      Manage
                    </Link>
                    {/* EES-5573 */}
                    {/* <DeleteUserModal
                      triggerButton={
                        <ButtonText
                          className={styles.deleteUserButton}
                        >
                          Delete
                        </ButtonText>
                      }
                      onConfirm={async () => await handleDeleteUser(user.email)}
                    /> */}
                  </td>
                </tr>
              ))}
            </tbody>
          )}
        </table>
      </LoadingSpinner>
      <p>
        <Link to="/administration/" className="govuk-back-link">
          Back
        </Link>
      </p>
    </Page>
  );
};

export default BauUsersPage;
