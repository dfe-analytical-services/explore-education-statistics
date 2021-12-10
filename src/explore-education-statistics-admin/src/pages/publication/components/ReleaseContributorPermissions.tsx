import { ManageAccessPageContributor } from '@admin/services/releasePermissionService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import styles from '@admin/pages/publication/components/ReleaseContributorPermissions.module.scss';
import React, { useState } from 'react';

export interface Props {
  contributors: ManageAccessPageContributor[];
  onUserRemove: (userId: string) => void;
}

const ReleaseContributorPermissions = ({
  contributors,
  onUserRemove,
}: Props) => {
  const [removeUser, setRemoveUser] = useState<ManageAccessPageContributor>();

  return (
    <>
      {!contributors || !contributors.length ? (
        <WarningMessage testId="releaseContributors-warning">
          There are currently no team members associated with this publication.
          You can invite new users by clicking the "Add or remove users" button
          or by going to the <a href="#invite-users">invite users tab</a>.
        </WarningMessage>
      ) : (
        <>
          <table>
            <tbody>
              {contributors.map(contributor => (
                <tr key={contributor.userId}>
                  <td className="govuk-!-width-one-half">
                    {contributor.userDisplayName} ({contributor.userEmail})
                  </td>
                  <td className={styles.control}>
                    <ButtonText onClick={() => setRemoveUser(contributor)}>
                      Remove user
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
        </>
      )}
    </>
  );
};

export default ReleaseContributorPermissions;
