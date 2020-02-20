import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './LoadingSpinner.module.scss';

interface Props {
  alert?: boolean;
  className?: string;
  children?: ReactNode;
  hideText?: boolean;
  text?: string;
  size?: number;
  inline?: boolean;
  loading?: boolean;
  overlay?: boolean;
}

const LoadingSpinner = ({
  alert = false,
  className,
  children,
  hideText = false,
  inline = false,
  loading = true,
  overlay = false,
  size = 80,
  text,
}: Props) => {
  return (
    <>
      {loading ? (
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
      ) : (
        children
      )}
    </>
  );
};

export default LoadingSpinner;
