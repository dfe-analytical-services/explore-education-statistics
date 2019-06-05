import React, { ReactNode } from 'react';
import Button from './Button';
import Modal from './Modal';

interface Props {
  children?: ReactNode;
  cancelText?: string;
  confirmText?: string;
  mounted?: boolean;
  onConfirm(): void;
  onCancel?(): void;
  onExit(): void;
  title: string;
}

const ModalConfirm = ({
  children,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  mounted,
  onConfirm,
  onCancel,
  onExit,
  title,
}: Props) => {
  return (
    <Modal title={title} onExit={onExit} mounted={mounted}>
      {children}

      <Button onClick={onConfirm}>{confirmText}</Button>
      <Button variant="secondary" onClick={onCancel}>
        {cancelText}
      </Button>
    </Modal>
  );
};

export default ModalConfirm;
