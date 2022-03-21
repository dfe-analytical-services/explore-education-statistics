import {
  ContributorViewModel,
  ContributorInvite,
} from '@admin/services/releasePermissionService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import styles from '@admin/pages/publication/components/ReleaseContributorPermissions.module.scss';
import React, { useState } from 'react';
import Tag from '@common/components/Tag';

export interface Props {
  contributors: ContributorViewModel[];
  invites: ContributorInvite[];
  onUserRemove: (userId: string) => void;
  onUserInvitesRemove: (email: string) => void;
}

const ReleaseContributorPermissions = ({
  contributors,
  invites,
  onUserRemove,
  onUserInvitesRemove,
}: Props) => {
  const [removeUser, setRemoveUser] = useState<ContributorViewModel>();
  const [removeInvite, setRemoveInvite] = useState<ContributorInvite>();

  return (
    <>
      {contributors.length === 0 && invites.length === 0 ? (
        <WarningMessage testId="releaseContributors-warning">
          There are currently no team members or pending invites associated with
          this publication. You can invite new users by clicking the "Add or
          remove users" button or by going to the{' '}
          <a href="#invite-users">invite users tab</a>.
        </WarningMessage>
      ) : (
        <>
          <table>
            <tbody>
              {contributors.map(contributor => (
                <tr key={contributor.userId}>
                  <td className="govuk-!-width-one-half">
                    {`${contributor.userDisplayName} (${contributor.userEmail})`}
                  </td>
                  <td className={styles.control}>
                    <ButtonText onClick={() => setRemoveUser(contributor)}>
                      Remove user
                    </ButtonText>
                  </td>
                </tr>
              ))}
              {invites.map(invite => (
                <tr key={invite.email}>
                  <td className="govuk-!-width-one-half">
                    {invite.email}
                    <Tag className="govuk-!-margin-left-3">Pending Invite</Tag>
                  </td>
                  <td className={styles.control}>
                    <ButtonText onClick={() => setRemoveInvite(invite)}>
                      Cancel invite
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
              <strong>{removeUser?.userDisplayName}</strong> from all releases
              in this publication?
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
              publication for email address{' '}
              <strong>{removeInvite?.email}</strong>?
            </p>
          </ModalConfirm>
        </>
      )}
    </>
  );
};

export default ReleaseContributorPermissions;
