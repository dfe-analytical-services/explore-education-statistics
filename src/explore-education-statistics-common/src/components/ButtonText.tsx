import React from 'react';
import { ButtonProps } from './Button';
import styles from './ButtonText.module.scss';

const ButtonText = ({ children, ...props }: ButtonProps) => {
  return (
    // eslint-disable-next-line react/button-has-type
    <button {...props} className={styles.button}>
      {children}
    </button>
  );
};

export default ButtonText;
