import React from 'react';
import Link from './Link';
import printStyles from './PrintCSS.module.scss';

export interface SubscribeLinkProps {
  slug: string;
}

const SubscribeLink = ({ slug }: SubscribeLinkProps) => {
  return (
    <Link
      className={printStyles.hidden}
      unvisited
      to={`/subscriptions?slug=${slug}`}
      data-testid={`subsciption-${slug}`}
    >
      Sign up for email alerts
    </Link>
  );
};

export default SubscribeLink;
