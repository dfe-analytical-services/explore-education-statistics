import classNames from 'classnames';
import React, { MouseEventHandler, ReactNode } from 'react';
import styles from './ButtonText.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  id?: string;
  testId?: string;
  onClick?: MouseEventHandler<HTMLButtonElement>;
  type?: 'button' | 'submit' | 'reset';
  underline?: boolean;
  variant?: 'warning';
}

const ButtonText = ({
  children,
  className,
  testId,
  type = 'button',
  underline = true,
  variant,
  ...props
}: Props) => {
  return (
    <button
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...props}
      className={classNames(
        styles.button,
        {
          [styles.noUnderline]: !underline,
          [styles.warning]: variant === 'warning',
        },
        className,
      )}
      data-testid={testId}
      // eslint-disable-next-line react/button-has-type
      type={type}
    >
      {children}
    </button>
  );
};

export default ButtonText;
