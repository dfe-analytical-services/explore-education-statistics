import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import Modal from '@common/components/Modal';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode, useEffect } from 'react';

interface Props {
  children?: ReactNode;
  className?: string;
  cancelText?: string;
  confirmText?: string;
  open?: boolean;
  showCancel?: boolean;
  title: string;
  triggerButton?: ReactNode;
  underlayClass?: string;
  onCancel?(): void;
  onConfirm(): void;
  onExit?(): void;
}

const ModalConfirm = ({
  cancelText = 'Cancel',
  children,
  className,
  confirmText = 'Confirm',
  open: initialOpen = false,
  showCancel = true,
  title,
  triggerButton,
  underlayClass,
  onCancel,
  onConfirm,
  onExit,
}: Props) => {
  const isMounted = useMountedRef();
  const [isDisabled, toggleDisabled] = useToggle(false);
  const [open, toggleOpen] = useToggle(initialOpen);

  useEffect(() => {
    toggleOpen(initialOpen);
  }, [initialOpen, toggleOpen]);

  const handleAction = (callback?: () => void) => async () => {
    if (!callback) {
      toggleOpen.off();
      return;
    }
    if (isDisabled || !isMounted.current) {
      return;
    }

    toggleDisabled.on();

    await callback();

    // Callback may finish after
    // component has been unmounted.
    if (isMounted.current) {
      toggleDisabled.off();
      toggleOpen.off();
    }
  };

  return (
    <Modal
      className={className}
      closeOnOutsideClick={!isDisabled}
      closeOnEsc={!isDisabled}
      open={open}
      title={title}
      triggerButton={triggerButton}
      underlayClass={underlayClass}
      onExit={onExit}
      onToggleOpen={toggleOpen}
    >
      {children}

      <ButtonGroup>
        {showCancel && (
          <Button
            variant="secondary"
            onClick={handleAction(onCancel)}
            disabled={isDisabled}
          >
            {cancelText}
          </Button>
        )}
        <Button onClick={handleAction(onConfirm)} disabled={isDisabled}>
          {confirmText}
        </Button>
      </ButtonGroup>
    </Modal>
  );
};

export default ModalConfirm;
