import styles from '@admin/pages/release/pre-release/components/PreReleaseInvitePlanModal.module.scss';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import React from 'react';
import { PreReleaseInvitePlan } from '@admin/services/preReleaseUserService';

interface Props {
  invitePlan: PreReleaseInvitePlan;
  isReleaseApproved: boolean;
  onConfirm: () => void;
  onCancel: () => void;
  onExit: () => void;
}

const PreReleaseInvitePlanModel = ({
  invitePlan,
  isReleaseApproved,
  onConfirm,
  onCancel,
  onExit,
}: Props) => {
  if (!invitePlan) {
    return null;
  }

  return (
    <ModalConfirm
      title="Confirm pre-release invitations"
      open
      onConfirm={onConfirm}
      onExit={onExit}
      onCancel={onCancel}
    >
      <p>
        Are you sure you want to invite these email addresses to access the
        pre-release?
      </p>

      {invitePlan.invitable.length > 0 ? (
        <>
          <WarningMessage>
            Email notifications will be sent{' '}
            {isReleaseApproved
              ? 'immediately.'
              : 'when the release is approved for publication.'}
          </WarningMessage>
          <div className={styles.invitesOverflow}>
            <ul className="govuk-list govuk-list--bullet govuk-!-margin-2">
              {invitePlan.invitable.map(email => (
                <li key={email}>
                  <p>{email}</p>
                </li>
              ))}
            </ul>
          </div>
        </>
      ) : (
        <WarningMessage>
          There are no email addresses that can be invited to access the
          pre-release.
        </WarningMessage>
      )}

      {invitePlan.alreadyAccepted.length > 0 && (
        <>
          <h2 className="govuk-heading-m">Already accepted</h2>
          <p>
            The following email addresses will be ignored as they are already
            accepted to access the pre-release:
          </p>
          <div className={styles.invitesOverflow}>
            <ul className="govuk-list govuk-list--bullet govuk-!-margin-2">
              {invitePlan.alreadyAccepted.map(email => (
                <li key={email}>
                  <p>{email}</p>
                </li>
              ))}
            </ul>
          </div>
        </>
      )}

      {invitePlan.alreadyInvited.length > 0 && (
        <>
          <h2 className="govuk-heading-m">Already invited</h2>
          <p>
            The following email addresses will be ignored as they are already
            invited to access the pre-release:
          </p>
          <div className={styles.invitesOverflow}>
            <ul className="govuk-list govuk-list--bullet govuk-!-margin-2">
              {invitePlan.alreadyInvited.map(email => (
                <li key={email}>
                  <p>{email}</p>
                </li>
              ))}
            </ul>
          </div>
        </>
      )}
    </ModalConfirm>
  );
};

export default PreReleaseInvitePlanModel;
