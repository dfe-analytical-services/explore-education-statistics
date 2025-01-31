import classNames from 'classnames';
import React, { ReactNode, useEffect, useState } from 'react';
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
  const [alertText, setAlertText] = useState('');

  useEffect(() => {
    let timeout: ReturnType<typeof setTimeout>;

    if (!alert || !text) {
      return () => clearTimeout(timeout);
    }

    if (loading) {
      // Set alert text with a delay so that screen readers
      // detect the change and announce it correctly.
      timeout = setTimeout(() => setAlertText(text), 300);
    } else {
      setAlertText('');
    }

    return () => clearTimeout(timeout);
  }, [alert, loading, text]);

  return (
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
            {alert ? alertText : text}
          </span>
        </div>
      ) : (
        children
      )}
    </>
  );
};

export default LoadingSpinner;
