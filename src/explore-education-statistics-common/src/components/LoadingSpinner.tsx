import classNames from 'classnames';
import React from 'react';
import styles from './LoadingSpinner.module.scss';

interface Props {
  className?: string;
  text?: string;
  size?: number;
}

const LoadingSpinner = ({ className, text, size = 80 }: Props) => {
  return (
    <div className={styles.container}>
      {text && <p className={styles.text}>{text}</p>}
      <div
        className={classNames(styles.spinner, className)}
        style={{
          height: `${size}px`,
          width: `${size}px`,
          borderWidth: `${size * 0.15}px`,
        }}
      />
    </div>
  );
};

export default LoadingSpinner;
