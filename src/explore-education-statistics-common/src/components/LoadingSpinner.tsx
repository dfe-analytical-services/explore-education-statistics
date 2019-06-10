import classNames from 'classnames';
import React from 'react';
import styles from './LoadingSpinner.module.scss';

interface Props {
  className?: string;
  text?: string;
}

const LoadingSpinner = ({ className, text }: Props) => {
  return (
    <div className={styles.container}>
      {text && <p className={styles.text}>{text}</p>}
      <div className={classNames(styles.spinner, className)} />
    </div>
  );
};

export default LoadingSpinner;
