import ModalConfirm from '@common/components/ModalConfirm';
import React, { ReactNode } from 'react';

interface Props {
  triggerButton: ReactNode;
  onConfirm: () => void;
}

const DeleteDraftModal = ({ triggerButton, onConfirm }: Props) => {
  return (
    <ModalConfirm
      title="Confirm you want to delete this draft release"
      triggerButton={triggerButton}
      onConfirm={onConfirm}
    >
      <p>
        By deleting this draft release you will lose any changes made. This
        action cannot be reversed.
      </p>
    </ModalConfirm>
  );
};

export default DeleteDraftModal;
