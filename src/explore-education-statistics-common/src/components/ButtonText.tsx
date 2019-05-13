import classNames from 'classnames';
import React from 'react';
import { ButtonProps } from './Button';
import styles from './ButtonText.module.scss';

const ButtonText = ({
  children,
  className,
  type = 'button',
  ...props
}: ButtonProps) => {
  return (
    // eslint-disable-next-line react/button-has-type
    <button
      {...props}
      className={classNames(styles.button, className)}
      type={type}
    >
      {children}
    </button>
  );
};

export default ButtonText;
