import Button from '@common/components/Button';
import styles from '@common/components/Modal.module.scss';
import useToggle from '@common/hooks/useToggle';
import * as Dialog from '@radix-ui/react-dialog';
import classNames from 'classnames';
import React, { ReactNode, useEffect, useRef } from 'react';

export interface ModalProps {
  children: ReactNode;
  className?: string;
  closeOnEsc?: boolean;
  closeOnOutsideClick?: boolean;
  closeText?: string;
  description?: string;
  fullScreen?: boolean;
  hideTitle?: boolean;
  open?: boolean;
  showClose?: boolean;
  title: string;
  triggerButton?: ReactNode;
  underlayClass?: string;
  onExit?: () => void;
  onOpen?: () => void;
  onToggleOpen?: (open: boolean) => void;
}

const Modal = ({
  children,
  className,
  closeOnEsc = true,
  closeOnOutsideClick = true,
  closeText = 'Close',
  description,
  fullScreen = false,
  hideTitle = false,
  open: initialOpen = false,
  showClose = false,
  title,
  triggerButton,
  underlayClass,
  onExit,
  onOpen,
  onToggleOpen,
}: ModalProps) => {
  const dialogRef = useRef<HTMLDivElement>(null);
  const [open, toggleOpen] = useToggle(initialOpen);

  useEffect(() => {
    toggleOpen(initialOpen);
  }, [initialOpen, toggleOpen]);

  return (
    <Dialog.Root
      open={open}
      onOpenChange={isOpen => {
        if (onToggleOpen) {
          onToggleOpen?.(isOpen);
        } else {
          toggleOpen(isOpen);
        }
        return isOpen ? onOpen?.() : onExit?.();
      }}
    >
      {triggerButton && (
        <Dialog.Trigger asChild>{triggerButton}</Dialog.Trigger>
      )}
      <Dialog.Portal>
        <Dialog.Overlay
          className={classNames(styles.underlay, underlayClass)}
          data-testid="modal-underlay"
        >
          <Dialog.Content
            className={classNames(styles.dialog, className, {
              [styles.fullScreen]: fullScreen,
            })}
            ref={dialogRef}
            onEscapeKeyDown={event =>
              !closeOnEsc ? event.preventDefault() : undefined
            }
            onOpenAutoFocus={event => {
              event.preventDefault();
              dialogRef.current?.focus();
            }}
            onPointerDownOutside={event =>
              !closeOnOutsideClick ? event.preventDefault() : undefined
            }
          >
            <Dialog.Title asChild>
              <h2
                className={classNames('govuk-heading-l', {
                  'govuk-visually-hidden': hideTitle,
                })}
              >
                {title}
              </h2>
            </Dialog.Title>
            {description && (
              <Dialog.Description>{description}</Dialog.Description>
            )}
            {children}
            {showClose && (
              <Dialog.Close asChild>
                <Button>{closeText}</Button>
              </Dialog.Close>
            )}
          </Dialog.Content>
        </Dialog.Overlay>
      </Dialog.Portal>
    </Dialog.Root>
  );
};

export default Modal;

export function ModalCloseButton({ children }: { children: ReactNode }) {
  return <Dialog.Close asChild>{children}</Dialog.Close>;
}
