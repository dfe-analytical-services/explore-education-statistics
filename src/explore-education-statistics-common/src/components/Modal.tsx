import styles from '@common/components/Modal.module.scss';
import classNames from 'classnames';
import React, { ReactNode, useEffect } from 'react';
import BaseModal from 'react-modal';

export interface ModalProps {
  children: ReactNode;
  className?: string;
  closeOnOutsideClick?: boolean;
  closeOnEsc?: boolean;
  // eslint-disable-next-line react/no-unused-prop-types
  disabled?: boolean;
  fullScreen?: boolean;
  hideTitle?: boolean;
  open?: boolean;
  title: string;
  underlayClass?: string;
  onOpen?: () => void;
  onExit?: () => void;
}

const Modal = ({
  children,
  className,
  closeOnOutsideClick = true,
  closeOnEsc = true,
  fullScreen = false,
  hideTitle = false,
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

  // when fullscreen prevent scroll on the body while the modal is open.
  useEffect(() => {
    if (fullScreen) {
      document.body.style.overflow = 'hidden';
      document.documentElement.style.overflow = 'hidden';
    }

    return () => {
      if (fullScreen) {
        document.body.style.overflow = 'unset';
        document.documentElement.style.overflow = 'unset';
      }
    };
  }, [fullScreen]);

  return (
    <BaseModal
      appElement={appElement}
      ariaHideApp={!!appElement}
      contentLabel={title}
      className={classNames(styles.dialog, className, {
        [styles.fullScreen]: fullScreen,
      })}
      isOpen={open}
      overlayClassName={classNames(styles.underlay, underlayClass)}
      shouldFocusAfterRender
      shouldCloseOnOverlayClick={closeOnOutsideClick}
      shouldCloseOnEsc={closeOnEsc}
      onRequestClose={onExit}
      onAfterOpen={onOpen}
    >
      <h2
        className={classNames('govuk-heading-l', {
          'govuk-visually-hidden': hideTitle,
        })}
      >
        {title}
      </h2>
      {children}
    </BaseModal>
  );
};

export default Modal;
