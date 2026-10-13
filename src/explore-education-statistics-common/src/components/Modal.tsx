import Button from '@common/components/Button';
import styles from '@common/components/Modal.module.scss';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useMounted from '@common/hooks/useMounted';
import useToggle from '@common/hooks/useToggle';
import * as Dialog from '@radix-ui/react-dialog';
import classNames from 'classnames';
import React, { ReactNode, useEffect, useRef } from 'react';

const defaultCloseText = 'Close';

export interface ModalProps {
  children: ReactNode;
  className?: string;
  closeOnEsc?: boolean;
  closeOnOutsideClick?: boolean;
  closeText?: string;
  description?: string;
  fullScreen?: boolean;
  hideTitle?: boolean;
  /**
   * Until JS has loaded, render the content in a native `<details>` with this
   * summary instead of the modal, so it is available without JS (including to
   * crawlers). Once mounted, it is swapped for the modal and its trigger.
   */
  noJsFallbackSummary?: string;
  open?: boolean;
  showClose?: boolean;
  title: string;
  titleId?: string;
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
  closeText = defaultCloseText,
  description,
  fullScreen = false,
  hideTitle = false,
  noJsFallbackSummary,
  open: initialOpen = false,
  showClose = false,
  title,
  titleId = 'modal-title',
  triggerButton,
  underlayClass,
  onExit,
  onOpen,
  onToggleOpen,
}: ModalProps) => {
  const dialogRef = useRef<HTMLDivElement>(null);
  const [open, toggleOpen] = useToggle(initialOpen);

  const { isMounted } = useMounted();

  useEffect(() => {
    toggleOpen(initialOpen);
  }, [initialOpen, toggleOpen]);

  // Server-rendered and on the first client render, so they match; swapped
  // for the modal once mounted.
  if (noJsFallbackSummary && !isMounted) {
    return (
      <details
        className="govuk-details govuk-!-margin-bottom-0"
        data-testid="modal-no-js-fallback"
      >
        <summary className="govuk-details__summary">
          <span className="govuk-details__summary-text">
            {noJsFallbackSummary}
          </span>
        </summary>
        <div className="govuk-details__text">
          {description && <p>{description}</p>}
          {children}
        </div>
      </details>
    );
  }

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
          className={classNames(
            styles.underlay,
            {
              [styles.noUnderlayClick]: !closeOnOutsideClick,
            },
            underlayClass,
          )}
          data-testid="modal-underlay"
        >
          <Dialog.Content
            aria-describedby={undefined}
            aria-labelledby={titleId}
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
            <Dialog.Title data-testid="modal-title">
              <span
                className={classNames('govuk-heading-l', {
                  'govuk-visually-hidden': hideTitle,
                })}
                id={titleId}
              >
                {title}
              </span>
            </Dialog.Title>
            {description && (
              <Dialog.Description>{description}</Dialog.Description>
            )}
            {children}
            {showClose && (
              <Dialog.Close asChild>
                <Button>
                  {closeText}
                  {closeText === defaultCloseText && (
                    <VisuallyHidden> modal</VisuallyHidden>
                  )}
                </Button>
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
