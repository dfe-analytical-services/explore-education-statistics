import classNames from 'classnames';
import React from 'react';
import AriaLiveMessage from './AriaLiveMessage';
import styles from './LoadingSpinner.module.scss';

interface Props {
  className?: string;
  text?: string;
  size?: number;
  inline?: boolean;
  overlay?: boolean;
  screenReaderMessage?: string;
}

const LoadingSpinner = ({
  className,
  text,
  size = 80,
  inline = false,
  overlay = false,
  screenReaderMessage = 'The page is loading.',
}: Props) => {
  return (
    <>
      <div
        className={classNames({
          [styles.container]: true,
          [styles.inline]: inline,
          [styles.overlay]: overlay,
        })}
      >
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
      {screenReaderMessage ||
        (text && <AriaLiveMessage message={screenReaderMessage || text} />)}
    </>
  );
};

export default LoadingSpinner;
