import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Modal from '@common/components/Modal';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode, useCallback, useEffect } from 'react';

interface Props {
  children?: ReactNode;
  className?: string;
  cancelText?: string;
  confirmText?: string;
  hiddenCancellingText?: string;
  hiddenConfirmingText?: string;
  open?: boolean;
  showCancel?: boolean;
  hideConfirm?: boolean;
  submitButtonVariant?: 'secondary' | 'warning';
  title: string;
  triggerButton?: ReactNode;
  underlayClass?: string;
  onCancel?(): void | Promise<void>;
  onConfirm(): void | Promise<void>;
  onExit?(): void | Promise<void>;
}

export default function ModalConfirm({
  cancelText = 'Cancel',
  children,
  className,
  confirmText = 'Confirm',
  hiddenCancellingText = 'Cancelling',
  hiddenConfirmingText = 'Confirming',
  open: initialOpen = false,
  showCancel = true,
  hideConfirm = false,
  submitButtonVariant,
  title,
  triggerButton,
  underlayClass,
  onExit,
  onCancel = onExit,
  onConfirm,
}: Props) {
  const isMounted = useMountedRef();

  const [isOpen, toggleOpen] = useToggle(initialOpen);
  const [isCancelling, toggleCancelling] = useToggle(false);
  const [isConfirming, toggleConfirming] = useToggle(false);

  const isCompleting = isCancelling || isConfirming;

  useEffect(() => {
    toggleOpen(initialOpen);
  }, [initialOpen, toggleOpen]);

  const handleCancel = useCallback(async () => {
    if (!onCancel) {
      toggleOpen.off();
      return;
    }

    if (isCompleting || !isMounted.current) {
      return;
    }

    toggleCancelling.on();

    await onCancel();

    if (isMounted.current) {
      toggleCancelling.off();
      toggleOpen.off();
    }
  }, [isCompleting, isMounted, onCancel, toggleCancelling, toggleOpen]);

  const handleConfirm = useCallback(async () => {
    if (!onConfirm) {
      toggleOpen.off();
      return;
    }

    if (isCompleting || !isMounted.current) {
      return;
    }

    toggleConfirming.on();

    await onConfirm();

    if (isMounted.current) {
      toggleConfirming.off();
      toggleOpen.off();
    }
  }, [isCompleting, isMounted, onConfirm, toggleConfirming, toggleOpen]);

  return (
    <Modal
      className={className}
      closeOnOutsideClick={!isCompleting}
      closeOnEsc={!isCompleting}
      open={isOpen}
      title={title}
      triggerButton={triggerButton}
      underlayClass={underlayClass}
      onExit={onExit}
      onToggleOpen={toggleOpen}
    >
      {children}

      <ButtonGroup className="govuk-!-margin-top-6">
        {showCancel && (
          <Button
            disabled={isConfirming}
            variant="secondary"
            onClick={handleCancel}
          >
            {cancelText}
          </Button>
        )}

        {!hideConfirm && (
          <Button
            disabled={isCancelling}
            variant={submitButtonVariant}
            onClick={handleConfirm}
          >
            {confirmText}
          </Button>
        )}

        <LoadingSpinner
          alert
          inline
          hideText
          loading={isCompleting}
          size="sm"
          text={isCancelling ? hiddenCancellingText : hiddenConfirmingText}
        />
      </ButtonGroup>
    </Modal>
  );
}
