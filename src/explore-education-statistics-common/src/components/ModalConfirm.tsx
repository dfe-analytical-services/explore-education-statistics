import ButtonGroup from '@common/components/ButtonGroup';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode } from 'react';
import Button from './Button';
import Modal from './Modal';

interface Props {
  children?: ReactNode;
  className?: string;
  cancelText?: string;
  confirmText?: string;
  open?: boolean;
  onConfirm(): void;
  onCancel?(): void;
  onExit(): void;
  showCancel?: boolean;
  title: string;
  underlayClass?: string;
}

const ModalConfirm = ({
  children,
  className,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  open,
  onConfirm,
  onExit,
  onCancel = onExit,
  showCancel = true,
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
      className={className}
      closeOnOutsideClick={!isDisabled}
      closeOnEsc={!isDisabled}
      title={title}
      open={open}
      onExit={onExit}
      underlayClass={underlayClass}
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
