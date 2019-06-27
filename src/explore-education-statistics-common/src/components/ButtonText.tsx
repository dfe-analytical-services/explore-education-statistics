import classNames from 'classnames';
import React from 'react';
import { ButtonProps } from './Button';
import styles from './ButtonText.module.scss';

interface Props extends ButtonProps {
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
