import { useConfirmContext } from '@common/context/ConfirmContext';
import React, { ReactNode } from 'react';
import Button from './Button';
import Modal from './Modal';

interface Props {
  children?: ReactNode;
  cancelText?: string;
  confirmText?: string;
  title: string;
}

const ModalConfirm = ({
  children,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  title,
}: Props) => {
  const { isConfirming, confirm, cancel } = useConfirmContext();

  return (
    <Modal title={title} onExit={() => cancel()} mounted={isConfirming}>
      {children}

      <Button
        onClick={() => {
          confirm();
        }}
      >
        {confirmText}
      </Button>
      <Button
        variant="secondary"
        onClick={() => {
          cancel();
        }}
      >
        {cancelText}
      </Button>
    </Modal>
  );
};

export default ModalConfirm;
