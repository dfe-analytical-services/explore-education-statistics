import { ManageAccessPageContributor } from '@admin/services/releasePermissionService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import styles from '@admin/pages/publication/components/ReleaseContributorPermissions.module.scss';
import React, { useState } from 'react';
import Tag from '@common/components/Tag';

export interface Props {
  contributors: ManageAccessPageContributor[];
  pendingInviteEmails: string[];
  onUserRemove: (userId: string) => void;
  onUserInvitesRemove: (email: string) => void;
}

const ReleaseContributorPermissions = ({
  contributors,
  pendingInviteEmails,
  onUserRemove,
  onUserInvitesRemove,
}: Props) => {
  const [removeUser, setRemoveUser] = useState<ManageAccessPageContributor>();
  const [removeInvitesEmail, setRemoveInvitesEmail] = useState<string>();

  return (
    <>
      {(!contributors || !contributors.length) &&
      (!pendingInviteEmails || !pendingInviteEmails.length) ? (
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
              {pendingInviteEmails.map(email => (
                <tr key={email}>
                  <td className="govuk-!-width-one-half">
                    {email}
                    <Tag className="govuk-!-margin-left-3">Pending Invite</Tag>
                  </td>
                  <td className={styles.control}>
                    <ButtonText onClick={() => setRemoveInvitesEmail(email)}>
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
            open={!!removeInvitesEmail}
            onConfirm={async () => {
              if (removeInvitesEmail) {
                onUserInvitesRemove(removeInvitesEmail);
                setRemoveInvitesEmail(undefined);
              }
            }}
            onCancel={() => setRemoveInvitesEmail(undefined)}
            onExit={() => setRemoveInvitesEmail(undefined)}
          >
            <p>
              Are you sure you want to cancel all invites to releases under this
              publication for email address{' '}
              <strong>{removeInvitesEmail}</strong>?
            </p>
          </ModalConfirm>
        </>
      )}
    </>
  );
};

export default ReleaseContributorPermissions;
