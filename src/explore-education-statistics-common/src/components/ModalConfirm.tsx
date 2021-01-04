import ButtonGroup from '@common/components/ButtonGroup';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
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
  underlayClass?: string;
}

const ModalConfirm = ({
  children,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  mounted,
  onConfirm,
  onExit,
  onCancel = onExit,
  title,
  underlayClass,
}: Props) => {
  const isMounted = useMountedRef();
  const [isDisabled, toggleDisabled] = useToggle(false);

  const handleAction = (callback: () => void) => async () => {
    if (isDisabled || !isMounted.current) {
      return;
    }

    toggleDisabled.on();

    await callback();

    // Callback may finish after
    // component has been unmounted.
    if (isMounted.current) {
      toggleDisabled.off();
    }
  };

  return (
    <Modal
      focusDialog
      title={title}
      mounted={mounted}
      underlayClass={underlayClass}
      underlayClickExits={!isDisabled}
      escapeExits={!isDisabled}
      onExit={onExit}
    >
      {children}

      <ButtonGroup>
        <Button
          variant="secondary"
          onClick={handleAction(onCancel)}
          disabled={isDisabled}
        >
          {cancelText}
        </Button>
        <Button onClick={handleAction(onConfirm)} disabled={isDisabled}>
          {confirmText}
        </Button>
      </ButtonGroup>
    </Modal>
  );
};

export default ModalConfirm;
