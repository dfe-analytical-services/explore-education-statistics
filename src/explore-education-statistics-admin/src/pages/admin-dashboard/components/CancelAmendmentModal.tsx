import { IdTitlePair } from '@admin/services/types/common';
import ModalConfirm from '@common/components/ModalConfirm';
import React, { ReactNode } from 'react';

interface Props {
  scheduledMethodologies?: IdTitlePair[];
  triggerButton: ReactNode;
  onCancel: () => void;
  onConfirm: () => void;
}

const CancelAmendmentModal = ({
  scheduledMethodologies,
  triggerButton,
  onCancel,
  onConfirm,
}: Props) => {
  return (
    <ModalConfirm
      title="Confirm you want to cancel this amended release"
      triggerButton={triggerButton}
      onCancel={onCancel}
      onConfirm={onConfirm}
      onExit={onCancel}
    >
      <p>
        By cancelling the amendments you will lose any changes made, and the
        original release will remain unchanged.
      </p>
      {scheduledMethodologies && scheduledMethodologies.length > 0 && (
        <>
          <p>
            The following methodologies are scheduled to be published with this
            amended release:
          </p>
          <ul>
            {scheduledMethodologies?.map(methodology => (
              <li key={methodology.id}>{methodology.title}</li>
            ))}
          </ul>
        </>
      )}
    </ModalConfirm>
  );
};

export default CancelAmendmentModal;
