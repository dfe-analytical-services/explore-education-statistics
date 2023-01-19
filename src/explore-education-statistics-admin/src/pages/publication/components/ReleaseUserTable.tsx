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
  'data-testid'?: string;
  invites: UserReleaseInvite[];
  users: UserReleaseRole[];
  onUserRemove?: (userId: string) => void;
  onUserInvitesRemove?: (email: string) => void;
}

const ReleaseUserTable = ({
  'data-testid': testId,
  invites,
  users,
  onUserRemove,
  onUserInvitesRemove,
}: Props) => {
  const [removeUser, setRemoveUser] = useState<UserReleaseRole>();
  const [removeInvite, setRemoveInvite] = useState<UserReleaseInvite>();

  return (
    <>
      <table data-testid={testId}>
        <thead>
          <tr>
            <th scope="col">Name</th>
            <th scope="col">Email</th>
            {(onUserRemove || onUserInvitesRemove) && (
              <th className={styles.actions} scope="col">
                Actions
              </th>
            )}
          </tr>
        </thead>
        <tbody>
          {users.map(user => (
            <tr key={user.userId}>
              <td>{user.userDisplayName}</td>
              <td>{user.userEmail}</td>
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
              <td />
              <td>
                {invite.email}
                <Tag className="govuk-!-margin-left-3">Pending invite</Tag>
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
              await onUserRemove(removeUser.userId);
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
              await onUserInvitesRemove(removeInvite.email);
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
