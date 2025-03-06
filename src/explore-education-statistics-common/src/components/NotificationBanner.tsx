import styles from '@common/components/NotificationBanner.module.scss';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children?: ReactNode;
  className?: string;
  fullWidthContent?: boolean;
  heading?: string;
  role?: 'region' | 'alert';
  title: string;
  variant?: 'error' | 'success';
}

const NotificationBanner = ({
  children,
  className,
  fullWidthContent = false,
  heading,
  role = 'region',
  title,
  variant,
}: Props) => {
  return (
    <div
      aria-labelledby="govuk-notification-banner-title"
      className={classNames(
        'govuk-notification-banner govuk-!-margin-bottom-6',
        className,
        {
          [styles.fullWidthContent]: fullWidthContent,
          [styles.error]: variant === 'error',
          'govuk-notification-banner--success': variant === 'success',
        },
      )}
      data-testid="notificationBanner"
      role={role}
    >
      <div className="govuk-notification-banner__header">
        <h2
          className="govuk-notification-banner__title"
          id="govuk-notification-banner-title"
        >
          {title}
        </h2>
      </div>
      <div className="govuk-notification-banner__content">
        {heading && (
          <p className="govuk-notification-banner__heading">{heading}</p>
        )}
        {children}
      </div>
    </div>
  );
};

export default NotificationBanner;
