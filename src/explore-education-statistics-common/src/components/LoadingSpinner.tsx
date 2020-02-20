import classNames from 'classnames';
import React from 'react';
import styles from './LoadingSpinner.module.scss';

interface Props {
  alert?: boolean;
  className?: string;
  hideText?: boolean;
  text?: string;
  size?: number;
  inline?: boolean;
  overlay?: boolean;
}

const LoadingSpinner = ({
  alert = false,
  className,
  hideText = false,
  inline = false,
  overlay = false,
  size = 80,
  text,
}: Props) => {
  return (
    <>
      <div
        className={classNames({
          [styles.noInlineFlex]: size >= 80 || !inline,
          [styles.container]: true,
          [styles.overlay]: overlay,
        })}
      >
        <div
          className={classNames(styles.spinner, className)}
          style={{
            height: `${size}px`,
            width: `${size}px`,
            borderWidth: `${size * 0.15}px`,
          }}
        />

        <span
          role={alert ? 'alert' : undefined}
          className={classNames({ 'govuk-visually-hidden': hideText })}
        >
          {text}
        </span>
      </div>
    </>
  );
};

export default LoadingSpinner;
