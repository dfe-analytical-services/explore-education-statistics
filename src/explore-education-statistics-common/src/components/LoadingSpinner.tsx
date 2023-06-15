import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './LoadingSpinner.module.scss';

interface Props {
  alert?: boolean;
  className?: string;
  children?: ReactNode;
  hideText?: boolean;
  text?: string;
  size?: 'sm' | 'md' | 'lg' | 'xl';
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
  size = 'xl',
  text,
}: Props) => {
  return (
    // eslint-disable-next-line react/jsx-no-useless-fragment
    <>
      {loading ? (
        <div
          className={classNames(
            styles.container,
            {
              [styles.noInlineFlex]: !inline,
              [styles.overlay]: overlay,
            },
            className,
          )}
          data-testid="loadingSpinner"
        >
          <div
            className={classNames(styles.spinner, styles[`spinner--${size}`])}
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
