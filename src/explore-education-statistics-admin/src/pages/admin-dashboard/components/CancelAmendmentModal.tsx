import ModalConfirm from '@common/components/ModalConfirm';
import React from 'react';

interface Props {
  onConfirm: () => void;
  onCancel: () => void;
}

const CancelAmendmentModal = ({ onConfirm, onCancel }: Props) => {
  return (
    <ModalConfirm
      title="Confirm you want to cancel this amended release"
      onConfirm={onConfirm}
      onExit={onCancel}
      onCancel={onCancel}
      mounted
    >
      <p>
        By cancelling the amendments you will lose any changes made, and the
        original release will remain unchanged.
      </p>
    </ModalConfirm>
  );
};

export default CancelAmendmentModal;
