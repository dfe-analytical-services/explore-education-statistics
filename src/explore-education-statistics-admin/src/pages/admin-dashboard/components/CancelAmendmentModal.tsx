import React from 'react';
import ModalConfirm from '@common/components/ModalConfirm';
import { IdTitlePair } from '@admin/services/types/common';

interface Props {
  scheduledMethodologies: IdTitlePair[];
  onCancel: () => void;
  onConfirm: () => void;
}

const CancelAmendmentModal = ({
  scheduledMethodologies,
  onCancel,
  onConfirm,
}: Props) => {
  return (
    <ModalConfirm
      open
      title="Confirm you want to cancel this amended release"
      onCancel={onCancel}
      onConfirm={onConfirm}
      onExit={onCancel}
    >
      <p>
        By cancelling the amendments you will lose any changes made, and the
        original release will remain unchanged.
      </p>
      {scheduledMethodologies.length > 0 && (
        <>
          <p>
            The following methodologies are scheduled to be published with this
            amended release:
          </p>
          <ul>
            {scheduledMethodologies.map(methodology => (
              <li key={methodology.id}>{methodology.title}</li>
            ))}
          </ul>
        </>
      )}
    </ModalConfirm>
  );
};

export default CancelAmendmentModal;
