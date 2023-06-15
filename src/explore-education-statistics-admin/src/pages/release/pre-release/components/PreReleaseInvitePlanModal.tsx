import styles from '@admin/pages/release/pre-release/components/PreReleaseInvitePlanModal.module.scss';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import { PreReleaseInvitePlan } from '@admin/services/preReleaseUserService';
import React from 'react';

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
            {`Email notifications will be sent ${
              isReleaseApproved
                ? 'immediately.'
                : 'when the release is approved for publication.'
            }`}
          </WarningMessage>
          <div className={styles.invitesOverflow}>
            <ul className="govuk-!-margin-2" data-testid="invitableList">
              {invitePlan.invitable?.map(email => (
                <li key={email}>{email}</li>
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
          <h2 id="already-accepted-heading" className="govuk-heading-m">
            Already accepted
          </h2>
          <p>
            The following email addresses will be ignored as they are already
            accepted to access the pre-release:
          </p>
          <div className={styles.invitesOverflow}>
            <ul
              aria-labelledby="already-accepted-heading"
              className="govuk-!-margin-2"
              data-testid="acceptedList"
            >
              {invitePlan.alreadyAccepted.map(email => (
                <li key={email}>{email}</li>
              ))}
            </ul>
          </div>
        </>
      )}

      {invitePlan.alreadyInvited.length > 0 && (
        <>
          <h2 id="already-invited-heading" className="govuk-heading-m">
            Already invited
          </h2>
          <p>
            The following email addresses will be ignored as they are already
            invited to access the pre-release:
          </p>
          <div className={styles.invitesOverflow}>
            <ul
              aria-labelledby="already-invited-heading"
              className="govuk-!-margin-2"
              data-testid="invitedList"
            >
              {invitePlan.alreadyInvited.map(email => (
                <li key={email}>{email}</li>
              ))}
            </ul>
          </div>
        </>
      )}
    </ModalConfirm>
  );
};

export default PreReleaseInvitePlanModel;
