import {
  ContributorViewModel,
  ContributorInvite,
} from '@admin/services/releasePermissionService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import styles from '@admin/pages/publication/components/ReleaseContributorTable.module.scss';
import React, { useState } from 'react';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';

interface Props {
  contributors: ContributorViewModel[];
  invites: ContributorInvite[];
  onUserRemove: (userId: string) => void;
  onUserInvitesRemove: (email: string) => void;
}

const ReleaseContributorTable = ({
  contributors,
  invites,
  onUserRemove,
  onUserInvitesRemove,
}: Props) => {
  const [removeUser, setRemoveUser] = useState<ContributorViewModel>();
  const [removeInvite, setRemoveInvite] = useState<ContributorInvite>();

  return (
    <>
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th className={styles.actions}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {contributors.map(contributor => (
            <tr key={contributor.userId}>
              <td>
                {`${contributor.userDisplayName} (${contributor.userEmail})`}
              </td>
              <td>
                <ButtonText
                  variant="warning"
                  onClick={() => setRemoveUser(contributor)}
                >
                  Remove
                  <VisuallyHidden>
                    {' '}
                    {contributor.userDisplayName}
                  </VisuallyHidden>
                </ButtonText>
              </td>
            </tr>
          ))}
          {invites.map(invite => (
            <tr key={invite.email}>
              <td>
                {invite.email}
                <Tag className="govuk-!-margin-left-3">Pending Invite</Tag>
              </td>
              <td>
                <ButtonText
                  variant="warning"
                  onClick={() => setRemoveInvite(invite)}
                >
                  Cancel invite
                  <VisuallyHidden>{` for ${invite.email}`}</VisuallyHidden>
                </ButtonText>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

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
          publication for email address <strong>{removeInvite?.email}</strong>?
        </p>
      </ModalConfirm>
    </>
  );
};

export default ReleaseContributorTable;
