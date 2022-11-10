import React, { ReactNode } from 'react';

interface Props {
  children?: ReactNode;
  heading: string;
  title: string;
}

const NotificationBanner = ({ children, heading, title }: Props) => {
  return (
    <div
      aria-labelledby="govuk-notification-banner-title"
      className="govuk-notification-banner govuk-!-margin-bottom-6"
      data-testid="notificationBanner"
      role="region"
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
        <p className="govuk-notification-banner__heading">{heading}</p>
        {children}
      </div>
    </div>
  );
};

export default NotificationBanner;
