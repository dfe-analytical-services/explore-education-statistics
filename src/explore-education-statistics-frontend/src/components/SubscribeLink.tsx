import React from 'react';
import Link from './Link';

export interface SubscribeLinkProps {
  slug: string;
}

const SubscribeLink = ({ slug }: SubscribeLinkProps) => {
  return (
    <Link
      className="govuk-!-display-none-print"
      unvisited
      to={`/subscriptions?slug=${slug}`}
      data-testid={`subsciption-${slug}`}
    >
      Sign up for email alerts
    </Link>
  );
};

export default SubscribeLink;
