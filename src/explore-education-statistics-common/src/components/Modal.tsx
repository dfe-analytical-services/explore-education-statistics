import { OmitStrict } from '@common/types';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import AriaModal, { AriaModalProps } from 'react-aria-modal';
import styles from './Modal.module.scss';

type Props = {
  children: ReactNode;
  title: string;
} & OmitStrict<
  AriaModalProps,
  'getApplicationNode' | 'verticallyCenter' | 'titleText' | 'titleId'
>;

const Modal = ({ children, dialogClass, title, ...props }: Props) => {
  return (
    <AriaModal
      {...props}
      dialogClass={classNames(styles.dialog, dialogClass)}
      titleText={title}
      verticallyCenter
      getApplicationNode={() => {
        return document.getElementById(process.env.APP_ROOT_ID) as HTMLElement;
      }}
    >
      <h1>{title}</h1>
      {children}
    </AriaModal>
  );
};

export default Modal;
