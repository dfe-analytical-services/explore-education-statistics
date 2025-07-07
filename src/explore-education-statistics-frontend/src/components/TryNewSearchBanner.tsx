import React from 'react';
import NotificationBanner from '@common/components/NotificationBanner';
import Link from '@frontend/components/Link';

const TryNewSearchBanner = () => {
  return (
    <NotificationBanner
      className="govuk-!-margin-top-6"
      fullWidthContent
      title="New improved search"
    >
      <p>
        Try out our{' '}
        <Link to="/find-statistics?azsearch=true">new improved search</Link> and
        give us feedback
      </p>
    </NotificationBanner>
  );
};

export default TryNewSearchBanner;
