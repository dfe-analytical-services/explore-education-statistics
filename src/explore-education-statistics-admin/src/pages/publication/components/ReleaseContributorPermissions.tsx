import {
  ManageAccessPageRelease,
  ManageAccessPageContributor,
} from '@admin/services/releasePermissionService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import Tag from '@common/components/Tag';
import Button from '@common/components/Button';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import styles from '@admin/pages/publication/components/ReleaseContributorPermissions.module.scss';
import userService from '@admin/services/userService';
import React, { useState } from 'react';

export interface Props {
  release: ManageAccessPageRelease;
  onChange: (
    release: ManageAccessPageRelease,
    removeUser: string | undefined,
  ) => void;
}

const ReleaseContributorPermissions = ({ release, onChange }: Props) => {
  const releaseContributors = release.userList;

  const [changeContributorAccess, setChangeContributorAccess] = useState<
    ManageAccessPageContributor
  >();

  const handleRemove = (userReleaseRoleId: string) => {
    onChange(release, undefined);
  };

  const handleAdd = (userId: string) => {
    onChange(release, undefined);
  };

  const handleGrantAll = () => {
    onChange(release, undefined);
  };

  const handleUserRemoval = (userId: string) => {
    onChange(release, userId);
  };

  const [
    grantAllContributorAccess,
    toggleGrantAllContributorAccess,
  ] = useToggle(false);

  const [removeUser, setRemoveUser] = useState<ManageAccessPageContributor>();

  const showGrantAllButton =
    releaseContributors &&
    releaseContributors?.length > 1 &&
    releaseContributors?.some(
      contributor => contributor.releaseRoleId === undefined,
    );

  return (
    <>
      {!releaseContributors || releaseContributors.length === 0 ? (
        <WarningMessage testId="releaseContributors-warning">
          There are currently no team members associated to this publication.
          <br />
          Please <a href="#invite-users">invite new users</a> to join.
        </WarningMessage>
      ) : (
        <>
          <table>
            <tbody>
              {releaseContributors?.map(contributor => (
                <tr key={contributor.userId}>
                  <td className="govuk-!-width-one-half">
                    {contributor.userFullName}
                  </td>
                  <td className={styles.control}>
                    <Tag colour={contributor.releaseRoleId ? 'grey' : 'red'}>
                      {contributor.releaseRoleId
                        ? 'Access granted'
                        : 'No access'}
                    </Tag>
                  </td>
                  <td className={styles.control}>
                    <Button
                      className="govuk-!-margin-bottom-0"
                      variant={
                        contributor.releaseRoleId ? 'warning' : 'secondary'
                      }
                      onClick={() => setChangeContributorAccess(contributor)}
                    >
                      {contributor.releaseRoleId
                        ? 'Remove access'
                        : 'Grant access'}
                    </Button>
                  </td>
                  <td className={styles.control}>
                    <ButtonText onClick={() => setRemoveUser(contributor)}>
                      Remove user
                    </ButtonText>
                  </td>
                </tr>
              ))}
              {showGrantAllButton && (
                <tr className={styles.grantAllRow}>
                  <td colSpan={2} />
                  <td className={styles.control}>
                    <Button
                      className="govuk-!-margin-bottom-0 govuk-!-margin-top-4"
                      onClick={toggleGrantAllContributorAccess.on}
                    >
                      Grant access to all
                    </Button>
                  </td>
                  <td />
                </tr>
              )}
            </tbody>
          </table>

          <ModalConfirm
            title={`Change Access for ${changeContributorAccess?.userFullName}`}
            open={!!changeContributorAccess}
            onConfirm={async () => {
              if (!changeContributorAccess) {
                return;
              }
              if (changeContributorAccess?.releaseRoleId) {
                await userService.removeUserReleaseRole(
                  changeContributorAccess.releaseRoleId,
                );
                setChangeContributorAccess(undefined);
                handleRemove(changeContributorAccess.releaseRoleId);
                return;
              }
              await userService.addUserReleaseRole(
                changeContributorAccess.userId,
                {
                  releaseId: changeContributorAccess.releaseId,
                  releaseRole: 'Contributor',
                },
              );
              setChangeContributorAccess(undefined);
              handleAdd(
                changeContributorAccess.userId,
                changeContributorAccess.releaseId,
              );
            }}
            onCancel={() => setChangeContributorAccess(undefined)}
            onExit={() => setChangeContributorAccess(undefined)}
          >
            <p>
              Are you sure you want to{' '}
              <strong>
                {changeContributorAccess?.releaseRoleId ? 'remove' : 'grant'}{' '}
                access
              </strong>{' '}
              for <strong>{release.releaseTitle}</strong>?
            </p>
          </ModalConfirm>

          <ModalConfirm
            title="Grant access to all listed users"
            open={grantAllContributorAccess}
            onConfirm={async () => {
              toggleGrantAllContributorAccess.off();
              handleGrantAll();
            }}
            onCancel={toggleGrantAllContributorAccess.off}
            onExit={toggleGrantAllContributorAccess.off}
          >
            <p>Are you sure you want to grant access to all listed users?</p>
          </ModalConfirm>

          <ModalConfirm
            title="Confirm user removal"
            open={!!removeUser}
            onConfirm={async () => {
              handleUserRemoval(removeUser?.userId);
              setRemoveUser(undefined);
            }}
            onCancel={() => setRemoveUser(undefined)}
            onExit={() => setRemoveUser(undefined)}
          >
            <p>
              Are you sure you want to remove{' '}
              <strong>{removeUser?.userFullName}</strong> from all releases in
              this publication?
            </p>
          </ModalConfirm>
        </>
      )}
    </>
  );
};

export default ReleaseContributorPermissions;
