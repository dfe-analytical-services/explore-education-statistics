import React from 'react';
import { ButtonProps } from './Button';
import styles from './ButtonText.module.scss';

const ButtonText = (props: ButtonProps) => {
  return (
    <button {...props} className={styles.button}>
      {props.children}
    </button>
  );
};

export default ButtonText;
