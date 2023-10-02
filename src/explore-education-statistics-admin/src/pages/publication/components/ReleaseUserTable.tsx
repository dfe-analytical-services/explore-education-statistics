import {
  UserReleaseRole,
  UserReleaseInvite,
} from '@admin/services/releasePermissionService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import styles from '@admin/pages/publication/components/ReleaseUserTable.module.scss';
import React from 'react';
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
  return (
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
                <ModalConfirm
                  title="Confirm user removal"
                  triggerButton={
                    <ButtonText variant="warning">
                      Remove
                      <VisuallyHidden> {user.userDisplayName}</VisuallyHidden>
                    </ButtonText>
                  }
                  onConfirm={async () => {
                    await onUserRemove(user.userId);
                  }}
                >
                  <p>
                    Are you sure you want to remove{' '}
                    <strong>{user?.userDisplayName}</strong> from all releases
                    in this publication?
                  </p>
                </ModalConfirm>
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
                <ModalConfirm
                  title="Confirm cancelling of user invites"
                  triggerButton={
                    <ButtonText variant="warning">
                      Cancel invite
                      <VisuallyHidden>{` for ${invite.email}`}</VisuallyHidden>
                    </ButtonText>
                  }
                  onConfirm={async () => {
                    await onUserInvitesRemove(invite.email);
                  }}
                >
                  <p>
                    Are you sure you want to cancel all invites to releases
                    under this publication for email address{' '}
                    <strong>{invite?.email}</strong>?
                  </p>
                </ModalConfirm>
              </td>
            )}
          </tr>
        ))}
      </tbody>
    </table>
  );
};

export default ReleaseUserTable;
