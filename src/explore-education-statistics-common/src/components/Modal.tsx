import styles from '@common/components/Modal.module.scss';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import BaseModal from 'react-modal';

export interface ModalProps {
  children: ReactNode;
  className?: string;
  closeOnOutsideClick?: boolean;
  closeOnEsc?: boolean;
  disabled?: boolean;
  open?: boolean;
  onOpen?: () => void;
  onExit?: () => void;
  title: string;
  underlayClass?: string;
}

const Modal = ({
  children,
  className,
  closeOnOutsideClick = true,
  closeOnEsc = true,
  open = true,
  onOpen,
  onExit,
  title,
  underlayClass,
}: ModalProps) => {
  const appElement =
    typeof document !== 'undefined'
      ? (document.getElementById(process.env.APP_ROOT_ID) as HTMLElement)
      : undefined;

  return (
    <BaseModal
      appElement={appElement}
      ariaHideApp={!!appElement}
      contentLabel={title}
      className={classNames(styles.dialog, className)}
      isOpen={open}
      overlayClassName={classNames(styles.underlay, underlayClass)}
      shouldFocusAfterRender
      shouldCloseOnOverlayClick={closeOnOutsideClick}
      shouldCloseOnEsc={closeOnEsc}
      onRequestClose={onExit}
      onAfterOpen={onOpen}
    >
      <h2 className="govuk-heading-l">{title}</h2>
      {children}
    </BaseModal>
  );
};

export default Modal;
