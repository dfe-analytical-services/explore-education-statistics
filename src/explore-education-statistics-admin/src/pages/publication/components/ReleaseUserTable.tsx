import {
  UserReleaseRole,
  UserReleaseInvite,
} from '@admin/services/releasePermissionService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import styles from '@admin/pages/publication/components/ReleaseUserTable.module.scss';
import React, { useState } from 'react';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';

interface Props {
  users: UserReleaseRole[];
  invites: UserReleaseInvite[];
  onUserRemove?: (userId: string) => void;
  onUserInvitesRemove?: (email: string) => void;
}

const ReleaseUserTable = ({
  users,
  invites,
  onUserRemove,
  onUserInvitesRemove,
}: Props) => {
  const [removeUser, setRemoveUser] = useState<UserReleaseRole>();
  const [removeInvite, setRemoveInvite] = useState<UserReleaseInvite>();

  return (
    <>
      <table>
        <thead>
          <tr>
            <th>Name</th>
            {(onUserRemove || onUserInvitesRemove) && (
              <th className={styles.actions}>Actions</th>
            )}
          </tr>
        </thead>
        <tbody>
          {users.map(user => (
            <tr key={user.userId}>
              <td>{`${user.userDisplayName} (${user.userEmail})`}</td>
              {onUserRemove && (
                <td>
                  <ButtonText
                    variant="warning"
                    onClick={() => setRemoveUser(user)}
                  >
                    Remove
                    <VisuallyHidden> {user.userDisplayName}</VisuallyHidden>
                  </ButtonText>
                </td>
              )}
            </tr>
          ))}
          {invites.map(invite => (
            <tr key={invite.email}>
              <td>
                {invite.email}
                <Tag className="govuk-!-margin-left-3">Pending Invite</Tag>
              </td>
              {onUserInvitesRemove && (
                <td>
                  <ButtonText
                    variant="warning"
                    onClick={() => setRemoveInvite(invite)}
                  >
                    Cancel invite
                    <VisuallyHidden>{` for ${invite.email}`}</VisuallyHidden>
                  </ButtonText>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>

      {onUserRemove && (
        <ModalConfirm
          title="Confirm user removal"
          open={!!removeUser}
          onConfirm={async () => {
            if (removeUser) {
              onUserRemove(removeUser.userId);
              setRemoveUser(undefined);
            }
          }}
          onCancel={() => setRemoveUser(undefined)}
          onExit={() => setRemoveUser(undefined)}
        >
          <p>
            Are you sure you want to remove{' '}
            <strong>{removeUser?.userDisplayName}</strong> from all releases in
            this publication?
          </p>
        </ModalConfirm>
      )}

      {onUserInvitesRemove && (
        <ModalConfirm
          title="Confirm cancelling of user invites"
          open={!!removeInvite}
          onConfirm={async () => {
            if (removeInvite) {
              onUserInvitesRemove(removeInvite.email);
              setRemoveInvite(undefined);
            }
          }}
          onCancel={() => setRemoveInvite(undefined)}
          onExit={() => setRemoveInvite(undefined)}
        >
          <p>
            Are you sure you want to cancel all invites to releases under this
            publication for email address <strong>{removeInvite?.email}</strong>
            ?
          </p>
        </ModalConfirm>
      )}
    </>
  );
};

export default ReleaseUserTable;
