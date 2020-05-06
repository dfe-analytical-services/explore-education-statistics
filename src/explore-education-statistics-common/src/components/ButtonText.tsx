import classNames from 'classnames';
import React, { MouseEventHandler, ReactNode } from 'react';
import { ButtonProps } from './Button';
import styles from './ButtonText.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  id?: string;
  onClick?: MouseEventHandler<HTMLButtonElement>;
  type?: 'button' | 'submit' | 'reset';
  underline?: boolean;
}

const ButtonText = ({
  children,
  className,
  type = 'button',
  underline = true,
  ...props
}: Props & ButtonProps) => {
  return (
    // eslint-disable-next-line react/button-has-type
    <button
      {...props}
      className={classNames(
        styles.button,
        {
          [styles.noUnderline]: !underline,
        },
        className,
      )}
      type={type}
    >
      {children}
    </button>
  );
};

export default ButtonText;
